namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class NullTileType : TileType
{
  public NullTileType(int tileId)
      : base(tileId, "Void")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    return true;
  }
}
