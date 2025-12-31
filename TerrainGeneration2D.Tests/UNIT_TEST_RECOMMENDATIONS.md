# Unit Test Recommendations for TerrainGeneration2D

## Overview
Focus on testing the map generation logic in the Core library, particularly chunk management, coordinate conversions, and deterministic generation.

## Recommended Test Structure

### 1. ChunkedTilemap Tests (`ChunkedTilemapTests.cs`)

#### Test: Coordinate Conversion
```csharp
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
```

#### Test: Deterministic Generation
```csharp
[Fact]
public void GenerateChunk_SameSeedProducesSameTiles()
{
    // Arrange
    var tileset = CreateMockTileset(16); // 16 tile types
    var map1 = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    var map2 = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    
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
    var tileset = CreateMockTileset(16);
    var map1 = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    var map2 = new ChunkedTilemap(tileset, 2048, seed: 54321, saveDir);
    
    // Act - Access same tile in both maps
    int tile1 = map1.GetTile(100, 100);
    int tile2 = map2.GetTile(100, 100);
    
    // Assert
    Assert.NotEqual(tile1, tile2);
}
```

#### Test: Chunk Persistence
```csharp
[Fact]
public void SaveAndLoad_PreservesChunkData()
{
    // Arrange
    var tileset = CreateMockTileset(16);
    var saveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var map = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    
    // Act - Generate and modify a chunk
    map.SetTile(100, 100, 5);
    int originalValue = map.GetTile(100, 100);
    map.SaveAll();
    
    // Create new map instance and load
    var newMap = new ChunkedTilemap(tileset, 2048, seed: 99999, saveDir); // Different seed
    int loadedValue = newMap.GetTile(100, 100);
    
    // Assert
    Assert.Equal(originalValue, loadedValue);
    Assert.Equal(5, loadedValue);
    
    // Cleanup
    Directory.Delete(saveDir, recursive: true);
}
```

#### Test: Chunk Loading Strategy
```csharp
[Fact]
public void UpdateActiveChunks_LoadsOnlyVisibleChunks()
{
    // Arrange
    var tileset = CreateMockTileset(16);
    var map = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    var viewport = new Rectangle(0, 0, 640, 480); // 32x24 tiles at 20px each
    
    // Act
    map.UpdateActiveChunks(viewport);
    
    // Assert - Use reflection to check _activeChunks dictionary size
    var activeChunks = GetPrivateField<Dictionary<Point, Chunk>>(map, "_activeChunks");
    
    // Should load approximately 3x3 = 9 chunks (viewport + buffer)
    Assert.InRange(activeChunks.Count, 4, 12);
}

[Fact]
public void UpdateActiveChunks_UnloadsDistantChunks()
{
    // Arrange
    var tileset = CreateMockTileset(16);
    var map = new ChunkedTilemap(tileset, 2048, seed: 12345, saveDir);
    
    // Act - Load chunks at origin
    var viewport1 = new Rectangle(0, 0, 640, 480);
    map.UpdateActiveChunks(viewport1);
    int initialCount = GetPrivateField<Dictionary<Point, Chunk>>(map, "_activeChunks").Count;
    
    // Move viewport far away
    var viewport2 = new Rectangle(10000, 10000, 640, 480);
    map.UpdateActiveChunks(viewport2);
    var activeChunks = GetPrivateField<Dictionary<Point, Chunk>>(map, "_activeChunks");
    
    // Assert - Original chunks should be unloaded
    Assert.DoesNotContain(activeChunks.Keys, key => key.X == 0 && key.Y == 0);
}
```

### 2. Chunk Tests (`ChunkTests.cs`)

#### Test: Chunk Boundaries
```csharp
[Fact]
public void Chunk_AccessWithinBounds_Succeeds()
{
    // Arrange
    var chunk = new Chunk(new Point(0, 0));
    
    // Act & Assert - Should not throw
    chunk[0, 0] = 1;
    chunk[63, 63] = 2;
    
    Assert.Equal(1, chunk[0, 0]);
    Assert.Equal(2, chunk[63, 63]);
}

[Theory]
[InlineData(-1, 0)]
[InlineData(0, -1)]
[InlineData(64, 0)]
[InlineData(0, 64)]
public void Chunk_AccessOutOfBounds_ThrowsException(int x, int y)
{
    // Arrange
    var chunk = new Chunk(new Point(0, 0));
    
    // Act & Assert
    Assert.Throws<IndexOutOfRangeException>(() => chunk[x, y] = 1);
}
```

