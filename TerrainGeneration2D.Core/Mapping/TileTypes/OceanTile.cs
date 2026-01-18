namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class OceanTileType : TileType
{
  public OceanTileType(int tileId)
      : base(tileId, "Ocean")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var rule = context.Config.GetRuleForType(TileId);
    if (rule != null && context.CandidateHeight.Altitude > rule.ElevationMax)
    {
      return false;
    }
    return MatchesNeighbor(context, TerrainTileIds.Void, TerrainTileIds.Ocean, TerrainTileIds.Beach);
  }
}