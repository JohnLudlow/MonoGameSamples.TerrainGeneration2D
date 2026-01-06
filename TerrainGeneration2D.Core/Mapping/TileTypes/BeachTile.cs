namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class BeachTileType : TileType
{
  private readonly TerrainRuleConfiguration _config;

  public BeachTileType(int tileId, TerrainRuleConfiguration config)
      : base(tileId, "Beach")
  {
    _config = config;
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var altitude = context.CandidateHeight.Altitude;
    if (altitude < context.Config.BeachHeightMin || altitude > context.Config.BeachHeightMax)
    {
      return false;
    }

    if (context.NeighborTileId == TerrainTileIds.Ocean)
    {
      var metrics = context.GetNeighborGroupMetrics();
      return !metrics.IsValid || IsWithin(metrics.Count, _config.BeachOceanSizeMin, _config.BeachOceanSizeMax);
    }

    if (context.NeighborTileId == TerrainTileIds.Plains)
    {
      var metrics = context.GetNeighborGroupMetrics();
      return !metrics.IsValid || IsWithin(metrics.Count, _config.BeachPlainsSizeMin, _config.BeachPlainsSizeMax);
    }

    return false;
  }

  private static bool IsWithin(int value, int min, int max)
  {
    return value >= min && value <= max;
  }
}
