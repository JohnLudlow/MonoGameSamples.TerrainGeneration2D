using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UnitTests.Core.Mapping;

public class ChunkTests
{
  [Fact]
  public void Chunk_AccessWithinBounds_Succeeds()
  {
    var chunk = new Chunk(new Point(0, 0));
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
    var chunk = new Chunk(new Point(0, 0));
    Assert.Throws<IndexOutOfRangeException>(() => chunk[x, y] = 1);
  }

  [Fact]
  public void Chunk_SetTile_MarksDirty()
  {
    var chunk = new Chunk(new Point(0, 0));
    Assert.False(chunk.IsDirty);
    chunk[10, 10] = 5;
    Assert.True(chunk.IsDirty);
  }

  [Fact]
  public void Chunk_NewChunk_IsNotDirty()
  {
    var chunk = new Chunk(new Point(5, 5));
    Assert.False(chunk.IsDirty);
  }

  [Fact]
  public void Chunk_WorldTilePosition_CalculatesCorrectly()
  {
    var chunk = new Chunk(new Point(2, 3));
    var worldPos = chunk.WorldTilePosition;
    Assert.Equal(128, worldPos.X);
    Assert.Equal(192, worldPos.Y);
  }
}
