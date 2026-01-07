using System.Diagnostics.Tracing;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;

// Configure a short, fast benchmark run when args contain "short" or "--fast"
var commandLineArgs = Environment.GetCommandLineArgs();
var isShort = commandLineArgs.Any(a => a.Contains("short", StringComparison.OrdinalIgnoreCase) || a.Equals("--fast", StringComparison.OrdinalIgnoreCase));

var manualConfig = ManualConfig.Create(DefaultConfig.Instance);
// Use an extremely short job to keep runtime low
manualConfig.AddJob(Job.ShortRun
    .WithWarmupCount(1)
    .WithIterationCount(1)
    .WithUnrollFactor(1));

// Attach EventPipe profiler only for full runs on Windows
if (OperatingSystem.IsWindows() && !isShort)
{
    manualConfig.AddDiagnoser(new EventPipeProfiler(EventPipeProfile.CpuSampling));
}

BenchmarkRunner.Run<ChunkGenerationBenchmark>(manualConfig);

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser, ExceptionDiagnoser]
public class ChunkGenerationBenchmark
{
    private string _saveRoot = string.Empty;
    private Tileset _tileset = null!;
    private static TerrainEventCounterListener? _listener;

    [ParamsSource(nameof(MapSizes))]
    public int MapSizeInTiles { get; set; }
    public IEnumerable<int> MapSizes => BenchmarkSettings.MapSizes;

    [ParamsSource(nameof(Strategies))]
    public EntropyStrategy Strategy { get; set; }
    public IEnumerable<EntropyStrategy> Strategies => BenchmarkSettings.Strategies;

    // Time-box per-chunk WFC to keep scenarios comparable
    [ParamsSource(nameof(TimeBudgets))]
    public int TimeBudgetMs { get; set; }
    public IEnumerable<int> TimeBudgets => BenchmarkSettings.TimeBudgets;

    // Toggle WFC vs random fallback to compare approaches
    [ParamsSource(nameof(WfcModes))]
    public bool UseWfc { get; set; }
    public IEnumerable<bool> WfcModes => BenchmarkSettings.WfcModes;

    // Heuristics tie-break toggles
    [ParamsSource(nameof(InfluenceModes))]
    public bool ApplyInfluenceTieBreakForSingleHeuristic { get; set; }
    public IEnumerable<bool> InfluenceModes => BenchmarkSettings.InfluenceModes;

    [ParamsSource(nameof(CenterBiasModes))]
    public bool PreferCentralCellTieBreak { get; set; }
    public IEnumerable<bool> CenterBiasModes => BenchmarkSettings.CenterBiasModes;

    // Heuristics knobs
    [ParamsSource(nameof(UniformFractions))]
    public double UniformPickFraction { get; set; }
    public IEnumerable<double> UniformFractions => BenchmarkSettings.UniformFractions;

    [ParamsSource(nameof(MostConstrainingBiases))]
    public double MostConstrainingBias { get; set; }
    public IEnumerable<double> MostConstrainingBiases => BenchmarkSettings.MostConstrainingBiases;

