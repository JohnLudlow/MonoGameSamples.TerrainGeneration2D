// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IRuleTable.cs


// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IRuleTable.cs

using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;


/// <summary>
/// Precomputed rule table for efficient adjacency lookups during WFC solving.
/// </summary>
/// <remarks>
/// Built once at initialization; avoid allocations during hot path solving.
/// </remarks>
public interface IRuleTable
{
  /// <summary>
  /// Gets allowed neighboring tile IDs for a given tile in a specific direction.
  /// </summary>
  /// <param name="tileId">Source tile ID</param>
  /// <param name="direction">Direction to check (North, South, East, West)</param>
  /// <returns>BitSet of allowed neighbor tile IDs for O(1) intersection operations</returns>
  BitSet GetAllowedNeighbors(int tileId, Direction direction);
}

/// <summary>
/// Precomputed rule table for efficient adjacency lookups during WFC solving.
/// </summary>
/// <remarks>
/// Built once at initialization; avoid allocations during hot path solving.
/// </remarks>
/// <typeparam name="TValue">Value type for which constraints are defined</typeparam>
public interface IRuleTable<TValue> : IRuleTable
{
    /// <summary>
    /// Gets allowed neighboring values for a given value in a specific direction.
    /// </summary>
    /// <param name="value">The source value to check neighbors for</param>
    /// <param name="direction">The direction to check (North, South, East, West)</param>
    /// <returns>Enumeration of allowed neighboring values</returns>
    IEnumerable<TValue> GetAllowedNeighbors(TValue value, Direction direction);
}