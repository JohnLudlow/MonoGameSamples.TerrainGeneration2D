using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class MappingTests
{
    [Fact]
    public void MappingInformationService_ReturnsCorrectGroupMetrics()
    {
        var output = new int[4, 4]
        {
            { 1, 1, 2, 2 },
            { 1, 1, 2, 2 },
            { 3, 3, 3, 4 },
            { 3, 3, 3, 4 }
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
        var mapping = new MappingInformationService(new int[2, 2]);
        var context = new TileRuleContext(
            new TilePoint(0, 0),
            TerrainTileIds.Beach,
            new TilePoint(1, 0),
            TerrainTileIds.Mountain,
            Direction.East,
            mapping);

        var beach = registry.GetTileType(TerrainTileIds.Beach);
        Assert.False(beach.EvaluateRules(context));
    }

    [Fact]
    public void WaveFunctionCollapse_FullyCollapses()
    {
        var registry = TileTypeRegistry.CreateDefault(5);
        var random = new Random(123);
        var wfc = new WaveFunctionCollapse(8, 8, registry, random);

        bool success = wfc.Generate(maxIterations: 1000);
        Assert.True(success);

        var output = wfc.GetOutput();
        Assert.All(output.Cast<int>(), tile => Assert.InRange(tile, 0, registry.TileCount - 1));
    }
}