    [GlobalSetup]
    public void Setup()
    {
        _saveRoot = Path.Combine(Path.GetTempPath(), "TerrainGeneration2DBench");
        if (!Directory.Exists(_saveRoot))
        {
            Directory.CreateDirectory(_saveRoot);
        }

        _tileset = TilesetFactory.CreateMockTileset(tileCount: 16, tileSize: 16);

        // Subscribe to custom EventSource counters during benchmarks
        _listener = new TerrainEventCounterListener();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_saveRoot))
        {
            Directory.Delete(_saveRoot, recursive: true);
        }

        _listener?.Dispose();
    }

    [Benchmark]
    public void GenerateChunkedTerrain()
    {
        _listener?.Reset();
        var outputDir = CreateOutputDir();
        var map = CreateMap(outputDir);
        map.UpdateActiveChunks(new Rectangle(0, 0, MapSizeInTiles * _tileset.TileWidth, MapSizeInTiles * _tileset.TileHeight));
        map.SaveAll();
        _listener?.PrintSummary(CreateLabel("GenerateChunkedTerrain"));
    }

    [Benchmark]
    public void GenerateAndScrollChunks()
    {
        _listener?.Reset();
        var outputDir = CreateOutputDir();
        var map = CreateMap(outputDir);
        map.UpdateActiveChunks(new Rectangle(0, 0, Chunk.ChunkSize * _tileset.TileWidth * 2,
            Chunk.ChunkSize * _tileset.TileHeight * 2));

        var scrollDistance = Chunk.ChunkSize * _tileset.TileWidth;
        for (var step = 1; step <= 8; step++)
        {
            var offset = step * scrollDistance;
            var viewport = new Rectangle(offset, offset, Chunk.ChunkSize * _tileset.TileWidth,
                Chunk.ChunkSize * _tileset.TileHeight);
            map.UpdateActiveChunks(viewport);
        }

        map.SaveAll();
        _listener?.PrintSummary(CreateLabel("GenerateAndScrollChunks"));
    }

    private ChunkedTilemap CreateMap(string outputDir)
    {
        var heuristics = Strategy switch
        {
            EntropyStrategy.Domain => new HeuristicsConfiguration
            {
                UseDomainEntropy = true,
                UseShannonEntropy = false,
                UseMostConstrainingTieBreak = true,
                ApplyInfluenceTieBreakForSingleHeuristic = ApplyInfluenceTieBreakForSingleHeuristic,
                PreferCentralCellTieBreak = PreferCentralCellTieBreak,
                UniformPickFraction = UniformPickFraction,
                MostConstrainingBias = MostConstrainingBias
            },
            EntropyStrategy.Shannon => new HeuristicsConfiguration
            {
                UseDomainEntropy = false,
                UseShannonEntropy = true,
                UseMostConstrainingTieBreak = true,
                ApplyInfluenceTieBreakForSingleHeuristic = ApplyInfluenceTieBreakForSingleHeuristic,
                PreferCentralCellTieBreak = PreferCentralCellTieBreak,
                UniformPickFraction = UniformPickFraction,
                MostConstrainingBias = MostConstrainingBias
            },
            EntropyStrategy.Combined => new HeuristicsConfiguration
            {
                UseDomainEntropy = true,
                UseShannonEntropy = true,
                UseMostConstrainingTieBreak = true,
                ApplyInfluenceTieBreakForSingleHeuristic = ApplyInfluenceTieBreakForSingleHeuristic,
                PreferCentralCellTieBreak = PreferCentralCellTieBreak,
                UniformPickFraction = UniformPickFraction,
                MostConstrainingBias = MostConstrainingBias
            },
            _ => new HeuristicsConfiguration()
        };

        // Note: logger is null in benchmarks to avoid console noise.
        return new ChunkedTilemap(
            _tileset,
            mapSizeInTiles: MapSizeInTiles,
            masterSeed: 42,
            saveDirectory: outputDir,
            useWaveFunctionCollapse: UseWfc,
            terrainRuleConfiguration: null,
            heightMapConfiguration: null,
            weightConfig: null,
            heuristicsConfig: heuristics,
            logger: null,
            wfcTimeBudgetMs: TimeBudgetMs);
    }

    private string CreateOutputDir()
    {
        var name = $"{MapSizeInTiles}_{Strategy}_{TimeBudgetMs}ms_infTie={ApplyInfluenceTieBreakForSingleHeuristic}_center={PreferCentralCellTieBreak}_uniform={UniformPickFraction.ToString(CultureInfo.InvariantCulture)}_bias={MostConstrainingBias.ToString(CultureInfo.InvariantCulture)}_{Guid.NewGuid():N}";
        var dir = Path.Combine(_saveRoot, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private string CreateLabel(string scenario)
    {
        var wfc = UseWfc ? "WFC" : "Random";
        return $"{scenario} size={MapSizeInTiles} strategy={Strategy} budget={TimeBudgetMs}ms mode={wfc} inf-tie={ApplyInfluenceTieBreakForSingleHeuristic} center={PreferCentralCellTieBreak} uniform={UniformPickFraction.ToString(CultureInfo.InvariantCulture)} bias={MostConstrainingBias.ToString(CultureInfo.InvariantCulture)}";
    }
}

public enum EntropyStrategy
{
    Domain,
    Shannon,
    Combined
}

internal static class TilesetFactory
{
    public static Tileset CreateMockTileset(int tileCount, int tileSize)
    {
        var tileset = (Tileset)FormatterServices.GetUninitializedObject(typeof(Tileset));
        SetBackingField(tileset, nameof(Tileset.TileWidth), tileSize);
        SetBackingField(tileset, nameof(Tileset.TileHeight), tileSize);
        SetBackingField(tileset, nameof(Tileset.Columns), tileCount);
        SetBackingField(tileset, nameof(Tileset.Rows), 1);
        SetBackingField(tileset, nameof(Tileset.Count), tileCount);
        SetTilesField(tileset, Array.Empty<TextureRegion>());
        return tileset;
    }

    private static void SetBackingField<T>(Tileset target, string propertyName, T value)
    {
        var field = typeof(Tileset).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException($"Backing field for '{propertyName}' was not found.");
        }

        field.SetValue(target, value);
    }

    private static void SetTilesField(Tileset target, TextureRegion[] values)
    {
        var field = typeof(Tileset).GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException("Tiles field was not found.");
        }

        field.SetValue(target, values);
    }
}

