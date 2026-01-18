namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class PlainsTileType : TileType
{
  public PlainsTileType(int tileId)
      : base(tileId, "Plains")
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
    // If no rule, allow by default (or you may choose to return false)
    return MatchesNeighbor(context, TerrainTileIds.Beach, TerrainTileIds.Plains, TerrainTileIds.Forest);
  }
}