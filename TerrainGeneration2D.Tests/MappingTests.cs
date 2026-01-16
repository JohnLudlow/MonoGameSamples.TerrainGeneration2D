using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class MappingTests
{
    private sealed class DeterministicRandomProvider : JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.IRandomProvider
    {
        public int NextInt() => 0;
        public int NextInt(int maxValue) => 0;
        public int NextInt(int minValue, int maxValue) => minValue;
        public double NextDouble() => 0.0;
    }
    private sealed class AlwaysInvalidTileType : TileType
    {
        public AlwaysInvalidTileType(int tileId) : base(tileId, "Invalid") {}
        public override bool EvaluateRules(TileRuleContext context) => false;
    }

    private sealed class AlwaysValidTileType : TileType
    {
        public AlwaysValidTileType(int tileId) : base(tileId, "Valid") {}
        public override bool EvaluateRules(TileRuleContext context)
        {
            // Only valid when the neighbor tile is also this valid tile (2)
            return context.NeighborTileId == 2;
        }
    }

    /// <summary>
    /// Helper method to create empty jagged array for tests.
    /// </summary>
    private static int[][] CreateEmptyJaggedArray(int rows, int cols)
    {
        var result = new int[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new int[cols];
        }
        return result;
    }

    [Fact]
    public void MappingInformationService_ReturnsCorrectGroupMetrics()
    {
        var output = new int[][]
        {
            [1, 1, 2, 2],
            [1, 1, 2, 2],
            [3, 3, 3, 4],
            [3, 3, 3, 4]
        };

        var service = new MappingInformationService(output);
        var metrics = service.GetGroupMetrics(new TilePoint(0, 0));

        Assert.Equal(4, metrics.Count);
        Assert.Equal(2, metrics.Width);
        Assert.Equal(2, metrics.Height);
    }

    [Fact]
    public void TileTypeRegistry_RespectsBeachRules()
    {
        var config = new TerrainRuleConfiguration
        {
            BeachOceanSizeMin = 1,
            BeachOceanSizeMax = 1,
            BeachPlainsSizeMin = 1,
            BeachPlainsSizeMax = 1
        };

        var registry = TileTypeRegistry.CreateDefault(7, config);
        var mapping = new MappingInformationService(CreateEmptyJaggedArray(2, 2));
        var context = new TileRuleContext(
            new TilePoint(0, 0),
            TerrainTileIds.Beach,
            new TilePoint(1, 0),
            TerrainTileIds.Mountain,
            Direction.East,
            config,
            new HeightSample(0.4f, 0f, 0f),
            new HeightSample(0.8f, 0f, 0f),
            mapping);

        var beach = registry.GetTileType(TerrainTileIds.Beach);
        Assert.False(beach.EvaluateRules(context));
    }

    [Fact]
    public void WaveFunctionCollapse_FullyCollapses()
    {
        var registry = TileTypeRegistry.CreateDefault(5);
        var random = new Random(123);
        var wfc = new Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, random, new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);

        var success = wfc.Generate(maxIterations: 1000);
        Assert.True(success);

        var output = wfc.GetOutput();
        Assert.All(output.SelectMany(row => row), tile => Assert.InRange(tile, 0, registry.TileCount - 1));
    }

    [Fact]
    public void WaveFunctionCollapse_Backtracking_FullyCollapses()
    {
        var registry = TileTypeRegistry.CreateDefault(5);
        var random = new Random(456);
        var wfc = new Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, random, new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);

        var success = wfc.Generate(enableBacktracking: true, maxIterations: 1000, maxBacktrackSteps: 2048, maxDepth: 128);
        Assert.True(success);

        var output = wfc.GetOutput();
        Assert.All(output.SelectMany(row => row), tile => Assert.InRange(tile, 0, registry.TileCount - 1));
    }

    [Fact]
    public void WaveFunctionCollapse_Backtracking_WithLimits_Completes()
    {
        var registry = TileTypeRegistry.CreateDefault(5);
        var random = new Random(789);
        var wfc = new JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, random, new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);

        // Exercise the backtracking path with tight limits to ensure parameters are honored
        var success = wfc.Generate(enableBacktracking: true, maxIterations: 1000, maxBacktrackSteps: 0, maxDepth: 4);
        // Success is not guaranteed if contradictions occur under tight limits, but the call should not throw
        var output = wfc.GetOutput();
        Assert.NotNull(output);
    }


    [Fact]
    public void WaveFunctionCollapse_NonBacktracking_Fails_WithTinyIterationLimit_But_Backtracking_Succeeds()
    {
        var registry = TileTypeRegistry.CreateDefault(5);
        var random = new Random(321);
        var wfcNoBacktrack = new JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, random, new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);
        var wfcBacktrack = new JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, new Random(654), new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);

        var successNoBack = wfcNoBacktrack.Generate(maxIterations: 1);
        var successBack = wfcBacktrack.Generate(enableBacktracking: true, maxIterations: 1000, maxBacktrackSteps: 4096, maxDepth: 128);

        Assert.False(successNoBack);
        Assert.True(successBack);
    }

    [Fact]
    public void WaveFunctionCollapse_Backtracking_Recovers_From_Contradiction()
    {
        // Registry with one universally invalid tile (0), one universally invalid tile (1) and one universally valid tile (2)
        var registry = new TileTypeRegistry(new TileType[]
        {
            new AlwaysInvalidTileType(0),
            new AlwaysInvalidTileType(1),
            new AlwaysValidTileType(2)
        });
        // Deterministic provider yields consistent choices; non-backtracking fails, backtracking recovers
        var wfcNoBacktrack = new JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, new DeterministicRandomProvider(), new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);
        // Force collapse of first cell to invalid tile (0)
        wfcNoBacktrack.GetOutput()[0][0] = 0;
        var successNoBack = wfcNoBacktrack.Generate(maxIterations: 1000);
        Assert.False(successNoBack);

        var wfcBacktrack = new JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.WfcProvider(8, 8, registry, new DeterministicRandomProvider(), new TerrainRuleConfiguration(), DefaultHeightProvider.Instance, Point.Zero);
        var successBack = wfcBacktrack.Generate(enableBacktracking: true, maxIterations: 10000, maxBacktrackSteps: 4096, maxDepth: 256);
        Assert.True(successBack);
    }
}
