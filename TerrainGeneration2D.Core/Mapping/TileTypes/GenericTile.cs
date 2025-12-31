namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class GenericTileType : TileType
{
  public GenericTileType(int tileId)
      : base(tileId, $"Tile {tileId}")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    return false;
  }
}
