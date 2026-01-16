// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/AC3Propagator.cs

using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

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
  private readonly HashSet<int>[][] _domains;

  /// <summary>
  /// Initializes a new AC-3 propagator with the specified rule table and domain grid.
  /// </summary>
  /// <param name="ruleTable">Precomputed rule table for adjacency lookups</param>
  /// <param name="domains">Reference to the WFC domain grid using jagged arrays for optimal performance</param>
  public AC3Propagator(IRuleTable ruleTable, HashSet<int>[][] domains)
  {
    _ruleTable = ruleTable ?? throw new ArgumentNullException(nameof(ruleTable));
    _domains = domains ?? throw new ArgumentNullException(nameof(domains));
    _arcQueue = new Queue<(int x, int y, Direction dir)>();
  }

  /// <summary>
  /// Propagates constraints from a newly collapsed cell to all neighbors, optionally recording changes for backtracking.
  /// </summary>
  /// <param name="sourceX">X coordinate of collapsed cell</param>
  /// <param name="sourceY">Y coordinate of collapsed cell</param>
  /// <param name="placedTileId">Tile ID that was placed</param>
  /// <param name="log">Optional ChangeLog for reversible propagation</param>
  /// <returns>True if propagation succeeded; false if contradiction detected</returns>
  public bool PropagateFrom(int sourceX, int sourceY, int placedTileId, ChangeLog? log = null)
  {
    // Enqueue arcs from all neighbors back to the collapsed cell
    var neighbors = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // N, E, S, W
    var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

    for (var i = 0; i < neighbors.Length; i++)
    {
      var (dx, dy) = neighbors[i];

      var nx = sourceX + dx;
      var ny = sourceY + dy;

      if (IsValidCoordinate(nx, ny) && _domains[nx][ny] != null)
      {
        _arcQueue.Enqueue((nx, ny, GetOppositeDirection(directions[i])));
      }
    }

    // Process arc consistency
    while (_arcQueue.Count > 0)
    {
      var (x, y, direction) = _arcQueue.Dequeue();
      if (RemoveInconsistentValues(x, y, direction, log))
      {
        // If domain is empty, contradiction
        if (_domains[x][y] != null && _domains[x][y].Count == 0)
          return false;

        // If domain reduced, enqueue neighbors
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
  private (int x, int y) GetNeighborPosition(int x, int y, Direction direction)
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

    for (int i = 0; i < neighbors.Length; i++)
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
  private Direction GetOppositeDirection(Direction direction)
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

  private bool RemoveInconsistentValues(int x, int y, Direction direction, ChangeLog? log = null)
  {
    // Example rule check: if neighbor cell contains Ocean (ID=0),
    // current cell can only contain Beach (ID=1)
    var neighborPos = GetNeighborPosition(x, y, direction);
    if (!IsValidCoordinate(neighborPos.x, neighborPos.y))
      return false;

    var currentDomain = _domains[x][y];
    var neighborDomain = _domains[neighborPos.x][neighborPos.y];

    // Handle null domains: null means collapsed cell, skip processing
    if (currentDomain == null || neighborDomain == null)
      return false;

    var removed = false;
    var tilesToRemove = new List<int>();

    foreach (var tileId in currentDomain.ToList())
    {
      var allowedNeighbors = _ruleTable.GetAllowedNeighbors(tileId, direction);
      var hasSupport = neighborDomain.Any(n => allowedNeighbors.Contains(n));

      if (!hasSupport)
        tilesToRemove.Add(tileId);
    }

    foreach (var tile in tilesToRemove)
    {
      currentDomain.Remove(tile);
      log?.RecordDomainRemoved(x, y, tile);


      removed = true;
    }

    if (currentDomain.Count == 1)
    {
      var chosenTile = currentDomain.First();
      if (log != null) log.RecordCellCollapsed(x, y, currentDomain, chosenTile);
      // ...existing code for cell collapse...
    }

    return removed;
  }
}