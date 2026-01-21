using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests.Core.Graphics;

public class Camera2DTests
{
  [Theory]
  [InlineData(5.0f, 4.0f)]    // Above max
  [InlineData(0.1f, 0.25f)]   // Below min
  [InlineData(2.0f, 2.0f)]    // Within range
  public void Zoom_ClampsToValidRange(float inputZoom, float expectedZoom)
  {
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    camera.Zoom = inputZoom;
    Assert.Equal(expectedZoom, camera.Zoom);
  }

  [Fact]
  public void ScreenToWorld_WorldToScreen_RoundTrip()
  {
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    camera.Position = new Vector2(1000, 1000);
    camera.Zoom = 2.0f;
    var originalWorld = new Vector2(1234, 5678);
    var screen = camera.WorldToScreen(originalWorld);
    var backToWorld = camera.ScreenToWorld(screen);
    Assert.Equal(originalWorld.X, backToWorld.X, precision: 1);
    Assert.Equal(originalWorld.Y, backToWorld.Y, precision: 1);
  }

  [Fact]
  public void GetTransformMatrix_ChangesWithZoomAndPosition()
  {
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    var matrix1 = camera.GetTransformMatrix();
    camera.Zoom = 2.0f;
    var matrix2 = camera.GetTransformMatrix();
    camera.Position = new Vector2(100, 100);
    var matrix3 = camera.GetTransformMatrix();
    Assert.NotEqual(matrix1, matrix2);
    Assert.NotEqual(matrix2, matrix3);
  }

  [Fact]
  public void Move_UpdatesPosition()
  {
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    var initialPosition = camera.Position;
    camera.Move(new Vector2(100, 50));
    Assert.Equal(initialPosition.X + 100, camera.Position.X);
    Assert.Equal(initialPosition.Y + 50, camera.Position.Y);
  }
}
