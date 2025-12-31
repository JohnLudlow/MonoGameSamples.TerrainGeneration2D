namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class OceanTileType : TileType
{
  public OceanTileType(int tileId)
      : base(tileId, "Ocean")
  {
  }

  public override bool EvaluateRules(TileRuleContext context)
  {
    return MatchesNeighbor(context, TerrainTileIds.Void, TerrainTileIds.Ocean, TerrainTileIds.Beach);
  }
}
