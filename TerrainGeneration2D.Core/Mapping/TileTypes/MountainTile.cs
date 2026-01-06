namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class MountainTileType : TileType
{
  private readonly TerrainRuleConfiguration _config;

  public MountainTileType(int tileId, TerrainRuleConfiguration config)
      : base(tileId, "Mountain")
  {
    _config = config;
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var altitude = context.CandidateHeight.Altitude;
    if (altitude < context.Config.MountainHeightMin || context.CandidateHeight.MountainNoise < context.Config.MountainNoiseThreshold)
    {
      return false;
    }

    if (!MatchesNeighbor(context, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain))
    {
      return false;
    }

    var candidateMetrics = context.GetCandidateGroupMetrics();
    var neighborMetrics = context.GetNeighborGroupMetrics();

    if (candidateMetrics.MaxDimension > _config.MountainWidthMax)
    {
      return false;
    }

    if (candidateMetrics.Count > _config.MountainRangeMax)
    {
      return false;
    }

    if (neighborMetrics.IsValid && neighborMetrics.Count > _config.MountainRangeMax)
    {
      return false;
    }

    var combinedCount = candidateMetrics.Count + neighborMetrics.Count;
    if (combinedCount > _config.MountainRangeMax)
    {
      return false;
    }

    if (candidateMetrics.Count > 0 && candidateMetrics.Count < _config.MountainRangeMin &&
        neighborMetrics.Count + candidateMetrics.Count < _config.MountainRangeMin)
    {
      // Allow small seeds to grow until they reach the minimum requirement
      return true;
    }

    return true;
  }
}
