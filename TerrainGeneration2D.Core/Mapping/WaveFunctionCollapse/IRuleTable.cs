// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IRuleTable.cs


// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IRuleTable.cs

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
