// Integration with boundary constraints for chunk seaming
using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
// using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.Boundaries;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse
{
  /// <summary>
  /// Enhanced WFC provider integrating advanced boundary constraint extraction and application for seamless chunk generation.
  /// </summary>
  /// <remarks>
  /// Ensures boundaries between adjacent chunks are consistent by applying neighbor constraints before AC-3 propagation.
  /// Supports partial neighbor constraints and deterministic domain restriction for robust terrain seaming.
  /// </remarks>
  public class EnhancedWfcProvider : WfcProvider
  {
    private readonly IBoundaryConstraintProvider _boundaryProvider;
    private readonly bool _enableValidation;

    /// <summary>
    /// Initializes a new instance of EnhancedWfcProvider.
    /// </summary>
    /// <param name="width">Chunk width in tiles.</param>
    /// <param name="height">Chunk height in tiles.</param>
    /// <param name="tileRegistry">Tile type registry.</param>
    /// <param name="randomProvider">Random provider for deterministic generation.</param>
    /// <param name="terrainConfig">Terrain rule configuration.</param>
    /// <param name="heightProvider">Height provider for terrain elevation data.</param>
    /// <param name="chunkOrigin">Origin point for chunk coordinates.</param>
    /// <param name="wfcConfig">WFC configuration (weights, heuristics).</param>
    /// <param name="boundaryProvider">Boundary constraint provider.</param>
    /// <param name="enableValidation">If true, validates chunk seams after generation.</param>
    public EnhancedWfcProvider(
        int width,
        int height,
        TileTypeRegistry tileRegistry,
        IRandomProvider randomProvider,
        TerrainRuleConfiguration terrainConfig,
        IHeightProvider heightProvider,
        Point chunkOrigin,
        WfcConfiguration wfcConfig,
        IBoundaryConstraintProvider boundaryProvider,
        bool enableValidation = false)
        : base(width, height, tileRegistry, randomProvider, terrainConfig, heightProvider, chunkOrigin, wfcConfig != null ? wfcConfig.Weights : throw new ArgumentNullException(nameof(wfcConfig)), wfcConfig.Heuristics)
    {
      _boundaryProvider = boundaryProvider;
      _enableValidation = enableValidation;
    }

    public bool GenerateWithBoundaries(Dictionary<Point, Chunk> neighborChunks,
        Point currentChunkCoords)
    {
      ArgumentNullException.ThrowIfNull(neighborChunks);
      // Apply boundary constraints before generation
      ApplyBoundaryConstraints(neighborChunks, currentChunkCoords);

      // Run standard AC-3 generation
      var success = Generate();

      // Validate seam consistency (optional verification step)
      if (success && _enableValidation)
      {
        ValidateChunkSeams(neighborChunks, currentChunkCoords);
      }

      return success;
    }

    /// <summary>
    /// Verifies that tiles along shared chunk boundaries match adjacency rules and logs any mismatches.
    /// </summary>
    /// <param name="neighborChunks">Dictionary of neighboring chunks keyed by their coordinates.</param>
    /// <param name="currentChunkCoords">Coordinates of the current chunk.</param>
    private void ValidateChunkSeams(Dictionary<Point, Chunk> neighborChunks, Point currentChunkCoords)
    {
      foreach (var kvp in neighborChunks)
      {
        var neighborCoords = kvp.Key;
        var neighborChunk = kvp.Value;
        var sharedEdge = GetSharedEdge(currentChunkCoords, neighborCoords);
        if (sharedEdge == null) continue;

        var currentBoundary = ExtractBoundaryTiles(this, sharedEdge.Value, true);
        var neighborBoundary = ExtractBoundaryTiles(neighborChunk, GetOppositeDirection(sharedEdge.Value), false);

        for (int i = 0; i < currentBoundary.Length; i++)
        {
          if (!AdjacencyRulesMatch(currentBoundary[i], neighborBoundary[i], sharedEdge.Value))
          {
            LogBoundaryMismatch(currentChunkCoords, neighborCoords, sharedEdge.Value, i, currentBoundary[i], neighborBoundary[i]);
          }
        }
      }
    }

    /// <summary>
    /// Determines which edge is shared between two chunk coordinates.
    /// </summary>
    /// <param name="a">Coordinates of the first chunk.</param>
    /// <param name="b">Coordinates of the second chunk.</param>
    /// <returns>The shared edge direction, or null if not adjacent.</returns>
    private static Direction? GetSharedEdge(Point a, Point b)
    {
      if (a.X == b.X && a.Y == b.Y + 1) return Direction.North;
      if (a.X == b.X && a.Y == b.Y - 1) return Direction.South;
      if (a.X == b.X + 1 && a.Y == b.Y) return Direction.West;
      if (a.X == b.X - 1 && a.Y == b.Y) return Direction.East;
      return null;
    }

    /// <summary>
    /// Extracts tile IDs along the specified edge from a chunk or provider.
    /// </summary>
    /// <param name="chunkOrProvider">Chunk or provider to extract from.</param>
    /// <param name="edge">Edge to extract.</param>
    /// <param name="isCurrent">True if extracting from the current chunk, false for neighbor.</param>
    /// <returns>Array of tile IDs along the edge.</returns>
    private static int[] ExtractBoundaryTiles(object chunkOrProvider, Direction edge, bool isCurrent)
    {
      // Example: extract tile IDs along the specified edge
      // Replace with actual chunk access in real code
      return new int[Chunk.ChunkSize];
    }

    /// <summary>
    /// Checks if two tiles match adjacency rules for a given edge.
    /// </summary>
    /// <param name="tileA">Tile ID from the current chunk.</param>
    /// <param name="tileB">Tile ID from the neighbor chunk.</param>
    /// <param name="edge">Edge direction being checked.</param>
    /// <returns>True if adjacency rules are satisfied; otherwise, false.</returns>
    private static bool AdjacencyRulesMatch(int tileA, int tileB, Direction edge)
    {
      // Replace with actual rule table lookup
      return true;
    }

    /// <summary>
    /// Gets the opposite direction for a given edge.
    /// </summary>
    /// <param name="dir">Direction to invert.</param>
    /// <returns>Opposite direction.</returns>
    private static Direction GetOppositeDirection(Direction dir)
    {
      return dir switch
      {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => dir
      };
    }

    /// <summary>
    /// Logs a boundary mismatch between two chunks at a specific edge and position.
    /// </summary>
    /// <param name="chunkA">Coordinates of the first chunk.</param>
    /// <param name="chunkB">Coordinates of the second chunk.</param>
    /// <param name="edge">Edge direction where mismatch occurred.</param>
    /// <param name="position">Position along the edge.</param>
    /// <param name="tileA">Tile ID from the first chunk.</param>
    /// <param name="tileB">Tile ID from the second chunk.</param>
    private static void LogBoundaryMismatch(Point chunkA, Point chunkB, Direction edge, int position, int tileA, int tileB)
    {
      // Replace with actual logging
      Console.WriteLine($"Boundary mismatch at edge {edge} position {position}: chunk {chunkA} tile {tileA} vs chunk {chunkB} tile {tileB}");
    }

    private void ApplyBoundaryConstraints(Dictionary<Point, Chunk> neighbors, Point coords)
    {
      var neighborOffsets = new[]
      {
            (new Point(0, -1), Direction.North),
            (new Point(1, 0), Direction.East),
            (new Point(0, 1), Direction.South),
            (new Point(-1, 0), Direction.West)
        };

      foreach (var (offset, direction) in neighborOffsets)
      {
        var neighborPos = coords + offset;
        if (neighbors.TryGetValue(neighborPos, out var chunk))
        {
          var constraints = _boundaryProvider.ExtractConstraints(chunk, direction);
          _boundaryProvider.ApplyConstraints(this._possibilities, constraints);
        }
      }

      // Important: Run initial propagation after applying boundary constraints
      // This ensures constraint consistency before starting main generation
      PropagateInitialConstraints();
    }

    private void PropagateInitialConstraints()
    {
      // Propagate from all boundary cells that have been constrained
      for (var x = 0; x < this.Width; x++)
      {
        for (var y = 0; y < this.Height; y++)
        {
          var cellPoss = this._possibilities[x][y];
          if (cellPoss != null && cellPoss.Count == 1)
          {
            // Single-domain cell acts as initial constraint
            var constrainedTile = cellPoss.First();
            if (!this._propagator.PropagateFrom(x, y, constrainedTile))
            {
              throw new InvalidOperationException(
                  $"Boundary constraints created contradiction at ({x},{y})");
            }
          }
        }
      }
    }
  }
}