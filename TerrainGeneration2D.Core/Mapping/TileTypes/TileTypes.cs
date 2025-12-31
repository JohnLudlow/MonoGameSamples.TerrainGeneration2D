using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

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

public readonly record struct TileRuleContext(
    TilePoint CandidatePosition,
    int CandidateTileId,
    TilePoint NeighborPosition,
    int NeighborTileId,
    Direction DirectionToNeighbor,
    MappingInformationService MappingService)
{
  public GroupMetrics GetCandidateGroupMetrics() => MappingService.GetGroupMetrics(CandidatePosition, CandidateTileId);
  public GroupMetrics GetNeighborGroupMetrics() => MappingService.GetGroupMetrics(NeighborPosition);
}

public sealed class TerrainRuleConfiguration
{
  public int MountainRangeMin { get; init; } = 8;
  public int MountainRangeMax { get; init; } = 48;
  public int MountainWidthMax { get; init; } = 12;
  public int MountainWidthMin { get; init; } = 3;
  public int BeachOceanSizeMin { get; init; } = 12;
  public int BeachOceanSizeMax { get; init; } = 180;
  public int BeachPlainsSizeMin { get; init; } = 20;
  public int BeachPlainsSizeMax { get; init; } = 400;
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

public sealed class TileTypeRegistry
{
  private readonly Dictionary<int, TileType> _tileTypes;

  public TileTypeRegistry(IEnumerable<TileType> tileTypes)
  {
    _tileTypes = tileTypes.ToDictionary(t => t.TileId);
  }

  public TileType GetTileType(int tileId)
  {
    if (!_tileTypes.TryGetValue(tileId, out var tileType))
    {
      throw new InvalidOperationException($"Tile type {tileId} is not registered.");
    }

    return tileType;
  }

  public int TileCount => _tileTypes.Count;

  public static TileTypeRegistry CreateDefault(int tileCount, TerrainRuleConfiguration? config = null)
  {
    config ??= new TerrainRuleConfiguration();
    var tileTypes = new List<TileType>();

    if (tileCount > TerrainTileIds.Void)
    {
      tileTypes.Add(new NullTileType(TerrainTileIds.Void));
    }

    if (tileCount > TerrainTileIds.Ocean)
    {
      tileTypes.Add(new OceanTileType(TerrainTileIds.Ocean));
    }

    if (tileCount > TerrainTileIds.Beach)
    {
      tileTypes.Add(new BeachTileType(TerrainTileIds.Beach, config));
    }

    if (tileCount > TerrainTileIds.Plains)
    {
      tileTypes.Add(new PlainsTileType(TerrainTileIds.Plains));
    }

    if (tileCount > TerrainTileIds.Forest)
    {
      tileTypes.Add(new ForestTileType(TerrainTileIds.Forest));
    }

    if (tileCount > TerrainTileIds.Snow)
    {
      tileTypes.Add(new SnowTileType(TerrainTileIds.Snow));
    }

    if (tileCount > TerrainTileIds.Mountain)
    {
      tileTypes.Add(new MountainTileType(TerrainTileIds.Mountain, config));
    }

    for (int tileId = TerrainTileIds.Mountain + 1; tileId < tileCount; tileId++)
    {
      tileTypes.Add(new GenericTileType(tileId));
    }

    return new TileTypeRegistry(tileTypes);
  }
}
