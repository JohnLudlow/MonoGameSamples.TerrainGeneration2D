namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class SnowTileType : TileType
{
  public SnowTileType(int tileId)
      : base(tileId, "Snow")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    return MatchesNeighbor(context, TerrainTileIds.Forest, TerrainTileIds.Snow, TerrainTileIds.Mountain);
  }
}