internal sealed class TerrainEventCounterListener : EventListener
{
    private readonly Dictionary<string, (double sum, int count, double max, double last)> _stats = new();
    private const string ProviderName = "JohnLudlow.TerrainGeneration2D.Performance";

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource?.Name == ProviderName)
        {
            EnableEvents(eventSource, EventLevel.Informational, EventKeywords.All, new Dictionary<string, string>
            {
                { "EventCounterIntervalSec", "1" }
            });
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData == null || eventData.EventName != "EventCounters" || eventData.Payload == null || eventData.Payload.Count == 0)
        {
            return;
        }

        var payload = eventData.Payload[0] as IDictionary<string, object>;
        if (payload == null || !payload.TryGetValue("Name", out var nameObj))
        {
            return;
        }

        var name = nameObj as string;
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        double value = 0;
        if (payload.TryGetValue("Mean", out var meanObj) && meanObj is double mean)
        {
            value = mean;
        }
        else if (payload.TryGetValue("Increment", out var incObj) && incObj is double inc)
        {
            value = inc;
        }

        if (_stats.TryGetValue(name, out var s))
        {
            s.sum += value;
            s.count++;
            s.max = Math.Max(s.max, value);
            s.last = value;
            _stats[name] = s;
        }
        else
        {
            _stats[name] = (value, 1, value, value);
        }
    }

    public void Reset()
    {
        _stats.Clear();
    }

    private (double? last, double? avg, double? max) GetStats(string name)
    {
        if (_stats.TryGetValue(name, out var s) && s.count > 0)
        {
            return (s.last, s.sum / s.count, s.max);
        }
        return (null, null, null);
    }

    public void PrintSummary(string label)
    {
        var (last, avg, max) = GetStats("active-chunk-count");
        var saved = GetStats("chunks-saved-per-second");
                
        Console.WriteLine($"[Counters] {label} | ActiveChunks last={
          last?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        } avg={
          avg?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        } max={
          max?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        }; Saved/s last={
          saved.last?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        } avg={
          saved.avg?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        } max={
          saved.max?.ToString("F2", CultureInfo.InvariantCulture) ?? "-"
        }");
    }
}

internal static class BenchmarkSettings
{
    private static readonly string[] _args = Environment.GetCommandLineArgs();
    private static readonly bool _isShort = _args.Any(a => a.Contains("short", StringComparison.OrdinalIgnoreCase) || a.Equals("--fast", StringComparison.OrdinalIgnoreCase));

    private static int? ParseInt(string key)
        => TryGetOverride(key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : (int?)null;
    private static bool? ParseBool(string key)
        => TryGetOverride(key, out var v) && bool.TryParse(v, out var b) ? b : (bool?)null;
    private static double? ParseDouble(string key)
        => TryGetOverride(key, out var v) && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
    private static EntropyStrategy? ParseStrategy(string key)
        => TryGetOverride(key, out var v) && Enum.TryParse<EntropyStrategy>(v, true, out var s) ? s : (EntropyStrategy?)null;

    private static bool TryGetOverride(string key, out string value)
    {
        value = string.Empty;
        for (int i = 0; i < _args.Length - 1; i++)
        {
            if (_args[i].Equals("--" + key, StringComparison.OrdinalIgnoreCase))
            {
                value = _args[i + 1];
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<int> MapSizes
        => ParseInt("size") is int s ? new[] { s } : (_isShort ? new[] { 512 } : new[] { 512, 2048 });
    public static IEnumerable<EntropyStrategy> Strategies
        => ParseStrategy("strategy") is EntropyStrategy st ? new[] { st } : (_isShort ? new[] { EntropyStrategy.Domain } : new[] { EntropyStrategy.Domain, EntropyStrategy.Shannon, EntropyStrategy.Combined });
    public static IEnumerable<int> TimeBudgets
        => ParseInt("budget") is int b ? new[] { b } : (_isShort ? new[] { 50 } : new[] { 50, 100 });
    public static IEnumerable<bool> WfcModes
        => ParseBool("wfc") is bool w ? new[] { w } : (_isShort ? new[] { true } : new[] { true, false });
    public static IEnumerable<bool> InfluenceModes
        => ParseBool("influenceSingle") is bool inf ? new[] { inf } : (_isShort ? new[] { false } : new[] { true, false });
    public static IEnumerable<bool> CenterBiasModes
        => ParseBool("centerBias") is bool c ? new[] { c } : (_isShort ? new[] { false } : new[] { false, true });
    public static IEnumerable<double> UniformFractions
        => ParseDouble("uniform") is double u ? new[] { u } : (_isShort ? new[] { 0.0 } : new[] { 0.0, 0.25 });
    public static IEnumerable<double> MostConstrainingBiases
        => ParseDouble("bias") is double m ? new[] { m } : (_isShort ? new[] { 0.0 } : new[] { 0.0, 0.5 });
}
