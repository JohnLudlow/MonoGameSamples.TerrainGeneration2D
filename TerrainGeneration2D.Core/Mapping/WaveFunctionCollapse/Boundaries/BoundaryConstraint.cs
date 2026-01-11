using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.Boundaries;

/// <summary>
/// Represents a constraint for a single position along a chunk boundary.
/// </summary>
public struct BoundaryConstraint
{
  public int Position { get; init; }        // 0-63 position along boundary
  public int RequiredTileId { get; init; }  // Tile that must be placed
  public Direction Side { get; init; }      // Which boundary edge this applies to
}