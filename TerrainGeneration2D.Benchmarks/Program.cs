using System.Diagnostics.Tracing;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;

// Attach EventPipe profiler on Windows to capture runtime profiling data
var config = DefaultConfig.Instance;
if (OperatingSystem.IsWindows())
{
    var manual = ManualConfig.Create(config);
    manual.AddDiagnoser(new EventPipeProfiler(EventPipeProfile.CpuSampling));
    config = manual;
}

BenchmarkRunner.Run<ChunkGenerationBenchmark>(config);

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser, ExceptionDiagnoser]
public class ChunkGenerationBenchmark
{
    private string _saveRoot = string.Empty;
    private Tileset _tileset = null!;
    private static TerrainEventCounterListener? _listener;

    [Params(512, 1024, 2048)]
    public int MapSizeInTiles { get; set; }

    [Params(EntropyStrategy.Domain, EntropyStrategy.Shannon, EntropyStrategy.Combined)]
    public EntropyStrategy Strategy { get; set; }

    // Time-box per-chunk WFC to keep scenarios comparable
    [Params(20, 50, 100)]
    public int TimeBudgetMs { get; set; }

    // Toggle WFC vs random fallback to compare approaches
    [Params(true, false)]
    public bool UseWfc { get; set; }

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
                UseMostConstrainingTieBreak = true
            },
            EntropyStrategy.Shannon => new HeuristicsConfiguration
            {
                UseDomainEntropy = false,
                UseShannonEntropy = true,
                UseMostConstrainingTieBreak = true
            },
            EntropyStrategy.Combined => new HeuristicsConfiguration
            {
                UseDomainEntropy = true,
                UseShannonEntropy = true,
                UseMostConstrainingTieBreak = true
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
        var name = $"{MapSizeInTiles}_{Strategy}_{TimeBudgetMs}ms_{Guid.NewGuid():N}";
        var dir = Path.Combine(_saveRoot, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private string CreateLabel(string scenario)
    {
        var wfc = UseWfc ? "WFC" : "Random";
        return $"{scenario} size={MapSizeInTiles} strategy={Strategy} budget={TimeBudgetMs}ms mode={wfc}";
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
