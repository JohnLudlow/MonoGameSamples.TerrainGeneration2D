using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Extensions.Logging;
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
  private readonly HeightMapConfiguration _heightMapConfiguration;
  private readonly WfcWeightConfiguration _wfcWeightConfig;
  private readonly HeuristicsConfiguration _heuristicsConfig;
  private readonly IHeightProvider _heightProvider;
  private readonly bool _useWaveFunctionCollapse;
  private readonly ILogger? _logger;
  private int _wfcTimeBudgetMs;

  public int TileSize => _tileSize;
  public int MapSizeInTiles => _mapSizeInTiles;
  public Tileset Tileset => _tileset;

  public ChunkedTilemap(Tileset tileset, int mapSizeInTiles, int masterSeed, string saveDirectory, bool useWaveFunctionCollapse = true, TerrainRuleConfiguration? terrainRuleConfiguration = null, HeightMapConfiguration? heightMapConfiguration = null, WfcWeightConfiguration? weightConfig = null, HeuristicsConfiguration? heuristicsConfig = null, ILogger? logger = null, int? wfcTimeBudgetMs = null)
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
    _heightMapConfiguration = heightMapConfiguration ?? new HeightMapConfiguration();
    _heightProvider = new HeightMapGenerator(masterSeed, _heightMapConfiguration);
    _wfcWeightConfig = weightConfig ?? new WfcWeightConfiguration();
    _heuristicsConfig = heuristicsConfig ?? new HeuristicsConfiguration();
    _tileTypeRegistry = TileTypeRegistry.CreateDefault(tileset.Count, _terrainRuleConfig);
    _logger = logger;
    _wfcTimeBudgetMs = wfcTimeBudgetMs ?? 50;

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
    var chunkSeed = _masterSeed + chunkCoords.X * 73856093 + chunkCoords.Y * 19349663;
    var random = new Random(chunkSeed);

    if (_useWaveFunctionCollapse)
    {
      // Use Wave Function Collapse for coherent terrain
      var chunkOrigin = new Point(chunkCoords.X * Chunk.ChunkSize, chunkCoords.Y * Chunk.ChunkSize);
      var randomProvider = new RandomAdapter(random);
      var wfc = new WfcProvider(Chunk.ChunkSize, Chunk.ChunkSize, _tileTypeRegistry, randomProvider, _terrainRuleConfig, _heightProvider, chunkOrigin, _wfcWeightConfig, _heuristicsConfig);
      if (_logger != null) GameLoggerMessages.MapGenerateBegin(_logger, Chunk.ChunkSize, Chunk.ChunkSize);

      // Enable backtracking to improve robustness on contradictions
      var wfcSuccess = wfc.Generate(enableBacktracking: true, maxIterations: 10000, maxBacktrackSteps: 2048, maxDepth: 128, timeBudget: TimeSpan.FromMilliseconds(_wfcTimeBudgetMs));
      if (wfcSuccess)
      {
        var output = wfc.GetOutput();
        for (var localY = 0; localY < Chunk.ChunkSize; localY++)
        {
          for (var localX = 0; localX < Chunk.ChunkSize; localX++)
          {
            chunk[localX, localY] = output[localX][localY];
          }
        }
        if (_logger != null) GameLoggerMessages.MapGenerateEnd(_logger, true);
      }
      else
      {
        // WFC failed (contradiction), fall back to random
        GenerateRandomChunk(chunk, random);
        if (_logger != null) GameLoggerMessages.MapGenerateEnd(_logger, false);
      }
    }
    else
    {
      // Use simple random generation
      GenerateRandomChunk(chunk, random);
      if (_logger != null) GameLoggerMessages.MapGenerateEnd(_logger, true);
    }

    chunk.IsDirty = true; // Mark as dirty so it gets saved
    return chunk;
  }

  /// <summary>
  /// Fill chunk with random tiles (fallback method)
  /// </summary>
  private void GenerateRandomChunk(Chunk chunk, Random random)
  {
    var baseWorld = chunk.WorldTilePosition;
    for (var localY = 0; localY < Chunk.ChunkSize; localY++)
    {
      for (var localX = 0; localX < Chunk.ChunkSize; localX++)
      {
        var worldX = baseWorld.X + localX;
        var worldY = baseWorld.Y + localY;
        var sample = _heightProvider.GetSample(worldX, worldY);
        chunk[localX, localY] = PickTileByHeight(sample, random);
      }
    }
  }

  private int PickTileByHeight(HeightSample sample, Random random)
  {
    var config = _terrainRuleConfig;

    if (sample.Altitude <= config.OceanHeightMax)
    {
      return TerrainTileIds.Ocean;
    }

    if (sample.Altitude >= config.MountainHeightMin && sample.MountainNoise >= config.MountainNoiseThreshold)
    {
      return TerrainTileIds.Mountain;
    }

    if (sample.Altitude >= config.SnowHeightMin)
    {
      return TerrainTileIds.Snow;
    }

    if (sample.Altitude >= config.BeachHeightMin && sample.Altitude <= config.BeachHeightMax)
    {
      if (sample.DetailNoise > random.NextSingle())
      {
        return TerrainTileIds.Beach;
      }
    }

    if (sample.Altitude >= config.ForestHeightMin && sample.Altitude <= config.ForestHeightMax)
    {
      return sample.DetailNoise > 0.5f ? TerrainTileIds.Forest : TerrainTileIds.Plains;
    }

    if (sample.Altitude >= config.PlainsHeightMin)
    {
      return TerrainTileIds.Plains;
    }

    return TerrainTileIds.Plains;
  }

  /// <summary>
  /// Loads a chunk from disk
  /// </summary>
  private Chunk? LoadChunk(Point chunkCoords)
  {
    TerrainPerformanceEventSource.Log.ChunkLoadBegin(chunkCoords.X, chunkCoords.Y);
    var loaded = false;

    try
    {
      var filePath = GetChunkFilePath(chunkCoords);

      if (!File.Exists(filePath))
      {
        return null;
      }

      using var fileStream = File.OpenRead(filePath);
      using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
      using var reader = new BinaryReader(gzipStream);

      var magic = reader.ReadBytes(4);
      if (magic[0] != 'C' || magic[1] != 'H' || magic[2] != 'N' || magic[3] != 'K')
      {
        return null;
      }

      var version = reader.ReadInt32();
      if (version != 1)
      {
        return null;
      }

      var chunkX = reader.ReadInt32();
      var chunkY = reader.ReadInt32();

      if (chunkX != chunkCoords.X || chunkY != chunkCoords.Y)
      {
        return null;
      }

      var chunk = new Chunk(chunkCoords);
      for (var y = 0; y < Chunk.ChunkSize; y++)
      {
        for (var x = 0; x < Chunk.ChunkSize; x++)
        {
          chunk[x, y] = reader.ReadInt32();
        }
      }

      chunk.IsDirty = false;
      loaded = true;
      return chunk;
    }
    catch (Exception)
    {
      return null;
    }
    finally
    {
      TerrainPerformanceEventSource.Log.ChunkLoadEnd(chunkCoords.X, chunkCoords.Y, loaded);
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

    var chunkPos = chunk.ChunkPosition;
    TerrainPerformanceEventSource.Log.ChunkSaveBegin(chunkPos.X, chunkPos.Y);
    var success = false;

    try
    {
      var filePath = GetChunkFilePath(chunkPos);

      using var fileStream = File.Create(filePath);
      using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
      using var writer = new BinaryWriter(gzipStream);

      writer.Write(new[] { (byte)'C', (byte)'H', (byte)'N', (byte)'K' });
      writer.Write(1); // Version
      writer.Write(chunkPos.X);
      writer.Write(chunkPos.Y);

      for (var y = 0; y < Chunk.ChunkSize; y++)
      {
        for (var x = 0; x < Chunk.ChunkSize; x++)
        {
          writer.Write(chunk[x, y]);
        }
      }

      chunk.IsDirty = false;
      success = true;
      TerrainPerformanceEventSource.Log.ChunkSaved();
    }
    catch (Exception)
    {
      success = false;
    }
    finally
    {
      TerrainPerformanceEventSource.Log.ChunkSaveEnd(chunkPos.X, chunkPos.Y, success);
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

    TerrainPerformanceEventSource.Log.UpdateActiveChunksBegin(minChunk.X, minChunk.Y, maxChunk.X, maxChunk.Y);

    // Load visible chunks
    for (var cy = minChunk.Y; cy <= maxChunk.Y; cy++)
    {
      for (var cx = minChunk.X; cx <= maxChunk.X; cx++)
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

    TerrainPerformanceEventSource.Log.ReportActiveChunkCount(_activeChunks.Count);
    TerrainPerformanceEventSource.Log.UpdateActiveChunksEnd(minChunk.X, minChunk.Y, maxChunk.X, maxChunk.Y, _activeChunks.Count);
  }

  /// <summary>
  /// Gets a tile ID at the specified world tile coordinates
  /// </summary>
  public int GetTile(int tileX, int tileY)
  {
    Point chunkCoords = TileToChunkCoordinates(tileX, tileY);
    Chunk chunk = GetOrCreateChunk(chunkCoords);

    var localX = tileX - chunk.WorldTilePosition.X;
    var localY = tileY - chunk.WorldTilePosition.Y;

    return chunk[localX, localY];
  }

  /// <summary>
  /// Sets a tile ID at the specified world tile coordinates
  /// </summary>
  public void SetTile(int tileX, int tileY, int tileId)
  {
    Point chunkCoords = TileToChunkCoordinates(tileX, tileY);
    Chunk chunk = GetOrCreateChunk(chunkCoords);

    var localX = tileX - chunk.WorldTilePosition.X;
    var localY = tileY - chunk.WorldTilePosition.Y;

    chunk[localX, localY] = tileId;
  }

  /// <summary>
  /// Draws visible tiles within the viewport
  /// </summary>
  public void Draw(SpriteBatch spriteBatch, Rectangle viewportWorldBounds)
  {
    ArgumentNullException.ThrowIfNull(spriteBatch);

    // Calculate visible tile range
    var minTileX = Math.Max(0, viewportWorldBounds.Left / _tileSize);
    var minTileY = Math.Max(0, viewportWorldBounds.Top / _tileSize);
    var maxTileX = Math.Min(_mapSizeInTiles - 1, viewportWorldBounds.Right / _tileSize);
    var maxTileY = Math.Min(_mapSizeInTiles - 1, viewportWorldBounds.Bottom / _tileSize);

    // Draw only visible tiles
    for (var tileY = minTileY; tileY <= maxTileY; tileY++)
    {
      for (var tileX = minTileX; tileX <= maxTileX; tileX++)
      {
        var tileId = GetTile(tileX, tileY);
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

  public IReadOnlyCollection<ActiveChunkInfo> GetActiveChunkInfos()
  {
    return _activeChunks.Values
        .Select(chunk => new ActiveChunkInfo(chunk.ChunkPosition, chunk.WorldTilePosition, chunk.IsDirty))
        .ToList();
  }

  public readonly record struct ActiveChunkInfo(Point ChunkPosition, Point WorldTilePosition, bool IsDirty);

  public int WfcTimeBudgetMs
  {
    get => _wfcTimeBudgetMs;
    set => _wfcTimeBudgetMs = Math.Max(1, value);
  }

  /// <summary>
  /// Regenerates all chunks currently within the expanded viewport region and optionally overwrites saves.
  /// Use after changing heuristics or terrain rules to see effects immediately.
  /// </summary>
  public void RegenerateChunksInView(Rectangle viewportWorldBounds, bool overwriteSaves = true)
  {
    Point minChunk = TileToChunkCoordinates(
        viewportWorldBounds.Left / _tileSize,
        viewportWorldBounds.Top / _tileSize
    );

    Point maxChunk = TileToChunkCoordinates(
        viewportWorldBounds.Right / _tileSize,
        viewportWorldBounds.Bottom / _tileSize
    );

    minChunk.X = Math.Max(0, minChunk.X - 1);
    minChunk.Y = Math.Max(0, minChunk.Y - 1);
    maxChunk.X = Math.Min(_mapSizeInChunks - 1, maxChunk.X + 1);
    maxChunk.Y = Math.Min(_mapSizeInChunks - 1, maxChunk.Y + 1);

    for (var cy = minChunk.Y; cy <= maxChunk.Y; cy++)
    {
      for (var cx = minChunk.X; cx <= maxChunk.X; cx++)
      {
        var pos = new Point(cx, cy);
        var regenerated = GenerateChunk(pos);
        _activeChunks[pos] = regenerated;
        if (overwriteSaves)
        {
          SaveChunk(regenerated);
        }
      }
    }
  }

  /// <summary>
  /// Deletes all saved chunks on disk. Next loads will regenerate using current settings.
  /// </summary>
  public void ClearAllSavedChunks()
  {
    if (!Directory.Exists(_saveDirectory))
    {
      return;
    }
    foreach (var file in Directory.EnumerateFiles(_saveDirectory, "map_*_*.dat"))
    {
      try { File.Delete(file); } catch { /* ignore */ }
    }
  }
}