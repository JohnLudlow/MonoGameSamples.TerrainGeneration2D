namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class ForestTileType : TileType
{
  public ForestTileType(int tileId)
      : base(tileId, "Forest")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    return MatchesNeighbor(context, TerrainTileIds.Plains, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}
