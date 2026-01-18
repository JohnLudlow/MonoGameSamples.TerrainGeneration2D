using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

public readonly record struct ResourceRuleContext(
    TilePoint CandidatePosition,
    int CandidateTileId,
    TilePoint NeighborPosition,
    int NeighborTileId,
    Direction DirectionToNeighbor,
    TileTypeRuleConfiguration Config,
    HeightSample CandidateHeight,
    HeightSample NeighborHeight,
    MappingInformationService MappingService)
{
  public GroupMetrics GetCandidateResourceMetrics() => MappingService.GetGroupMetrics(CandidatePosition, CandidateTileId);
  public GroupMetrics GetNeighborResourceMetrics() => MappingService.GetGroupMetrics(NeighborPosition);
}
