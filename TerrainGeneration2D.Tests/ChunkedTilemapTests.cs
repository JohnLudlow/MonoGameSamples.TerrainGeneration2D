using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class ChunkedTilemapTests : IDisposable
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
        // Arrange & Act
        var result = ChunkedTilemap.TileToChunkCoordinates(tileX, tileY);

        // Assert
        Assert.Equal(expectedChunkX, result.X);
        Assert.Equal(expectedChunkY, result.Y);
    }

    [Fact]
    public void GenerateChunk_SameSeedProducesSameTiles()
    {
        // Arrange
        var tileset = TestHelpers.CreateMockTileset(16);
        var map1 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir, useWaveFunctionCollapse: false);
        var map2 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir + "2", useWaveFunctionCollapse: false);

        // Act - Access same chunk in both maps
        int tile1 = map1.GetTile(100, 100);
        int tile2 = map2.GetTile(100, 100);

        // Assert
        Assert.Equal(tile1, tile2);
    }

    [Fact]
    public void GenerateChunk_DifferentSeedsProduceDifferentTiles()
    {
        // Arrange
        var tileset = TestHelpers.CreateMockTileset(16);
        var map1 = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir, useWaveFunctionCollapse: false);
        var map2 = new ChunkedTilemap(tileset, 2048, 54321, _testSaveDir + "2", useWaveFunctionCollapse: false);

        // Act - Sample multiple tiles to ensure different generation
        bool foundDifference = false;
        for (int i = 0; i < 100; i++)
        {
            int tile1 = map1.GetTile(i, i);
            int tile2 = map2.GetTile(i, i);
            if (tile1 != tile2)
            {
                foundDifference = true;
                break;
            }
        }

        // Assert
        Assert.True(foundDifference, "Different seeds should produce different tile patterns");
    }

    [Fact]
    public void SaveAndLoad_PreservesChunkData()
    {
        // Arrange
        var tileset = TestHelpers.CreateMockTileset(16);
        var map = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir);

        // Act - Generate and save
        int originalValue = map.GetTile(100, 100);
        map.SaveAll();

        // Create new map instance with different seed and load
        var newMap = new ChunkedTilemap(tileset, 2048, 99999, _testSaveDir);
        int loadedValue = newMap.GetTile(100, 100);

        // Assert
        Assert.Equal(originalValue, loadedValue);
    }

    [Fact]
    public void UpdateActiveChunks_LoadsVisibleChunks()
    {
        // Arrange
        var tileset = TestHelpers.CreateMockTileset(16);
        var map = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir);
        var viewport = new Rectangle(0, 0, 640, 480); // 32x24 tiles at 20px each

        // Act
        map.UpdateActiveChunks(viewport);

        // Assert - Use reflection to check _activeChunks dictionary size
        var activeChunks = TestHelpers.GetPrivateField<Dictionary<Point, Chunk>>(map, "_activeChunks");

        // Should load approximately 3x3 = 9 chunks (viewport + buffer)
        Assert.NotNull(activeChunks);
        Assert.InRange(activeChunks!.Count, 4, 12);
    }

    [Fact]
    public void GetTile_ReturnsValidTileId()
    {
        // Arrange
        var tileset = TestHelpers.CreateMockTileset(16);
        var map = new ChunkedTilemap(tileset, 2048, 12345, _testSaveDir);

        // Act
        int tileId = map.GetTile(0, 0);

        // Assert - Tile ID should be within tileset range
        Assert.InRange(tileId, 0, 15);
    }
}
