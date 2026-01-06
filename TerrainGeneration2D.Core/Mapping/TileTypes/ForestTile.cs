namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class ForestTileType : TileType
{
  public ForestTileType(int tileId)
      : base(tileId, "Forest")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var altitude = context.CandidateHeight.Altitude;
    if (altitude < context.Config.ForestHeightMin || altitude > context.Config.ForestHeightMax)
    {
      return false;
    }

    return MatchesNeighbor(context, TerrainTileIds.Plains, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}