#### Test: Dirty Flag
```csharp
[Fact]
public void Chunk_SetTile_MarksDirty()
{
    // Arrange
    var chunk = new Chunk(new Point(0, 0));
    Assert.False(chunk.IsDirty);
    
    // Act
    chunk[10, 10] = 5;
    
    // Assert
    Assert.True(chunk.IsDirty);
}

[Fact]
public void Chunk_NewChunk_IsNotDirty()
{
    // Arrange & Act
    var chunk = new Chunk(new Point(5, 5));
    
    // Assert
    Assert.False(chunk.IsDirty);
}
```

### 3. Camera2D Tests (`Camera2DTests.cs`)

#### Test: Zoom Clamping
```csharp
[Theory]
[InlineData(5.0f, 4.0f)]    // Above max
[InlineData(0.1f, 0.25f)]   // Below min
[InlineData(2.0f, 2.0f)]    // Within range
public void Zoom_ClampsToValidRange(float inputZoom, float expectedZoom)
{
    // Arrange
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    
    // Act
    camera.Zoom = inputZoom;
    
    // Assert
    Assert.Equal(expectedZoom, camera.Zoom);
}
```

#### Test: Coordinate Conversion
```csharp
[Fact]
public void ScreenToWorld_WorldToScreen_RoundTrip()
{
    // Arrange
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    camera.Position = new Vector2(1000, 1000);
    camera.Zoom = 2.0f;
    
    var originalWorld = new Vector2(1234, 5678);
    
    // Act
    var screen = camera.WorldToScreen(originalWorld);
    var backToWorld = camera.ScreenToWorld(screen);
    
    // Assert
    Assert.Equal(originalWorld.X, backToWorld.X, precision: 1);
    Assert.Equal(originalWorld.Y, backToWorld.Y, precision: 1);
}
```

#### Test: Transform Matrix
```csharp
[Fact]
public void GetTransformMatrix_ChangesWithZoomAndPosition()
{
    // Arrange
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    
    // Act
    var matrix1 = camera.GetTransformMatrix();
    
    camera.Zoom = 2.0f;
    var matrix2 = camera.GetTransformMatrix();
    
    camera.Position = new Vector2(100, 100);
    var matrix3 = camera.GetTransformMatrix();
    
    // Assert
    Assert.NotEqual(matrix1, matrix2);
    Assert.NotEqual(matrix2, matrix3);
}
```

### 4. Chunk Serialization Tests (`ChunkSerializationTests.cs`)

#### Test: Binary Format Validation
```csharp
[Fact]
public void SaveChunk_ProducesValidBinaryFormat()
{
    // Arrange
    var chunk = new Chunk(new Point(5, 10));
    for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
            chunk[x, y] = (x + y) % 16;
    
    chunk.IsDirty = true;
    var saveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(saveDir);
    
    // Use reflection to call private SaveChunk method
    var tileset = CreateMockTileset(16);
    var map = new ChunkedTilemap(tileset, 2048, 12345, saveDir);
    var saveMethod = typeof(ChunkedTilemap).GetMethod("SaveChunk", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    
    // Act
    saveMethod.Invoke(map, new object[] { chunk });
    
    // Assert - Check file exists and has valid header
    var filePath = Path.Combine(saveDir, "map_5_10.dat");
    Assert.True(File.Exists(filePath));
    
    using var fs = File.OpenRead(filePath);
    using var gz = new GZipStream(fs, CompressionMode.Decompress);
    using var reader = new BinaryReader(gz);
    
    var magic = reader.ReadBytes(4);
    Assert.Equal((byte)'C', magic[0]);
    Assert.Equal((byte)'H', magic[1]);
    Assert.Equal((byte)'N', magic[2]);
    Assert.Equal((byte)'K', magic[3]);
    
    var version = reader.ReadInt32();
    Assert.Equal(1, version);
    
    var chunkX = reader.ReadInt32();
    var chunkY = reader.ReadInt32();
    Assert.Equal(5, chunkX);
    Assert.Equal(10, chunkY);
    
    // Cleanup
    Directory.Delete(saveDir, recursive: true);
}
```

