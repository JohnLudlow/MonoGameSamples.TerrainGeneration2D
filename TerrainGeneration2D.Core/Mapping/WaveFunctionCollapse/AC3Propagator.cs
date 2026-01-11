// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/AC3Propagator.cs

using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// AC-3 constraint propagation implementation for WFC domains.
/// </summary>
/// <remarks>
/// Maintains arc consistency between all neighboring cell domains.
/// </remarks>
public class AC3Propagator
{
  private readonly IRuleTable _ruleTable;
  private readonly Queue<(int x, int y, Direction dir)> _arcQueue;
  private readonly HashSet<int>?[][] _domains;

  /// <summary>
  /// Initializes a new AC-3 propagator with the specified rule table and domain grid.
  /// </summary>
  /// <param name="ruleTable">Precomputed rule table for adjacency lookups</param>
  /// <param name="domains">Reference to the WFC domain grid using jagged arrays for optimal performance</param>
  public AC3Propagator(IRuleTable ruleTable, HashSet<int>?[][] domains)
  {
    _ruleTable = ruleTable ?? throw new ArgumentNullException(nameof(ruleTable));
    _domains = domains ?? throw new ArgumentNullException(nameof(domains));
    _arcQueue = new Queue<(int x, int y, Direction dir)>();
  }

  /// <summary>
  /// Propagates constraints from a newly collapsed cell to all neighbors.
  /// </summary>
  /// <param name="sourceX">X coordinate of collapsed cell</param>
  /// <param name="sourceY">Y coordinate of collapsed cell</param>
  /// <param name="placedTileId">Tile ID that was placed</param>
  /// <returns>True if propagation succeeded; false if contradiction detected</returns>
  public bool PropagateFrom(int sourceX, int sourceY, int placedTileId)
  {
    // Example: Ocean (ID=0) can only be adjacent to Beach (ID=1) on North/South/East/West
    // Beach (ID=1) can be adjacent to Ocean (ID=0) or Plains (ID=2)
    // Plains (ID=2) can be adjacent to Beach (ID=1) or Forest (ID=3)

    // Enqueue arcs from all neighbors back to the collapsed cell
    var neighbors = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // N, E, S, W
    var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

    for (var i = 0; i < neighbors.Length; i++)
    {
      var (dx, dy) = neighbors[i];
      var neighborX = sourceX + dx;
      var neighborY = sourceY + dy;

      if (IsValidCoordinate(neighborX, neighborY))
      {
        _arcQueue.Enqueue((neighborX, neighborY, directions[i]));
      }
    }

    // Process arc consistency
    while (_arcQueue.Count > 0)
    {
      var (x, y, direction) = _arcQueue.Dequeue();

      if (RemoveInconsistentValues(x, y, direction))
      {
        if (_domains[x][y]?.Count == 0)
          return false; // Contradiction detected

        // Re-enqueue arcs from neighbors of (x,y)
        EnqueueNeighborArcs(x, y);
      }
    }

    return true;
  }

  /// <summary>
  /// Checks if the given coordinates are within the domain grid bounds.
  /// </summary>
  private bool IsValidCoordinate(int x, int y)
  {
    return x >= 0 && y >= 0 && x < _domains.Length && y < _domains[x].Length;
  }

  /// <summary>
  /// Gets the neighbor position in the specified direction.
  /// </summary>
  private static (int x, int y) GetNeighborPosition(int x, int y, Direction direction)
  {
    return direction switch
    {
      Direction.North => (x, y - 1),
      Direction.East => (x + 1, y),
      Direction.South => (x, y + 1),
      Direction.West => (x - 1, y),
      _ => (x, y)
    };
  }

  /// <summary>
  /// Enqueues arcs from all neighbors of the given cell for consistency checking.
  /// </summary>
  private void EnqueueNeighborArcs(int x, int y)
  {
    var neighbors = new[] { (0, -1), (1, 0), (0, 1), (-1, 0) }; // N, E, S, W
    var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

    for (var i = 0; i < neighbors.Length; i++)
    {
      var (dx, dy) = neighbors[i];
      var neighborX = x + dx;
      var neighborY = y + dy;

      if (IsValidCoordinate(neighborX, neighborY))
      {
        // Enqueue arc from neighbor back to current cell
        var oppositeDirection = GetOppositeDirection(directions[i]);
        _arcQueue.Enqueue((neighborX, neighborY, oppositeDirection));
      }
    }
  }

  /// <summary>
  /// Gets the opposite direction for constraint checking.
  /// </summary>
  private static Direction GetOppositeDirection(Direction direction)
  {
    return direction switch
    {
      Direction.North => Direction.South,
      Direction.East => Direction.West,
      Direction.South => Direction.North,
      Direction.West => Direction.East,
      _ => direction
    };
  }

  ///<summary>
  /// 
  ///</summary>
  ///<param name="x"></param>
  ///<param name="y"></param>
  ///<param name="direction"></param> <summary>
  private bool RemoveInconsistentValues(int x, int y, Direction direction)
  {
    // Example rule check: if neighbor cell contains Ocean (ID=0),
    // current cell can only contain Beach (ID=1)
    var neighborPos = GetNeighborPosition(x, y, direction);
    if (!IsValidCoordinate(neighborPos.x, neighborPos.y))
      return false;

    var currentDomain = _domains[x][y];
    var neighborDomain = _domains[neighborPos.x][neighborPos.y];

    if (currentDomain is null || neighborDomain is null) return false;

    var removed = false;

    var tilesToRemove = new List<int>();
    foreach (var tileId in currentDomain)
    {
      var allowedNeighbors = _ruleTable.GetAllowedNeighbors(tileId, direction);

      // Check if any tile in neighbor domain is allowed
      var hasSupport = neighborDomain.Any(neighborTile =>
          allowedNeighbors.Contains(neighborTile));

      if (!hasSupport)
      {
        tilesToRemove.Add(tileId);
      }
    }

    foreach (var tile in tilesToRemove)
    {
      currentDomain.Remove(tile);
      removed = true;
    }

    return removed;
  }
}