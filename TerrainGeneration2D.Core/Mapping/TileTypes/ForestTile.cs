namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class ForestTileType : TileType
{
  public ForestTileType(int tileId)
      : base(tileId, "Forest")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var rule = context.Config.GetRuleForType(TileId);
    var altitude = context.CandidateHeight.Altitude;
    if (rule != null)
    {
      if (altitude < rule.ElevationMin || altitude > rule.ElevationMax)
      {
        return false;
      }
    }
    return MatchesNeighbor(context, TerrainTileIds.Plains, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}