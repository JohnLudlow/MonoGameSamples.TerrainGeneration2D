using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class ChunkTests
{
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

  [Fact]
  public void Chunk_WorldTilePosition_CalculatesCorrectly()
  {
    // Arrange
    var chunk = new Chunk(new Point(2, 3));

    // Act
    var worldPos = chunk.WorldTilePosition;

    // Assert
    Assert.Equal(128, worldPos.X); // 2 * 64
    Assert.Equal(192, worldPos.Y); // 3 * 64
  }
}