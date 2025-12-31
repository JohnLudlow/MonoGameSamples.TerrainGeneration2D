using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

/// <summary>
/// Manages a large tilemap using chunks for efficient memory usage
/// </summary>
public class ChunkedTilemap
{
    private readonly Dictionary<Point, Chunk> _activeChunks;
    private readonly Tileset _tileset;
    private readonly int _tileSize;
    private readonly int _mapSizeInTiles;
    private readonly int _mapSizeInChunks;
    private readonly int _masterSeed;
    private readonly string _saveDirectory;
    private readonly Random _random;
    private readonly TileTypeRegistry _tileTypeRegistry;
    private readonly TerrainRuleConfiguration _terrainRuleConfig;
    private readonly bool _useWaveFunctionCollapse;
    
    public int TileSize => _tileSize;
    public int MapSizeInTiles => _mapSizeInTiles;
    public Tileset Tileset => _tileset;
    
    public ChunkedTilemap(Tileset tileset, int mapSizeInTiles, int masterSeed, string saveDirectory, bool useWaveFunctionCollapse = true, TerrainRuleConfiguration? terrainRuleConfiguration = null)
    {
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        _tileSize = tileset.TileWidth;
        _mapSizeInTiles = mapSizeInTiles;
        _mapSizeInChunks = (int)Math.Ceiling((double)mapSizeInTiles / Chunk.ChunkSize);
        _masterSeed = masterSeed;
        _saveDirectory = saveDirectory;
        _activeChunks = new Dictionary<Point, Chunk>();
        _random = new Random();
        _useWaveFunctionCollapse = useWaveFunctionCollapse;
        _terrainRuleConfig = terrainRuleConfiguration ?? new TerrainRuleConfiguration();
        _tileTypeRegistry = TileTypeRegistry.CreateDefault(tileset.Count, _terrainRuleConfig);
        
        // Ensure save directory exists
        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }
    
    /// <summary>
    /// Converts world tile coordinates to chunk coordinates
    /// </summary>
    public static Point TileToChunkCoordinates(int tileX, int tileY)
    {
        return new Point(
            tileX >= 0 ? tileX / Chunk.ChunkSize : (tileX - Chunk.ChunkSize + 1) / Chunk.ChunkSize,
            tileY >= 0 ? tileY / Chunk.ChunkSize : (tileY - Chunk.ChunkSize + 1) / Chunk.ChunkSize
        );
    }
    
    /// <summary>
    /// Gets or creates a chunk at the specified chunk coordinates
    /// </summary>
    private Chunk GetOrCreateChunk(Point chunkCoords)
    {
        if (_activeChunks.TryGetValue(chunkCoords, out var chunk))
        {
            return chunk;
        }
        
        // Try to load from disk first
        chunk = LoadChunk(chunkCoords);
        
        // If not on disk, generate new chunk
        if (chunk == null)
        {
            chunk = GenerateChunk(chunkCoords);
        }
        
        _activeChunks[chunkCoords] = chunk;
        return chunk;
    }
    
    /// <summary>
    /// Generates a new chunk using Wave Function Collapse or random generation
    /// </summary>
    private Chunk GenerateChunk(Point chunkCoords)
    {
        var chunk = new Chunk(chunkCoords);
        
        // Generate deterministic seed for this chunk
        int chunkSeed = _masterSeed + chunkCoords.X * 73856093 + chunkCoords.Y * 19349663;
        var random = new Random(chunkSeed);
        
        if (_useWaveFunctionCollapse)
        {
            // Use Wave Function Collapse for coherent terrain
            var wfc = new WaveFunctionCollapse(Chunk.ChunkSize, Chunk.ChunkSize, _tileTypeRegistry, random);
            
            if (wfc.Generate())
            {
                var output = wfc.GetOutput();
                for (int localY = 0; localY < Chunk.ChunkSize; localY++)
                {
                    for (int localX = 0; localX < Chunk.ChunkSize; localX++)
                    {
                        chunk[localX, localY] = output[localX, localY];
                    }
                }
            }
            else
            {
                // WFC failed (contradiction), fall back to random
                GenerateRandomChunk(chunk, random);
            }
        }
        else
        {
            // Use simple random generation
            GenerateRandomChunk(chunk, random);
        }
        
        chunk.IsDirty = true; // Mark as dirty so it gets saved
        return chunk;
    }

    /// <summary>
    /// Fill chunk with random tiles (fallback method)
    /// </summary>
    private void GenerateRandomChunk(Chunk chunk, Random random)
    {
        for (int localY = 0; localY < Chunk.ChunkSize; localY++)
        {
            for (int localX = 0; localX < Chunk.ChunkSize; localX++)
            {
                chunk[localX, localY] = random.Next(0, _tileset.Count);
            }
        }
    }
    
    /// <summary>
    /// Loads a chunk from disk
    /// </summary>
    private Chunk? LoadChunk(Point chunkCoords)
    {
        string filePath = GetChunkFilePath(chunkCoords);
        
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            using var fileStream = File.OpenRead(filePath);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var reader = new BinaryReader(gzipStream);
            
            // Read and validate header
            byte[] magic = reader.ReadBytes(4);
            if (magic[0] != 'C' || magic[1] != 'H' || magic[2] != 'N' || magic[3] != 'K')
            {
                return null;
            }
            
            int version = reader.ReadInt32();
            if (version != 1)
            {
                return null;
            }
            
            int chunkX = reader.ReadInt32();
            int chunkY = reader.ReadInt32();
            
            if (chunkX != chunkCoords.X || chunkY != chunkCoords.Y)
            {
                return null;
            }
            
            // Read tile data
            var chunk = new Chunk(chunkCoords);
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                for (int x = 0; x < Chunk.ChunkSize; x++)
                {
                    chunk[x, y] = reader.ReadInt32();
                }
            }
            