### 5. Mapping & Tile Rules Tests (`MappingTests.cs`)

#### Test: Mapping Information Metrics
```csharp
[Fact]
public void GetGroupMetrics_ReturnsCorrectDimensions()
{
    var output = new int[4, 4]
    {
        { 1, 1, 2, 2 },
        { 1, 1, 2, 2 },
        { 3, 3, 3, 4 },
        { 3, 3, 3, 4 }
    };
    var service = new MappingInformationService(output);

    var metrics = service.GetGroupMetrics(new TilePoint(0, 0));

    Assert.Equal(4, metrics.Count);
    Assert.Equal(2, metrics.Width);
    Assert.Equal(2, metrics.Height);
}
```

#### Test: Tile Rule Enforcement
```csharp
[Fact]
public void TileTypes_RejectNeighborViolations()
{
    var config = new TerrainRuleConfiguration
    {
        BeachOceanSizeMin = 1,
        BeachOceanSizeMax = 1,
        BeachPlainsSizeMin = 1,
        BeachPlainsSizeMax = 1
    };
    var registry = TileTypeRegistry.CreateDefault(7, config);
    var mapping = new MappingInformationService(new int[2, 2]);

    var beach = registry.GetTileType(TerrainTileIds.Beach);
    var context = new TileRuleContext(new TilePoint(0, 0), TerrainTileIds.Beach,
        new TilePoint(1, 0), TerrainTileIds.Mountain, Direction.East, mapping);

    Assert.False(beach.EvaluateRules(context));
}
```

#### Test: WFC Filters Candidates
```csharp
[Fact]
public void WaveFunctionCollapse_PropagationFiltersOptions()
{
    var registry = TileTypeRegistry.CreateDefault(5);
    var random = new Random(123);
    var wfc = new WaveFunctionCollapse(8, 8, registry, random);

    // Force a single collapse to seed a constraint
    wfc.Generate();
    var output = wfc.GetOutput();

    // After propagation, every cell should have been collapsed
    Assert.DoesNotContain(-1, output.Cast<int>());
}
```

## Test Project Setup

### Create Test Project
```bash
dotnet new xunit -n TerrainGeneration2D.Tests -f net10.0
cd TerrainGeneration2D.Tests
dotnet add reference ../TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
dotnet add package MonoGame.Framework.DesktopGL
dotnet add package Moq
```

### Add to Solution
```bash
dotnet sln TerrainGeneration2D.slnx add TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

## Mock Helpers

```csharp
// TestHelpers.cs
public static class TestHelpers
{
    public static Tileset CreateMockTileset(int tileCount, int tileSize = 20)
    {
        // Create a 1x1 white texture for testing
        var texture = new Texture2D(graphicsDevice, tileSize * tileCount, tileSize);
        var region = new TextureRegion(texture, 0, 0, tileSize * tileCount, tileSize);
        return new Tileset(region, tileSize, tileSize);
    }
    
    public static T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(obj);
    }
}
```

## Key Testing Principles

1. **Determinism**: Map generation must be reproducible with same seed
2. **Boundary Conditions**: Test chunk edges, negative coordinates, map limits
3. **Persistence**: Ensure chunks save and load correctly
4. **Memory Management**: Verify chunks are loaded/unloaded appropriately
5. **Coordinate Conversion**: Critical for camera/tooltip functionality
6. **Performance**: Consider benchmark tests for large viewport updates

## Integration Test Suggestions

1. **Full Map Navigation**: Simulate scrolling across entire map, verify no crashes
2. **Repeated Save/Load**: Test multiple save/load cycles preserve data
3. **Concurrent Access**: If multi-threading added, test thread safety
4. **Corrupted Save Files**: Test graceful handling of invalid chunk files
