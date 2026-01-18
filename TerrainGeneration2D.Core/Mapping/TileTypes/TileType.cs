using System;
using System.Linq;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public static class TerrainTileIds
{
  public const int Void = 0;
  public const int Ocean = 1;
  public const int Beach = 2;
  public const int Plains = 3;
  public const int Forest = 4;
  public const int Snow = 5;
  public const int Mountain = 6;
}

public enum Direction
{
  North,
  South,
  East,
  West
}

public abstract class TileType
{
  protected TileType(int tileId, string name)
  {
    TileId = tileId;
    Name = name;
  }

  public int TileId { get; }
  public string Name { get; }

  public abstract bool EvaluateRules(TileRuleContext context);

  protected static bool MatchesNeighbor(TileRuleContext context, params int[] allowed)
  {
    return allowed.Contains(context.NeighborTileId);
  }
}
