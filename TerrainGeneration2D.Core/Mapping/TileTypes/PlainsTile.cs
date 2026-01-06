namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class PlainsTileType : TileType
{
  public PlainsTileType(int tileId)
      : base(tileId, "Plains")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    var altitude = context.CandidateHeight.Altitude;
    if (altitude < context.Config.PlainsHeightMin || altitude > context.Config.PlainsHeightMax)
    {
      return false;
    }

    return MatchesNeighbor(context, TerrainTileIds.Beach, TerrainTileIds.Plains, TerrainTileIds.Forest);
  }
}
