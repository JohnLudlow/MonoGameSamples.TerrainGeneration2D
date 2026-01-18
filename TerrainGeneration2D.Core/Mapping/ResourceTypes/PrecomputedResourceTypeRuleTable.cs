// Rule table implementation that converts TileTypeRegistry to AC3 format
using System;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

/// <summary>
/// Precomputed rule table implementation that converts TileTypeRegistry adjacency rules 
/// into efficient BitSet lookup tables for O(1) constraint checking during WFC solving.
/// </summary>
/// <remarks>
/// Built once during initialization to eliminate runtime rule evaluation costs.
/// Uses BitSet data structures for efficient set operations on tile ID collections.
/// </remarks>
public class PrecomputedResourceTypeRuleTable : IRuleTable, IRuleTable<int>
{
  private readonly Dictionary<(int tileId, Direction dir), BitSet> _allowedNeighbors;


  /// <summary>
  /// Initializes a new precomputed rule table from the specified tile registry.
  /// </summary>
  /// <param name="registry">Tile registry containing adjacency rules to precompute</param>
  /// <exception cref="ArgumentNullException">Thrown when registry is null</exception>
  public PrecomputedResourceTypeRuleTable(ResourceTypeRegistry registry)
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
  /// Gets the set of allowed neighboring tile IDs for a given tile in a specific direction.
  /// </summary>
  /// <param name="tileId">Source tile ID to check neighbors for</param>
  /// <param name="direction">Direction to check (North, South, East, West)</param>
  /// <returns>BitSet containing allowed neighbor tile IDs; empty set if no constraints</returns>
  IEnumerable<int> IRuleTable<int>.GetAllowedNeighbors(int tileId, Direction direction)
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
  private void PrecomputeAllRules(ResourceTypeRegistry registry)
  {
    // Convert ResourceType adjacency rules into efficient BitSet lookups
    var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

    // Use new flexible configuration
    var config = new TileTypeRuleConfiguration(); // Or ResourceTypeRuleConfiguration if available
    // Optionally: populate config.Rules with sensible defaults if needed

    for (var typeId = 0; typeId < registry.TileCount; typeId++)
    {
      var resourceType = registry.GetResourceType(typeId);
      var rule = config.GetRuleForType(typeId);

      // Pick valid height sample for this resource type using rule if present
      HeightSample validHeight;
      if (rule != null)
      {
        validHeight = new HeightSample
        {
          Altitude = rule.ElevationMin + 0.01f,
          MountainNoise = rule.NoiseThreshold.HasValue ? rule.NoiseThreshold.Value + 0.01f : 0.5f
        };
      }
      else
      {
        validHeight = new HeightSample { Altitude = 0.5f };
      }

      foreach (var direction in directions)
      {
        var allowedSet = new BitSet(registry.TileCount);

        // Test each potential neighbor resource
        for (var neighborId = 0; neighborId < registry.TileCount; neighborId++)
        {
          var context = new ResourceRuleContext(
            CandidatePosition: new TilePoint(0, 0),
            CandidateTileId: typeId,
            NeighborPosition: GetNeighborPosition(direction),
            NeighborTileId: neighborId,
            DirectionToNeighbor: direction,
            Config: config,
            CandidateHeight: validHeight,
            NeighborHeight: validHeight,
            MappingService: new MappingInformationService([new int[1]])
          );

          if (resourceType.EvaluateRules(context))
          {
            allowedSet.Add(neighborId);
          }
        }

        _allowedNeighbors[(typeId, direction)] = allowedSet;
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