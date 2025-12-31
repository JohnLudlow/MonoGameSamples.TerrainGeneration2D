using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

BenchmarkRunner.Run<ChunkGenerationBenchmark>();

[MemoryDiagnoser]
public class ChunkGenerationBenchmark
{
    private const int MapSizeInTiles = 1024;
    private string _saveRoot = string.Empty;
    private Tileset _tileset = null!;

    [GlobalSetup]
    public void Setup()
    {
        _saveRoot = Path.Combine(Path.GetTempPath(), "TerrainGeneration2DBench");
        if (!Directory.Exists(_saveRoot))
        {
            Directory.CreateDirectory(_saveRoot);
        }

        _tileset = TilesetFactory.CreateMockTileset(tileCount: 16, tileSize: 16);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_saveRoot))
        {
            Directory.Delete(_saveRoot, recursive: true);
        }
    }

    [Benchmark]
    public void GenerateChunkedTerrain()
    {
        var outputDir = Path.Combine(_saveRoot, Guid.NewGuid().ToString("N"));
        var map = new ChunkedTilemap(_tileset, MapSizeInTiles, masterSeed: 42, outputDir);
        map.UpdateActiveChunks(new Rectangle(0, 0, MapSizeInTiles * _tileset.TileWidth, MapSizeInTiles * _tileset.TileHeight));
        map.SaveAll();
    }
}

internal static class TilesetFactory
{
    public static Tileset CreateMockTileset(int tileCount, int tileSize)
    {
        var tileset = (Tileset)FormatterServices.GetUninitializedObject(typeof(Tileset));
        SetBackingField(tileset, nameof(Tileset.TileWidth), tileSize);
        SetBackingField(tileset, nameof(Tileset.TileHeight), tileSize);
        SetBackingField(tileset, nameof(Tileset.Columns), tileCount);
        SetBackingField(tileset, nameof(Tileset.Rows), 1);
        SetBackingField(tileset, nameof(Tileset.Count), tileCount);
        SetTilesField(tileset, Array.Empty<TextureRegion>());
        return tileset;
    }

    private static void SetBackingField<T>(Tileset target, string propertyName, T value)
    {
        var field = typeof(Tileset).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException($"Backing field for '{propertyName}' was not found.");
        }

        field.SetValue(target, value);
    }

    private static void SetTilesField(Tileset target, TextureRegion[] values)
    {
        var field = typeof(Tileset).GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException("Tiles field was not found.");
        }

        field.SetValue(target, values);
    }
}
