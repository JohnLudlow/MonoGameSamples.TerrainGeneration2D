using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Benchmarks;

public enum EntropyStrategy
{
  Domain,
  Shannon,
  Combined
}


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