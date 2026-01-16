namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class NullTileType : TileType
{
  public NullTileType(int tileId = 0)
      : base(tileId, "Void")
  {
  }

  public static NullTileType Instance { get; } = new();

  public override bool EvaluateRules(TileRuleContext context)
  {
    return false;
  }
}