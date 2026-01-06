namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class SnowTileType : TileType
{
  public SnowTileType(int tileId)
      : base(tileId, "Snow")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    if (context.CandidateHeight.Altitude < context.Config.SnowHeightMin)
    {
      return false;
    }

    return MatchesNeighbor(context, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}
