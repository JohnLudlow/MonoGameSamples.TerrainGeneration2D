using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;
using Microsoft.Xna.Framework;
using Xunit;
using System;
using System.IO;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon.Core.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests.Core.Graphics;

public sealed class ChunkedTilemapTests : IDisposable
{
  private readonly string _testSaveDir;

  public ChunkedTilemapTests()
  {
    _testSaveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testSaveDir);
  }

  public void Dispose()
  {
    if (Directory.Exists(_testSaveDir))
    {
      Directory.Delete(_testSaveDir, recursive: true);
    }
  }

  [Theory]
  [InlineData(0, 0, 0, 0)]      // First tile of first chunk
  [InlineData(63, 63, 0, 0)]    // Last tile of first chunk
  [InlineData(64, 64, 1, 1)]    // First tile of second chunk
  [InlineData(-1, -1, -1, -1)]  // Negative coordinates
  [InlineData(128, 256, 2, 4)]  // Arbitrary positive coords
  public void TileToChunkCoordinates_ConvertsCorrectly(int tileX, int tileY, int expectedChunkX, int expectedChunkY)
  {
    var result = ChunkedTilemap.TileToChunkCoordinates(tileX, tileY);
    Assert.Equal(expectedChunkX, result.X);
    Assert.Equal(expectedChunkY, result.Y);
  }

  [Fact]
  public void GenerateChunk_SameSeedProducesSameTiles()
  {
    var tileset = GraphicsTestHelpers.CreateMockTileset(16);
    var map1 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir, useWaveFunctionCollapse: false);
    var map2 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir + "2", useWaveFunctionCollapse: false);
    var tile1 = map1.GetTile(100, 100);
    var tile2 = map2.GetTile(100, 100);
    Assert.Equal(tile1, tile2);
  }

  [Fact]
  public void GenerateChunk_DifferentSeedsProduceDifferentTiles()
  {
    var tileset = GraphicsTestHelpers.CreateMockTileset(16);
    var terrainConfig = new TileTypeRuleConfiguration();
    terrainConfig.Rules.AddRange([
      new() { Id = TerrainTileIds.Ocean, ElevationMax = 0.34f },
      new() { Id = TerrainTileIds.Beach, ElevationMin = 0.33f, ElevationMax = 0.48f, MinGroupSizeX = 12, MaxGroupSizeX = 180 },
      new() { Id = TerrainTileIds.Plains, ElevationMin = 0.35f, ElevationMax = 0.78f },
      new() { Id = TerrainTileIds.Forest, ElevationMin = 0.42f, ElevationMax = 0.88f },
      new() { Id = TerrainTileIds.Snow, ElevationMin = 0.82f },
      new() { Id = TerrainTileIds.Mountain, ElevationMin = 0.76f, NoiseThreshold = 0.55f, MinGroupSizeX = 3, MaxGroupSizeX = 12, MinGroupSizeY = 8, MaxGroupSizeY = 48 }
    ]);
    var map1 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir, useWaveFunctionCollapse: false, terrainRuleConfiguration: terrainConfig);
    var map2 = new ChunkedTilemap(tileset, 2048, 54321, _testSaveDir + "2", useWaveFunctionCollapse: false, terrainRuleConfiguration: terrainConfig);
    // Try several coordinates to increase the chance of difference
    var coords = new[] { (100, 100), (500, 500), (123, 456), (789, 321) };
    bool foundDifference = false;
    foreach (var (x, y) in coords)
    {
      var tile1 = map1.GetTile(x, y);
      var tile2 = map2.GetTile(x, y);
      // Print for debug
      System.Diagnostics.Debug.WriteLine($"Seed1:{tile1} Seed2:{tile2} at ({x},{y})");
      if (tile1 != tile2)
      {
        foundDifference = true;
        break;
      }
    }
    Assert.True(foundDifference, "Tiles should differ for different seeds at at least one coordinate");
  }
}
