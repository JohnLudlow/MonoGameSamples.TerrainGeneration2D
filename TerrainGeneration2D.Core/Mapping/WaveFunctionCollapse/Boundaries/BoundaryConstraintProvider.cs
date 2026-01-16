using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.Boundaries;

/// <summary>
/// Default implementation of <see cref="IBoundaryConstraintProvider"/> for managing chunk boundary constraints.
/// </summary>
/// <remarks>
/// Ensures seamless transitions between chunks by extracting and applying boundary constraints for WFC domains.
/// </remarks>
public class BoundaryConstraintProvider : IBoundaryConstraintProvider
{
  /// <summary>
  /// Extracts boundary constraints from a neighboring chunk along the specified shared edge.
  /// </summary>
  /// <param name="neighborChunk">The neighboring chunk to extract constraints from.</param>
  /// <param name="sharedEdge">The direction of the shared edge (North, South, East, West).</param>
  /// <returns>An array of <see cref="BoundaryConstraint"/> representing the constraints for the shared edge.</returns>
  public BoundaryConstraint[] ExtractConstraints(Chunk neighborChunk, Direction sharedEdge)
  {
    var constraints = new BoundaryConstraint[Chunk.ChunkSize];

    // Example: Extract from North neighbor's South edge
    if (sharedEdge == Direction.North)
    {
      // Get tiles along neighbor's bottom row (y = ChunkSize-1)
      for (int x = 0; x < Chunk.ChunkSize; x++)
      {
        var tileId = neighborChunk[x, Chunk.ChunkSize - 1];
        constraints[x] = new BoundaryConstraint
        {
          Position = x,
          RequiredTileId = tileId,
          Side = Direction.North
        };
      }
    }
    else if (sharedEdge == Direction.East)
    {
      // Get tiles along neighbor's left column (x = 0)
      for (int y = 0; y < Chunk.ChunkSize; y++)
      {
        var tileId = neighborChunk[0, y];
        constraints[y] = new BoundaryConstraint
        {
          Position = y,
          RequiredTileId = tileId,
          Side = Direction.East
        };
      }
    }
    // Similar logic for South and West...

    return constraints;
  }

  /// <summary>
  /// Applies boundary constraints to the WFC domain grid before solving begins.
  /// </summary>
  /// <param name="domains">The WFC domain grid to constrain (nullable jagged array matching WfcProvider pattern).</param>
  /// <param name="constraints">The boundary constraints to apply.</param>
  public void ApplyConstraints(HashSet<int>?[][] domains, BoundaryConstraint[] constraints)
  {
    foreach (var constraint in constraints)
    {
      int x, y;

      // Convert constraint to domain grid coordinates
      switch (constraint.Side)
      {
        case Direction.North:
          x = constraint.Position;
          y = 0; // Top row of current chunk
          break;
        case Direction.East:
          x = Chunk.ChunkSize - 1; // Right column
          y = constraint.Position;
          break;
        case Direction.South:
          x = constraint.Position;
          y = Chunk.ChunkSize - 1; // Bottom row
          break;
        case Direction.West:
          x = 0; // Left column
          y = constraint.Position;
          break;
        default:
          continue;
      }

      // Constrain domain to only the required tile (handle nullable arrays)
      if (domains[x][y] != null)
      {
        domains[x][y]!.Clear();
        domains[x][y]!.Add(constraint.RequiredTileId);
      }
    }
  }
}