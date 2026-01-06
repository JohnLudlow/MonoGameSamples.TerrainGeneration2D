using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using Microsoft.Xna.Framework;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Wave Function Collapse (WFC) provider for procedural tile generation.
/// Supports standard collapse/propagation and an optional backtracking mode
/// that records reversible changes and retries alternate candidates on contradictions.
/// </summary>
public class WfcProvider
{
  private readonly TileTypeRegistry _tileRegistry;
  private readonly IRandomProvider _random;
  private readonly WfcWeightConfiguration _weightConfig;
  private readonly HeuristicsConfiguration _heuristicsConfig;
  private readonly ICellEntropyProvider _domainEntropy;
  private readonly ICellEntropyProvider _shannonEntropy;
  private readonly int _width;
  private readonly int _height;
  private readonly HashSet<int>?[,] _possibilities;
  private readonly int[,] _output;
  private readonly MappingInformationService _mappingService;
  private readonly TerrainRuleConfiguration _config;
  private readonly IHeightProvider _heightProvider;
  private readonly Point _chunkOrigin;
  private bool _collapsed;

  /// <summary>
  /// Create a WFC solver bound to a chunk-sized grid.
  /// </summary>
  /// <param name="width">Number of tiles in X for this solve.</param>
  /// <param name="height">Number of tiles in Y for this solve.</param>
  /// <param name="tileRegistry">Tile registry and rules.</param>
  /// <param name="random">Deterministic random source (seeded per-chunk).</param>
  /// <param name="config">Terrain rule configuration.</param>
  /// <param name="heightProvider">Height/biome sampler for contextual rules.</param>
  /// <param name="chunkOrigin">World-space origin of this chunk, used for sampling.</param>
  public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, Random random, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    ArgumentNullException.ThrowIfNull(random);
    _random = new RandomAdapter(random);
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;
    _weightConfig = new WfcWeightConfiguration();
    _heuristicsConfig = new HeuristicsConfiguration();
    _domainEntropy = new DomainEntropyProvider();
    _shannonEntropy = new ShannonEntropyProvider();

    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }

  /// <summary>
  /// Create a WFC solver using a custom random provider (useful for deterministic tests).
  /// </summary>
  public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, IRandomProvider randomProvider, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    _random = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;
    _weightConfig = new WfcWeightConfiguration();
    _heuristicsConfig = new HeuristicsConfiguration();
    _domainEntropy = new DomainEntropyProvider();
    _shannonEntropy = new ShannonEntropyProvider();

    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }

  /// <summary>
  /// Create a WFC solver using a custom random provider and weight configuration.
  /// </summary>
  public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, IRandomProvider randomProvider, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin, WfcWeightConfiguration weightConfig)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    _random = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;
    _weightConfig = weightConfig ?? new WfcWeightConfiguration();
    _heuristicsConfig = new HeuristicsConfiguration();
    _domainEntropy = new DomainEntropyProvider();
    _shannonEntropy = new ShannonEntropyProvider();

    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }

  /// <summary>
  /// Create a WFC solver using System.Random and weight configuration.
  /// </summary>
  public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, Random random, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin, WfcWeightConfiguration weightConfig)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    ArgumentNullException.ThrowIfNull(random);
    _random = new RandomAdapter(random);
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;
    _weightConfig = weightConfig ?? new WfcWeightConfiguration();
    _heuristicsConfig = new HeuristicsConfiguration();
    _domainEntropy = new DomainEntropyProvider();
    _shannonEntropy = new ShannonEntropyProvider();
    
    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }
  /// <summary>
  /// Create a WFC solver with custom random, weight, and heuristics configuration.
  /// </summary>
  public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, IRandomProvider randomProvider, TerrainRuleConfiguration config, IHeightProvider heightProvider, Point chunkOrigin, WfcWeightConfiguration weightConfig, HeuristicsConfiguration heuristics)
  {
    _width = width;
    _height = height;
    _tileRegistry = tileRegistry ?? throw new ArgumentNullException(nameof(tileRegistry));
    _random = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _heightProvider = heightProvider ?? throw new ArgumentNullException(nameof(heightProvider));
    _chunkOrigin = chunkOrigin;
    _weightConfig = weightConfig ?? new WfcWeightConfiguration();
    _heuristicsConfig = heuristics ?? new HeuristicsConfiguration();
    _domainEntropy = new DomainEntropyProvider();
    _shannonEntropy = new ShannonEntropyProvider();

    var validTileIds = _tileRegistry.ValidTileIds;
    _possibilities = new HashSet<int>?[width, height];
    _output = new int[width, height];
    _collapsed = false;
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        _possibilities[x, y] = new HashSet<int>(validTileIds);
        _output[x, y] = -1;
      }
    }

    _mappingService = new MappingInformationService(_output);
  }

  /// <summary>
  /// Run WFC without backtracking until all cells collapse or a contradiction occurs.
  /// </summary>
  /// <param name="maxIterations">Safety cap on iterations.</param>
  /// <returns>True if fully collapsed; false on contradiction.</returns>
  public bool Generate(int maxIterations = 10000, TimeSpan? timeBudget = null)
  {
    TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(_chunkOrigin.X, _chunkOrigin.Y);
    var success = false;
    var decisions = 0;
    const int depth = 0; // no backtracking yet
    Stopwatch? sw = null;
    if (timeBudget.HasValue) sw = Stopwatch.StartNew();

    try
    {
      var iterations = 0;

      while (!_collapsed && iterations < maxIterations)
      {
        if (sw != null && sw.Elapsed > timeBudget!.Value)
        {
          success = false;
          return false;
        }
        var (x, y) = FindLowestEntropy();

        if (x == -1 || y == -1)
        {
          _collapsed = true;
          success = true;
          return true;
        }

        var poss = _possibilities[x, y];
        var candidateCount = poss?.Count ?? 0;
        TerrainPerformanceEventSource.Log.WfcDecisionPush(depth, x, y, candidateCount);

        if (!CollapseCell(x, y))
        {
          TerrainPerformanceEventSource.Log.WfcContradiction(depth, x, y);
          success = false;
          return false;
        }

        if (!Propagate(x, y))
        {
          TerrainPerformanceEventSource.Log.WfcContradiction(depth, x, y);
          success = false;
          return false;
        }

        TerrainPerformanceEventSource.Log.WfcDecisionPop(depth);
        iterations++;
      }

      success = _collapsed;
      return success;
    }
    finally
    {
      TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(_chunkOrigin.X, _chunkOrigin.Y, success);
      TerrainPerformanceEventSource.Log.WfcStats(decisions, 0, 0);
    }
  }

  /// <summary>
  /// Run WFC with optional backtracking. When enabled, the solver records changes
  /// and attempts alternate candidates on contradictions, rolling state back as needed.
  /// </summary>
  /// <param name="enableBacktracking">Enable decision-stack backtracking.</param>
  /// <param name="maxIterations">Safety cap on forward solve iterations.</param>
  /// <param name="maxBacktrackSteps">Maximum number of backtrack steps before failing.</param>
  /// <param name="maxDepth">Optional maximum decision depth; useful to bound search.</param>
  /// <returns>True if fully collapsed; false if limits hit or unsatisfiable.</returns>
  public bool Generate(bool enableBacktracking, int maxIterations = 10000, int? maxBacktrackSteps = null, int? maxDepth = null, TimeSpan? timeBudget = null)
  {
    if (!enableBacktracking)
    {
      return Generate(maxIterations, timeBudget);
    }

    TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(_chunkOrigin.X, _chunkOrigin.Y);
    var success = false;
    var iterations = 0;
    var backtracks = 0;
    var maxObservedDepth = 0;
    var log = new ChangeLog();
    var stack = new Stack<DecisionFrame>();

    Stopwatch? sw = null;
    if (timeBudget.HasValue) sw = Stopwatch.StartNew();

    try
    {
      while (iterations < maxIterations)
      {
        if (sw != null && sw.Elapsed > timeBudget!.Value)
        {
          success = false;
          return false;
        }
        var (x, y) = FindLowestEntropy();
        if (x == -1 || y == -1)
        {
          success = true;
          return true;
        }

        var poss = _possibilities[x, y];
        if (poss == null || poss.Count == 0)
        {
          success = false;
          return false;
        }

        var neighborTiles = new List<int>();
        if (y > 0 && _output[x, y - 1] != -1) neighborTiles.Add(_output[x, y - 1]);
        if (y < _height - 1 && _output[x, y + 1] != -1) neighborTiles.Add(_output[x, y + 1]);
        if (x > 0 && _output[x - 1, y] != -1) neighborTiles.Add(_output[x - 1, y]);
        if (x < _width - 1 && _output[x + 1, y] != -1) neighborTiles.Add(_output[x + 1, y]);

        var weighted = poss.Select(tile => new { Tile = tile, Weight = _weightConfig.Base + neighborTiles.Count(n => n == tile) * _weightConfig.NeighborMatchBoost }).ToList();
        var ordered = weighted
          .OrderByDescending(w => w.Weight)
          .ThenBy(w => w.Tile)
          .Select(w => w.Tile)
          .ToArray();

        var depth = stack.Count + 1;
        maxObservedDepth = Math.Max(maxObservedDepth, depth);
        TerrainPerformanceEventSource.Log.WfcDecisionPush(depth, x, y, ordered.Length);
        var frame = new DecisionFrame { X = x, Y = y, Candidates = ordered, NextIndex = 0, ChangesMark = log.Mark(), Depth = depth };
        stack.Push(frame);

        var advanced = false;
        while (stack.Count > 0)
        {
          if (sw != null && sw.Elapsed > timeBudget!.Value)
          {
            success = false;
            return false;
          }
          var top = stack.Peek();
          if (maxDepth.HasValue && top.Depth > maxDepth.Value)
          {
            TerrainPerformanceEventSource.Log.WfcRollbackBegin(top.Depth, top.ChangesMark);
            log.RollbackTo(top.ChangesMark, _possibilities, _output);
            TerrainPerformanceEventSource.Log.WfcRollbackEnd(top.Depth);
            stack.Pop();
            TerrainPerformanceEventSource.Log.WfcDecisionPop(top.Depth);
            backtracks++;
            break;
          }

          if (top.NextIndex >= top.Candidates.Length)
          {
            stack.Pop();
            TerrainPerformanceEventSource.Log.WfcDecisionPop(top.Depth);
            if (stack.Count == 0)
            {
              success = false;
              return false;
            }
            continue;
          }

          var chosen = top.Candidates[top.NextIndex++];
          TerrainPerformanceEventSource.Log.WfcApplyChoice(top.Depth, top.X, top.Y, chosen);

          if (!CollapseCell(top.X, top.Y, chosen, log))
          {
            TerrainPerformanceEventSource.Log.WfcContradiction(top.Depth, top.X, top.Y);
            TerrainPerformanceEventSource.Log.WfcRollbackBegin(top.Depth, top.ChangesMark);
            log.RollbackTo(top.ChangesMark, _possibilities, _output);
            TerrainPerformanceEventSource.Log.WfcRollbackEnd(top.Depth);
            backtracks++;
            if (maxBacktrackSteps.HasValue && backtracks > maxBacktrackSteps.Value)
            {
              success = false;
              return false;
            }
            continue;
          }

          if (!Propagate(top.X, top.Y, log))
          {
            TerrainPerformanceEventSource.Log.WfcContradiction(top.Depth, top.X, top.Y);
            TerrainPerformanceEventSource.Log.WfcRollbackBegin(top.Depth, top.ChangesMark);
            log.RollbackTo(top.ChangesMark, _possibilities, _output);
            TerrainPerformanceEventSource.Log.WfcRollbackEnd(top.Depth);
            backtracks++;
            if (maxBacktrackSteps.HasValue && backtracks > maxBacktrackSteps.Value)
            {
              success = false;
              return false;
            }
            continue;
          }

          advanced = true;
          break;
        }

        if (advanced)
        {
          iterations++;
        }
        else
        {
          iterations++;
        }
      }

      success = false;
      return false;
    }
    finally
    {
      TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(_chunkOrigin.X, _chunkOrigin.Y, success);
      TerrainPerformanceEventSource.Log.WfcStats(0, backtracks, maxObservedDepth);
    }
  }

  /// <summary>
  /// Get the final tile output for this solve. When backtracking is enabled
  /// and the solver succeeds, this contains the collapsed tile IDs.
  /// </summary>
  public int[,] GetOutput() => _output;

  private (int x, int y) FindLowestEntropy()
  {
    // Collect candidate cells and compute both scores when enabled
    var candidateCells = new List<(int x, int y, double kScore, double hScore, int influence)>();

    for (var y = 0; y < _height; y++)
    {
      for (var x = 0; x < _width; x++)
      {
        var poss = _possibilities[x, y];
        if (poss == null || poss.Count <= 1) continue;

        var k = _heuristicsConfig.UseDomainEntropy ? _domainEntropy.GetScore(x, y, _possibilities, _output, _weightConfig) : double.PositiveInfinity;
        var h = _heuristicsConfig.UseShannonEntropy ? _shannonEntropy.GetScore(x, y, _possibilities, _output, _weightConfig) : double.PositiveInfinity;

        // Influence: how many undecided neighbors this cell may constrain
        var influence = 0;
        if (y > 0 && _possibilities[x, y - 1] != null) influence++;
        if (y < _height - 1 && _possibilities[x, y + 1] != null) influence++;
        if (x > 0 && _possibilities[x - 1, y] != null) influence++;
        if (x < _width - 1 && _possibilities[x + 1, y] != null) influence++;

        candidateCells.Add((x, y, k, h, influence));
      }
    }

    if (candidateCells.Count == 0)
      return (-1, -1);

    if (!_heuristicsConfig.UseDomainEntropy && !_heuristicsConfig.UseShannonEntropy)
      throw new InvalidOperationException("No entropy heuristic enabled: enable DomainEntropy and/or ShannonEntropy.");

    // Selection
    List<(int x, int y, double k, double h, int influence)> shortlist;
    if (_heuristicsConfig.UseDomainEntropy && _heuristicsConfig.UseShannonEntropy)
    {
      var minK = candidateCells.Min(c => c.kScore);
      var minH = candidateCells.Min(c => c.hScore);
      var setK = candidateCells.Where(c => Math.Abs(c.kScore - minK) < 1e-9).ToList();
      var setH = candidateCells.Where(c => Math.Abs(c.hScore - minH) < 1e-9).ToList();
      var intersect = setK.Where(k => setH.Any(h => h.x == k.x && h.y == k.y)).ToList();
      shortlist = intersect.Count > 0 ? intersect : setK.Concat(setH).ToList();
    }
    else if (_heuristicsConfig.UseDomainEntropy)
    {
      var minK = candidateCells.Min(c => c.kScore);
      shortlist = candidateCells.Where(c => Math.Abs(c.kScore - minK) < 1e-9).ToList();
    }
    else
    {
      var minH = candidateCells.Min(c => c.hScore);
      shortlist = candidateCells.Where(c => Math.Abs(c.hScore - minH) < 1e-9).ToList();
    }

    if (shortlist.Count == 0)
      return (-1, -1);

    if (_heuristicsConfig.UseDomainEntropy && _heuristicsConfig.UseShannonEntropy && _heuristicsConfig.UseMostConstrainingTieBreak)
    {
      var maxInf = shortlist.Max(c => c.influence);
      shortlist = shortlist.Where(c => c.influence == maxInf).ToList();
    }

    var choice = shortlist[_random.NextInt(shortlist.Count)];
    return (choice.x, choice.y);
  }

  private bool CollapseCell(int x, int y)
  {
    var possibilities = _possibilities[x, y];
    if (possibilities == null || possibilities.Count == 0)
      return false;

    var neighborTiles = new List<int>();

    if (y > 0 && _output[x, y - 1] != -1) neighborTiles.Add(_output[x, y - 1]);
    if (y < _height - 1 && _output[x, y + 1] != -1) neighborTiles.Add(_output[x, y + 1]);
    if (x > 0 && _output[x - 1, y] != -1) neighborTiles.Add(_output[x - 1, y]);
    if (x < _width - 1 && _output[x + 1, y] != -1) neighborTiles.Add(_output[x + 1, y]);

    var weightedOptions = possibilities
        .Select(tile => new
        {
          Tile = tile,
          Weight = _weightConfig.Base + neighborTiles.Count(neighbor => neighbor == tile) * _weightConfig.NeighborMatchBoost
        })
        .OrderBy(o => o.Tile)
        .ToList();

    var totalWeight = weightedOptions.Sum(option => option.Weight);
    var roll = _random.NextInt(totalWeight);

    var cumulative = 0;
    var chosenTile = weightedOptions.First().Tile;

    foreach (var option in weightedOptions)
    {
      cumulative += option.Weight;
      if (roll < cumulative)
      {
        chosenTile = option.Tile;
        break;
      }
    }

    TerrainPerformanceEventSource.Log.WfcApplyChoice(0, x, y, chosenTile);
    _output[x, y] = chosenTile;
    _possibilities[x, y] = null;

    return true;
  }

  private bool CollapseCell(int x, int y, int chosenTile, ChangeLog log)
  {
    var possibilities = _possibilities[x, y];
    if (possibilities == null)
      return true;
    if (possibilities.Count == 0)
      return false;

    log.RecordCellCollapsed(x, y, possibilities, chosenTile);
    var prev = _output[x, y];
    log.RecordOutputSet(x, y, prev, chosenTile);
    _output[x, y] = chosenTile;
    _possibilities[x, y] = null;
    return true;
  }

  private bool Propagate(int startX, int startY)
  {
    Queue<(int x, int y)> queue = new();
    queue.Enqueue((startX, startY));

    while (queue.Count > 0)
    {
      var (x, y) = queue.Dequeue();
      var currentTile = _output[x, y];

      if (currentTile == -1)
        continue;

      var currentPoint = new TilePoint(x, y);
      if (y > 0 && !ConstrainNeighbor(x, y - 1, Direction.South, currentTile, currentPoint))
        return false;

      if (y < _height - 1 && !ConstrainNeighbor(x, y + 1, Direction.North, currentTile, currentPoint))
        return false;

      if (x < _width - 1 && !ConstrainNeighbor(x + 1, y, Direction.West, currentTile, currentPoint))
        return false;

      if (x > 0 && !ConstrainNeighbor(x - 1, y, Direction.East, currentTile, currentPoint))
        return false;

      if (y > 0 && _possibilities[x, y - 1] != null) queue.Enqueue((x, y - 1));
      if (y < _height - 1 && _possibilities[x, y + 1] != null) queue.Enqueue((x, y + 1));
      if (x < _width - 1 && _possibilities[x + 1, y] != null) queue.Enqueue((x + 1, y));
      if (x > 0 && _possibilities[x - 1, y] != null) queue.Enqueue((x - 1, y));
    }

    return true;
  }

  private bool Propagate(int startX, int startY, ChangeLog log)
  {
    Queue<(int x, int y)> queue = new();
    queue.Enqueue((startX, startY));

    while (queue.Count > 0)
    {
      var (x, y) = queue.Dequeue();
      var currentTile = _output[x, y];
      if (currentTile == -1)
        continue;

      var currentPoint = new TilePoint(x, y);

      if (y > 0 && !ConstrainAndRecord(x, y - 1, Direction.South, currentTile, currentPoint, log)) return false;
      if (y < _height - 1 && !ConstrainAndRecord(x, y + 1, Direction.North, currentTile, currentPoint, log)) return false;
      if (x < _width - 1 && !ConstrainAndRecord(x + 1, y, Direction.West, currentTile, currentPoint, log)) return false;
      if (x > 0 && !ConstrainAndRecord(x - 1, y, Direction.East, currentTile, currentPoint, log)) return false;

      if (y > 0 && _possibilities[x, y - 1] != null) queue.Enqueue((x, y - 1));
      if (y < _height - 1 && _possibilities[x, y + 1] != null) queue.Enqueue((x, y + 1));
      if (x < _width - 1 && _possibilities[x + 1, y] != null) queue.Enqueue((x + 1, y));
      if (x > 0 && _possibilities[x - 1, y] != null) queue.Enqueue((x - 1, y));
    }

    return true;
  }

  private bool ConstrainAndRecord(int x, int y, Direction directionToNeighbor, int neighborTileId, TilePoint neighborPosition, ChangeLog log)
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

    foreach (var tile in possibilities.ToList())
    {
      if (!allowed.Contains(tile))
      {
        possibilities.Remove(tile);
        log.RecordDomainRemoved(x, y, tile);
      }
    }

    if (possibilities.Count == 0)
    {
      return false;
    }

    if (possibilities.Count == 1)
    {
      var chosen = possibilities.First();
      log.RecordCellCollapsed(x, y, possibilities, chosen);
      var prev = _output[x, y];
      log.RecordOutputSet(x, y, prev, chosen);
      _output[x, y] = chosen;
      _possibilities[x, y] = null;
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
      TerrainPerformanceEventSource.Log.WfcContradiction(0, x, y);
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
