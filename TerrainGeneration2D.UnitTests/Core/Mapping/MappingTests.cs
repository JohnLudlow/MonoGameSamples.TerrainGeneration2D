using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UnitTests.Core.Mapping;

public class MappingTests
{
  [Fact]
  public void MappingInformationService_ReturnsCorrectGroupMetrics()
  {
    var output = new int[][]
    {
      [TerrainTileIds.Ocean, TerrainTileIds.Ocean, TerrainTileIds.Beach, TerrainTileIds.Beach],
      [TerrainTileIds.Ocean, TerrainTileIds.Ocean, TerrainTileIds.Beach, TerrainTileIds.Beach],
      [TerrainTileIds.Plains, TerrainTileIds.Plains, TerrainTileIds.Plains, TerrainTileIds.Forest],
      [TerrainTileIds.Plains, TerrainTileIds.Plains, TerrainTileIds.Plains, TerrainTileIds.Forest]
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
    var config = new TileTypeRuleConfiguration([
      new GroupRuleConfiguration
      {
        Id = TerrainTileIds.Beach,
        MinGroupSizeX = 1,
        MaxGroupSizeX = 1,
        ElevationMin = 0.0f,
        ElevationMax = 1.0f
      }]
    );
    var registry = TileTypeRegistry.CreateDefault(7, config);

    var output = new int[2][]
    {
      [TerrainTileIds.Beach, TerrainTileIds.Void],
      [TerrainTileIds.Ocean, TerrainTileIds.Void]
    };

    var mapping = new MappingInformationService(output);

    var context = new TileRuleContext(
      new TilePoint(0, 0), // CandidatePosition
      TerrainTileIds.Beach, // CandidateTileId
      new TilePoint(1, 0), // NeighborPosition
      TerrainTileIds.Ocean, // NeighborTileId
      Direction.East,      // DirectionToNeighbor
      config,              // TileTypeRuleConfiguration
      new HeightSample(0.4f, 0.0f, 0.0f), // CandidateHeight
      new HeightSample(0.4f, 0.0f, 0.0f), // NeighborHeight
      mapping              // MappingInformationService
    );

    var tileType = registry.GetTileType(TerrainTileIds.Beach);
    Assert.True(tileType.EvaluateRules(context));
  }
}