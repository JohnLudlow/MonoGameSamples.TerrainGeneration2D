using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class Camera2DTests
{
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

  [Fact]
  public void Move_UpdatesPosition()
  {
    // Arrange
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    var initialPosition = camera.Position;

    // Act
    camera.Move(new Vector2(100, 50));

    // Assert
    Assert.Equal(initialPosition.X + 100, camera.Position.X);
    Assert.Equal(initialPosition.Y + 50, camera.Position.Y);
  }

  [Fact]
  public void ViewportWorldBounds_ReflectsZoom()
  {
    // Arrange
    var viewport = new Viewport(0, 0, 800, 600);
    var camera = new Camera2D(viewport);
    camera.Position = new Vector2(0, 0);
    camera.Zoom = 1.0f;

    // Act
    var bounds1 = camera.ViewportWorldBounds;

    camera.Zoom = 2.0f;
    var bounds2 = camera.ViewportWorldBounds;

    // Assert - Higher zoom means smaller world bounds
    Assert.True(bounds2.Width < bounds1.Width);
    Assert.True(bounds2.Height < bounds1.Height);
  }
}