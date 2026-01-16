using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

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
    TerrainRuleConfiguration Config,
    HeightSample CandidateHeight,
    HeightSample NeighborHeight,
    MappingInformationService MappingService)
{
  public GroupMetrics GetCandidateGroupMetrics() => MappingService.GetGroupMetrics(CandidatePosition, CandidateTileId);
  public GroupMetrics GetNeighborGroupMetrics() => MappingService.GetGroupMetrics(NeighborPosition);
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
  private readonly List<int> _tileOrder;
  private readonly List<int> _validTileIds;

  public TileTypeRegistry(IEnumerable<TileType> tileTypes)
  {
    _tileTypes = tileTypes.ToDictionary(t => t.TileId);
    _tileOrder = [.. tileTypes.Select(t => t.TileId)];
    _validTileIds = _tileOrder.Where(id => id != TerrainTileIds.Void).ToList();
  }

  public TileType GetTileType(int tileId)
  {
    if (!_tileTypes.TryGetValue(tileId, out var tileType))
    {
      throw new InvalidOperationException($"Tile type {tileId} is not registered.");
    }

    return tileType;
  }

  public int TileCount => _tileOrder.Count;

  public IReadOnlyList<int> TileIds => _tileOrder;
  public IReadOnlyList<int> ValidTileIds => _validTileIds;

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

    for (var tileId = TerrainTileIds.Mountain + 1; tileId < tileCount; tileId++)
    {
      tileTypes.Add(new GenericTileType(tileId));
    }

    return new TileTypeRegistry(tileTypes);
  }
}