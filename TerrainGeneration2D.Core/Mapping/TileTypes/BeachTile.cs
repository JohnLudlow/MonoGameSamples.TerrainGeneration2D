namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class BeachTileType : TileType
{
  public BeachTileType(int tileId)
      : base(tileId, "Beach")
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
    // Neighbor group size checks
    if (context.NeighborTileId == TerrainTileIds.Ocean && rule != null)
    {
      var metrics = context.GetNeighborGroupMetrics();
      return !metrics.IsValid || IsWithin(metrics.Count, rule.MinGroupSizeX, rule.MaxGroupSizeX);
    }
    if (context.NeighborTileId == TerrainTileIds.Plains && rule != null)
    {
      var metrics = context.GetNeighborGroupMetrics();
      return !metrics.IsValid || IsWithin(metrics.Count, rule.MinGroupSizeY, rule.MaxGroupSizeY);
    }
    return false;
  }

  private static bool IsWithin(int value, int min, int max)
  {
    return value >= min && value <= max;
  }
}