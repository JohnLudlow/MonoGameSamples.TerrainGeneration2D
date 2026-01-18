using System;
using System.Collections.Generic;
using System.Linq;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

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

  public static TileTypeRegistry CreateDefault(int tileCount, TileTypeRuleConfiguration? config = null)
  {
    config ??= new TileTypeRuleConfiguration();
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
      tileTypes.Add(new BeachTileType(TerrainTileIds.Beach));
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
      tileTypes.Add(new MountainTileType(TerrainTileIds.Mountain));
    }

    for (var tileId = TerrainTileIds.Mountain + 1; tileId < tileCount; tileId++)
    {
      tileTypes.Add(new GenericTileType(tileId));
    }

    return new TileTypeRegistry(tileTypes);
  }
}