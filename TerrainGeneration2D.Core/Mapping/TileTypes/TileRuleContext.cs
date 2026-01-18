using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public readonly record struct TileRuleContext(
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
  public GroupMetrics GetCandidateGroupMetrics() => MappingService.GetGroupMetrics(CandidatePosition, CandidateTileId);
  public GroupMetrics GetNeighborGroupMetrics() => MappingService.GetGroupMetrics(NeighborPosition);
}
