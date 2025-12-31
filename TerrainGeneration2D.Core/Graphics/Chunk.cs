using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

/// <summary>
/// Represents a chunk of tiles in the world (64x64 tiles)
/// </summary>
public class Chunk
{
    public const int ChunkSize = 64;
    
    private readonly int[,] _tiles;
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
        _tiles = new int[ChunkSize, ChunkSize];
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
    public int[,] Tiles => _tiles;
    
    /// <summary>
    /// Gets the world tile position of the top-left corner of this chunk
    /// </summary>
    public Point WorldTilePosition => new Point(ChunkPosition.X * ChunkSize, ChunkPosition.Y * ChunkSize);
}
