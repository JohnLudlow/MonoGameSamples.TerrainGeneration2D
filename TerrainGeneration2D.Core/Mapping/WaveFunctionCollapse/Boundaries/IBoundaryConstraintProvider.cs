using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.Boundaries;

/// <summary>
/// Manages boundary constraints between adjacent chunks for seamless terrain.
/// </summary>
/// <remarks>
/// Extracts constraints from neighboring chunks before WFC initialization.
/// </remarks>
public interface IBoundaryConstraintProvider
{
  /// <summary>
  /// Extracts boundary constraints from an already-generated neighboring chunk.
  /// </summary>
  /// <param name="neighborChunk">Source chunk to extract constraints from</param>
  /// <param name="sharedEdge">Which edge is shared (North, South, East, West)</param>
  /// <returns>Array of tile constraints for the shared boundary</returns>
  BoundaryConstraint[] ExtractConstraints(Chunk neighborChunk, Direction sharedEdge);

  /// <summary>
  /// Applies boundary constraints to WFC domains before solving begins.
  /// </summary>
  /// <param name="domains">WFC domain grid to constrain (nullable jagged array matching WfcProvider pattern)</param>
  /// <param name="constraints">Boundary constraints to apply</param>
  void ApplyConstraints(HashSet<int>?[][] domains, BoundaryConstraint[] constraints);
}