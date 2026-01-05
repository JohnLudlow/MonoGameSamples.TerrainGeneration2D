using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

/// <summary>
/// Wave Function Collapse algorithm for procedural tile generation
/// </summary>
public class WaveFunctionCollapse
{
  private readonly TileTypeRegistry _tileRegistry;
  private readonly Random _random;
  private readonly int _width;
  private readonly int _height;
  private readonly HashSet<int>?[,] _possibilities;
  private readonly int[,] _output;
  private readonly MappingInformationService _mappingService;
  private readonly TerrainRuleConfiguration _config;
  private readonly IHeightProvider _heightProvider;
  private readonly Point _chunkOrigin;
  private bool _collapsed;

  public WaveFunctionCollapse(int width, int height, TileTypeRegistry tileRegistry, Random random, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    _random = random ?? throw new ArgumentNullException(nameof(random));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;

    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }

  /// <summary>
  /// Run the WFC algorithm until all cells are collapsed or contradiction occurs
  /// </summary>
  public bool Generate(int maxIterations = 10000)
  {
    TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(_chunkOrigin.X, _chunkOrigin.Y);
    bool success = false;

    try
    {
      int iterations = 0;

      while (!_collapsed && iterations < maxIterations)
      {
        var (x, y) = FindLowestEntropy();

        if (x == -1 || y == -1)
        {
          // All cells collapsed
          _collapsed = true;
          success = true;
          return true;
        }

        if (!CollapseCell(x, y))
        {
          // Contradiction - generation failed
          success = false;
          return false;
        }

        if (!Propagate(x, y))
        {
          // Propagation caused contradiction
          success = false;
          return false;
        }

        iterations++;
      }

      success = _collapsed;
      return success;
    }
    finally
    {
      TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(_chunkOrigin.X, _chunkOrigin.Y, success);
    }
  }

  /// <summary>
  /// Get the generated output
  /// </summary>
  public int[,] GetOutput() => _output;

  /// <summary>
  /// Find the cell with lowest entropy (fewest possibilities) that hasn't been collapsed
  /// </summary>
  private (int x, int y) FindLowestEntropy()
  {
    int minEntropy = int.MaxValue;
    List<(int x, int y)> candidates = new();

    for (int y = 0; y < _height; y++)
    {
      for (int x = 0; x < _width; x++)
      {
        var possibilities = _possibilities[x, y];
        if (possibilities == null || possibilities.Count <= 1)
          continue; // Already collapsed or contradiction

        int entropy = possibilities.Count;

        if (entropy < minEntropy)
        {
          minEntropy = entropy;
          candidates.Clear();
          candidates.Add((x, y));
        }
        else if (entropy == minEntropy)
        {
          candidates.Add((x, y));
        }
      }
    }

    if (candidates.Count == 0)
      return (-1, -1); // All collapsed

    // Pick randomly from candidates with same entropy
    return candidates[_random.Next(candidates.Count)];
  }

  /// <summary>
  /// Collapse a cell to a single tile
  /// </summary>
  private bool CollapseCell(int x, int y)
  {
    var possibilities = _possibilities[x, y];
    if (possibilities == null || possibilities.Count == 0)
      return false; // Contradiction

    var neighborTiles = new List<int>();
    if (y > 0 && _output[x, y - 1] != -1) neighborTiles.Add(_output[x, y - 1]);
    if (y < _height - 1 && _output[x, y + 1] != -1) neighborTiles.Add(_output[x, y + 1]);
    if (x > 0 && _output[x - 1, y] != -1) neighborTiles.Add(_output[x - 1, y]);
    if (x < _width - 1 && _output[x + 1, y] != -1) neighborTiles.Add(_output[x + 1, y]);

    var weightedOptions = possibilities
        .Select(tile => new
        {
          Tile = tile,
          Weight = 1 + neighborTiles.Count(neighbor => neighbor == tile) * 3
        })
        .ToList();

    int totalWeight = weightedOptions.Sum(option => option.Weight);
    int roll = _random.Next(totalWeight);

    int cumulative = 0;
    int chosenTile = weightedOptions.First().Tile;

    foreach (var option in weightedOptions)
    {
      cumulative += option.Weight;
      if (roll < cumulative)
      {
        chosenTile = option.Tile;
        break;
      }
    }

    _output[x, y] = chosenTile;
    _possibilities[x, y] = null; // Mark as collapsed

    return true;
  }

  /// <summary>
  /// Propagate constraints from collapsed cell to neighbors
  /// </summary>
  private bool Propagate(int startX, int startY)
  {
    Queue<(int x, int y)> queue = new();
    queue.Enqueue((startX, startY));

    while (queue.Count > 0)
    {
      var (x, y) = queue.Dequeue();
      int currentTile = _output[x, y];

      if (currentTile == -1)
        continue; // Not collapsed yet

      // Constrain north neighbor
      var currentPoint = new TilePoint(x, y);
      if (y > 0 && !ConstrainNeighbor(x, y - 1, Direction.South, currentTile, currentPoint))
        return false;

      if (y < _height - 1 && !ConstrainNeighbor(x, y + 1, Direction.North, currentTile, currentPoint))
        return false;

      if (x < _width - 1 && !ConstrainNeighbor(x + 1, y, Direction.West, currentTile, currentPoint))
        return false;

      if (x > 0 && !ConstrainNeighbor(x - 1, y, Direction.East, currentTile, currentPoint))
        return false;

      // Add changed neighbors to queue for further propagation
      if (y > 0 && _possibilities[x, y - 1] != null) queue.Enqueue((x, y - 1));
      if (y < _height - 1 && _possibilities[x, y + 1] != null) queue.Enqueue((x, y + 1));
      if (x < _width - 1 && _possibilities[x + 1, y] != null) queue.Enqueue((x + 1, y));
      if (x > 0 && _possibilities[x - 1, y] != null) queue.Enqueue((x - 1, y));
    }

    return true;
  }

  private bool ConstrainNeighbor(int x, int y, Direction directionToNeighbor, int neighborTileId, TilePoint neighborPosition)
  {
    var possibilities = _possibilities[x, y];
    if (possibilities == null)
    {
      return true;
    }

    var allowed = new HashSet<int>();
    var candidatePoint = new TilePoint(x, y);

    foreach (var tileId in possibilities.ToList())
    {
      var tileType = _tileRegistry.GetTileType(tileId);
      var candidateWorldX = _chunkOrigin.X + candidatePoint.X;
      var candidateWorldY = _chunkOrigin.Y + candidatePoint.Y;
      var neighborWorldX = _chunkOrigin.X + neighborPosition.X;
      var neighborWorldY = _chunkOrigin.Y + neighborPosition.Y;
      var candidateSample = _heightProvider.GetSample(candidateWorldX, candidateWorldY);
      var neighborSample = _heightProvider.GetSample(neighborWorldX, neighborWorldY);
      var context = new TileRuleContext(
          candidatePoint,
          tileId,
          neighborPosition,
          neighborTileId,
          directionToNeighbor,
          _config,
          candidateSample,
          neighborSample,
          _mappingService);

      if (tileType.EvaluateRules(context))
      {
        allowed.Add(tileId);
      }
    }

    if (allowed.Count == 0)
    {
      return false;
    }

    possibilities.IntersectWith(allowed);

    if (possibilities.Count == 1)
    {
      _output[x, y] = possibilities.First();
      _possibilities[x, y] = null;
    }

    return true;
  }
}
