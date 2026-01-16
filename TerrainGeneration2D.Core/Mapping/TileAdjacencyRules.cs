using System;
using System.Collections.Generic;
using System.Linq;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

/// <summary>
/// Defines which tiles can be adjacent to each other in each direction
/// </summary>
public class TileAdjacencyRules
{
  private readonly Dictionary<int, HashSet<int>> _northRules = new();
  private readonly Dictionary<int, HashSet<int>> _southRules = new();
  private readonly Dictionary<int, HashSet<int>> _eastRules = new();
  private readonly Dictionary<int, HashSet<int>> _westRules = new();

  public int TileCount { get; }

  public TileAdjacencyRules(int tileCount)
  {
    TileCount = tileCount;

    for (var i = 0; i < tileCount; i++)
    {
      _northRules[i] = new HashSet<int>();
      _southRules[i] = new HashSet<int>();
      _eastRules[i] = new HashSet<int>();
      _westRules[i] = new HashSet<int>();
    }
  }

  /// <summary>
  /// Add a rule: tileA can have tileB to its north
  /// </summary>
  public void AddNorthRule(int tileA, int tileB)
  {
    _northRules[tileA].Add(tileB);
    _southRules[tileB].Add(tileA); // Symmetric relationship
  }

  /// <summary>
  /// Add a rule: tileA can have tileB to its south
  /// </summary>
  public void AddSouthRule(int tileA, int tileB)
  {
    _southRules[tileA].Add(tileB);
    _northRules[tileB].Add(tileA); // Symmetric relationship
  }

  /// <summary>
  /// Add a rule: tileA can have tileB to its east
  /// </summary>
  public void AddEastRule(int tileA, int tileB)
  {
    _eastRules[tileA].Add(tileB);
    _westRules[tileB].Add(tileA); // Symmetric relationship
  }

  /// <summary>
  /// Add a rule: tileA can have tileB to its west
  /// </summary>
  public void AddWestRule(int tileA, int tileB)
  {
    _westRules[tileA].Add(tileB);
    _eastRules[tileB].Add(tileA); // Symmetric relationship
  }

  /// <summary>
  /// Get all tiles that can be north of the given tile
  /// </summary>
  public HashSet<int> GetValidNorthTiles(int tileId) => _northRules[tileId];

  /// <summary>
  /// Get all tiles that can be south of the given tile
  /// </summary>
  public HashSet<int> GetValidSouthTiles(int tileId) => _southRules[tileId];

  /// <summary>
  /// Get all tiles that can be east of the given tile
  /// </summary>
  public HashSet<int> GetValidEastTiles(int tileId) => _eastRules[tileId];

  /// <summary>
  /// Get all tiles that can be west of the given tile
  /// </summary>
  public HashSet<int> GetValidWestTiles(int tileId) => _westRules[tileId];

  /// <summary>
  /// Create default rules where all tiles can be adjacent to all tiles
  /// </summary>
  public static TileAdjacencyRules CreateDefault(int tileCount)
  {
    var rules = new TileAdjacencyRules(tileCount);

    for (var i = 0; i < tileCount; i++)
    {
      for (var j = 0; j < tileCount; j++)
      {
        rules._northRules[i].Add(j);
        rules._southRules[i].Add(j);
        rules._eastRules[i].Add(j);
        rules._westRules[i].Add(j);
      }
    }

    return rules;
  }

  /// <summary>
  /// Create simple terrain rules for demonstration
  /// Assumes tiles follow: 0=null/void, 1=ocean, 2=beach/coast, 3=plains, 4=forest, 5=snow, 6=mountain
  /// </summary>
  public static TileAdjacencyRules CreateSimpleTerrainRules(int tileCount)
  {
    var rules = new TileAdjacencyRules(tileCount);

    if (tileCount < 7)
    {
      return CreateDefault(tileCount);
    }

    var voidTiles = new[] { 0 };
    var ocean = new[] { 1 };
    var beach = new[] { 2 };
    var plains = new[] { 3 };
    var forest = new[] { 4 };
    var snow = new[] { 5 };
    var mountain = new[] { 6 };
    var extraTiles = Enumerable.Range(7, Math.Max(0, tileCount - 7)).ToArray();

    foreach (var tile in voidTiles)
    {
      foreach (var neighbor in Enumerable.Range(0, tileCount))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in ocean)
    {
      foreach (var neighbor in ocean.Concat(beach))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in beach)
    {
      foreach (var neighbor in ocean.Concat(beach).Concat(plains))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in plains)
    {
      foreach (var neighbor in beach.Concat(plains).Concat(forest))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in forest)
    {
      foreach (var neighbor in plains.Concat(forest).Concat(snow).Concat(mountain))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in snow)
    {
      foreach (var neighbor in forest.Concat(snow).Concat(mountain))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    foreach (var tile in mountain)
    {
      foreach (var neighbor in forest.Concat(snow).Concat(mountain))
      {
        rules.AddNorthRule(tile, neighbor);
        rules.AddSouthRule(tile, neighbor);
        rules.AddEastRule(tile, neighbor);
        rules.AddWestRule(tile, neighbor);
      }
    }

    if (extraTiles.Length > 0)
    {
      foreach (var tile in extraTiles)
      {
        foreach (var neighbor in Enumerable.Range(0, tileCount))
        {
          rules.AddNorthRule(tile, neighbor);
          rules.AddSouthRule(tile, neighbor);
          rules.AddEastRule(tile, neighbor);
          rules.AddWestRule(tile, neighbor);
        }
      }
    }

    return rules;
  }
}