            chunk.IsDirty = false;
            return chunk;
        }
        catch (Exception)
        {
            // Failed to load chunk, will regenerate
            return null;
        }
    }
    
    /// <summary>
    /// Saves a chunk to disk
    /// </summary>
    private void SaveChunk(Chunk chunk)
    {
        if (!chunk.IsDirty)
        {
            return;
        }
        
        string filePath = GetChunkFilePath(chunk.ChunkPosition);
        
        try
        {
            using var fileStream = File.Create(filePath);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new BinaryWriter(gzipStream);
            
            // Write header
            writer.Write(new[] { (byte)'C', (byte)'H', (byte)'N', (byte)'K' });
            writer.Write(1); // Version
            writer.Write(chunk.ChunkPosition.X);
            writer.Write(chunk.ChunkPosition.Y);
            
            // Write tile data
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                for (int x = 0; x < Chunk.ChunkSize; x++)
                {
                    writer.Write(chunk[x, y]);
                }
            }
            
            chunk.IsDirty = false;
        }
        catch (Exception)
        {
            // Ignore save errors for now
        }
    }
    
    /// <summary>
    /// Gets the file path for a chunk
    /// </summary>
    private string GetChunkFilePath(Point chunkCoords)
    {
        return Path.Combine(_saveDirectory, $"map_{chunkCoords.X}_{chunkCoords.Y}.dat");
    }
    
    /// <summary>
    /// Updates active chunks based on camera viewport
    /// </summary>
    public void UpdateActiveChunks(Rectangle viewportWorldBounds)
    {
        // Calculate visible chunk range
        Point minChunk = TileToChunkCoordinates(
            viewportWorldBounds.Left / _tileSize,
            viewportWorldBounds.Top / _tileSize
        );
        
        Point maxChunk = TileToChunkCoordinates(
            viewportWorldBounds.Right / _tileSize,
            viewportWorldBounds.Bottom / _tileSize
        );
        
        // Expand by 1 chunk in each direction for buffer
        minChunk.X = Math.Max(0, minChunk.X - 1);
        minChunk.Y = Math.Max(0, minChunk.Y - 1);
        maxChunk.X = Math.Min(_mapSizeInChunks - 1, maxChunk.X + 1);
        maxChunk.Y = Math.Min(_mapSizeInChunks - 1, maxChunk.Y + 1);
        
        // Load visible chunks
        for (int cy = minChunk.Y; cy <= maxChunk.Y; cy++)
        {
            for (int cx = minChunk.X; cx <= maxChunk.X; cx++)
            {
                GetOrCreateChunk(new Point(cx, cy));
            }
        }
        
        // Unload distant chunks
        var chunksToUnload = new List<Point>();
        foreach (var kvp in _activeChunks)
        {
            Point chunkPos = kvp.Key;
            if (chunkPos.X < minChunk.X - 1 || chunkPos.X > maxChunk.X + 1 ||
                chunkPos.Y < minChunk.Y - 1 || chunkPos.Y > maxChunk.Y + 1)
            {
                chunksToUnload.Add(chunkPos);
            }
        }
        
        foreach (var chunkPos in chunksToUnload)
        {
            SaveChunk(_activeChunks[chunkPos]);
            _activeChunks.Remove(chunkPos);
        }
    }
    
    /// <summary>
    /// Gets a tile ID at the specified world tile coordinates
    /// </summary>
    public int GetTile(int tileX, int tileY)
    {
        Point chunkCoords = TileToChunkCoordinates(tileX, tileY);
        Chunk chunk = GetOrCreateChunk(chunkCoords);
        
        int localX = tileX - chunk.WorldTilePosition.X;
        int localY = tileY - chunk.WorldTilePosition.Y;
        
        return chunk[localX, localY];
    }
    
    /// <summary>
    /// Sets a tile ID at the specified world tile coordinates
    /// </summary>
    public void SetTile(int tileX, int tileY, int tileId)
    {
        Point chunkCoords = TileToChunkCoordinates(tileX, tileY);
        Chunk chunk = GetOrCreateChunk(chunkCoords);
        
        int localX = tileX - chunk.WorldTilePosition.X;
        int localY = tileY - chunk.WorldTilePosition.Y;
        
        chunk[localX, localY] = tileId;
    }
    
    /// <summary>
    /// Draws visible tiles within the viewport
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Rectangle viewportWorldBounds)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        
        // Calculate visible tile range
        int minTileX = Math.Max(0, viewportWorldBounds.Left / _tileSize);
        int minTileY = Math.Max(0, viewportWorldBounds.Top / _tileSize);
        int maxTileX = Math.Min(_mapSizeInTiles - 1, viewportWorldBounds.Right / _tileSize);
        int maxTileY = Math.Min(_mapSizeInTiles - 1, viewportWorldBounds.Bottom / _tileSize);
        
        // Draw only visible tiles
        for (int tileY = minTileY; tileY <= maxTileY; tileY++)
        {
            for (int tileX = minTileX; tileX <= maxTileX; tileX++)
            {
                int tileId = GetTile(tileX, tileY);
                TextureRegion region = _tileset.GetTile(tileId);
                
                var position = new Vector2(tileX * _tileSize, tileY * _tileSize);
                spriteBatch.Draw(region.Texture, position, region.SourceRectangle, Color.White);
            }
        }
    }
    
    /// <summary>
    /// Saves all dirty chunks
    /// </summary>
    public void SaveAll()
    {
        foreach (var chunk in _activeChunks.Values)
        {
            SaveChunk(chunk);
        }
    }
}
