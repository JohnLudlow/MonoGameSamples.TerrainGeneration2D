namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class OceanTileType : TileType
{
  public OceanTileType(int tileId)
      : base(tileId, "Ocean")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    if (context.CandidateHeight.Altitude > context.Config.OceanHeightMax)
    {
      return false;
    }

    return MatchesNeighbor(context, TerrainTileIds.Void, TerrainTileIds.Ocean, TerrainTileIds.Beach);
  }
}
