namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class GenericTileType : TileType
{
  public GenericTileType(int tileId)
      : base(tileId, $"Tile {tileId}")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    // Placeholder tiles never pass by default; subclass or override to reuse the sprite.
    return false;
  }
}