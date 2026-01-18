namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class MountainTileType : TileType
{
  public MountainTileType(int tileId)
      : base(tileId, "Mountain")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var rule = context.Config.GetRuleForType(TileId);
    var altitude = context.CandidateHeight.Altitude;
    var noise = context.CandidateHeight.MountainNoise;
    if (rule != null)
    {
      if (altitude < rule.ElevationMin || (rule.NoiseThreshold.HasValue && noise < rule.NoiseThreshold.Value))
      {
        return false;
      }
    }
    if (!MatchesNeighbor(context, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain))
    {
      return false;
    }
    var candidateMetrics = context.GetCandidateGroupMetrics();
    var neighborMetrics = context.GetNeighborGroupMetrics();
    if (rule != null)
    {
      if (candidateMetrics.MaxDimension > rule.MaxGroupSizeX)
      {
        return false;
      }
      if (candidateMetrics.Count > rule.MaxGroupSizeY)
      {
        return false;
      }
      if (neighborMetrics.IsValid && neighborMetrics.Count > rule.MaxGroupSizeY)
      {
        return false;
      }
      var combinedCount = candidateMetrics.Count + neighborMetrics.Count;
      if (combinedCount > rule.MaxGroupSizeY)
      {
        return false;
      }
      if (candidateMetrics.Count > 0 && candidateMetrics.Count < rule.MinGroupSizeX &&
          neighborMetrics.Count + candidateMetrics.Count < rule.MinGroupSizeX)
      {
        // Allow small seeds to grow until they reach the minimum requirement
        return true;
      }
    }
    return true;
  }
}