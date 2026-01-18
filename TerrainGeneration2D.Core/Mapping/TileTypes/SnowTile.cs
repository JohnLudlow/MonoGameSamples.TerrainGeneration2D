namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class SnowTileType : TileType
{
  public SnowTileType(int tileId)
      : base(tileId, "Snow")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var rule = context.Config.GetRuleForType(TileId);
    if (rule != null && context.CandidateHeight.Altitude < rule.ElevationMin)
    {
      return false;
    }
    return MatchesNeighbor(context, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}