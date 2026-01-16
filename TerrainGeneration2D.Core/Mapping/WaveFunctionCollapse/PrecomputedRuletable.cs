// Rule table implementation that converts TileTypeRegistry to AC3 format
using System;
using System.Linq;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Precomputed rule table implementation that converts TileTypeRegistry adjacency rules 
/// into efficient BitSet lookup tables for O(1) constraint checking during WFC solving.
/// </summary>
/// <remarks>
/// Built once during initialization to eliminate runtime rule evaluation costs.
/// Uses BitSet data structures for efficient set operations on tile ID collections.
/// </remarks>
public class PrecomputedRuleTable : IRuleTable
{
  private readonly Dictionary<(int tileId, Direction dir), BitSet> _allowedNeighbors;


  /// <summary>
  /// Initializes a new precomputed rule table from the specified tile registry.
  /// </summary>
  /// <param name="registry">Tile registry containing adjacency rules to precompute</param>
  /// <exception cref="ArgumentNullException">Thrown when registry is null</exception>
  public PrecomputedRuleTable(TileTypeRegistry registry)
  {
    ArgumentNullException.ThrowIfNull(registry);

    _allowedNeighbors = [];

    PrecomputeAllRules(registry);
  }

  /// <summary>
  /// Gets the set of allowed neighboring tile IDs for a given tile in a specific direction.
  /// </summary>
  /// <param name="tileId">Source tile ID to check neighbors for</param>
  /// <param name="direction">Direction to check (North, South, East, West)</param>
  /// <returns>BitSet containing allowed neighbor tile IDs; empty set if no constraints</returns>
  public BitSet GetAllowedNeighbors(int tileId, Direction direction)
  {
    return _allowedNeighbors.GetValueOrDefault((tileId, direction), new BitSet(0));
  }

  /// <summary>
  /// Precomputes all adjacency rules by testing every tile-direction-neighbor combination
  /// and storing results in efficient BitSet lookup tables.
  /// </summary>
  /// <param name="registry">Tile registry containing rules to evaluate</param>
  /// <remarks>
  /// Creates <see cref="TileRuleContext"/> objects with default values for testing basic adjacency rules
  /// without runtime-specific data like height samples or mapping information.
  /// </remarks>
  private void PrecomputeAllRules(TileTypeRegistry registry)
  {
    // Convert TileType adjacency rules into efficient BitSet lookups
    var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

    // Helper: pick valid context for each tile type
    TerrainRuleConfiguration config = new TerrainRuleConfiguration();
    for (int tileId = 0; tileId < registry.TileCount; tileId++)
    {
      var tileType = registry.GetTileType(tileId);

      // Pick valid height sample for this tile type
      HeightSample validHeight = tileType switch
      {
        OceanTileType => new HeightSample { Altitude = config.OceanHeightMax - 0.01f },
        BeachTileType => new HeightSample { Altitude = config.BeachHeightMin + 0.01f },
        PlainsTileType => new HeightSample { Altitude = config.PlainsHeightMin + 0.01f },
        ForestTileType => new HeightSample { Altitude = config.ForestHeightMin + 0.01f },
        SnowTileType => new HeightSample { Altitude = config.SnowHeightMin + 0.01f },
        MountainTileType => new HeightSample { Altitude = config.MountainHeightMin + 0.01f, MountainNoise = config.MountainNoiseThreshold + 0.01f },
        _ => new HeightSample { Altitude = 0.5f }
      };

      foreach (var direction in directions)
      {
        var allowedSet = new BitSet(registry.TileCount);

        // Test each potential neighbor tile
        for (var neighborId = 0; neighborId < registry.TileCount; neighborId++)
        {
          var context = new TileRuleContext(
            CandidatePosition: new TilePoint(0, 0),
            CandidateTileId: tileId,
            NeighborPosition: GetNeighborPosition(direction),
            NeighborTileId: neighborId,
            DirectionToNeighbor: direction,
            Config: config,
            CandidateHeight: validHeight,
            NeighborHeight: validHeight,
            MappingService: new MappingInformationService([new int[1]])
          );

          if (tileType.EvaluateRules(context))
          {
            allowedSet.Add(neighborId);
          }
        }

        _allowedNeighbors[(tileId, direction)] = allowedSet;
      }
    }
  }

  /// <summary>
  /// Gets the neighbor position coordinates in the specified direction from origin (0,0).
  /// </summary>
  /// <param name="direction">Direction to get neighbor position for</param>
  /// <returns>TilePoint representing neighbor position relative to origin</returns>
  /// <remarks>
  /// Used during rule precomputation to create consistent TileRuleContext objects.
  /// </remarks>
  private static TilePoint GetNeighborPosition(Direction direction)
  {
    return direction switch
    {
      Direction.North => new TilePoint(0, -1),
      Direction.South => new TilePoint(0, 1),
      Direction.East => new TilePoint(1, 0),
      Direction.West => new TilePoint(-1, 0),
      _ => new TilePoint(0, 0)
    };
  }
}