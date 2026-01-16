using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

/// <summary>
/// Represents a chunk of tiles in the world (64x64 tiles)
/// </summary>
public class Chunk
{
  public const int ChunkSize = 64;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
  private readonly int[,] _tiles;
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
  private bool _isDirty;

  /// <summary>
  /// Gets the chunk coordinates (not tile coordinates)
  /// </summary>
  public Point ChunkPosition { get; }

  /// <summary>
  /// Gets or sets whether this chunk has been modified since last save
  /// </summary>
  public bool IsDirty
  {
    get => _isDirty;
    set => _isDirty = value;
  }

  public Chunk(Point chunkPosition)
  {
    ChunkPosition = chunkPosition;
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
    _tiles = new int[ChunkSize, ChunkSize];
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
    _isDirty = false;
  }

  /// <summary>
  /// Gets or sets a tile ID at the specified local chunk coordinates (0-63, 0-63)
  /// </summary>
  public int this[int localX, int localY]
  {
    get => _tiles[localX, localY];
    set
    {
      _tiles[localX, localY] = value;
      _isDirty = true;
    }
  }

  /// <summary>
  /// Gets the tile array (for serialization)
  /// </summary>
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1819 // Prefer jagged arrays over multidimensional
  public int[,] Tiles => _tiles;
#pragma warning restore CA1819 // Prefer jagged arrays over multidimensional
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

  /// <summary>
  /// Gets the world tile position of the top-left corner of this chunk
  /// </summary>
  public Point WorldTilePosition => new Point(ChunkPosition.X * ChunkSize, ChunkPosition.Y * ChunkSize);
}