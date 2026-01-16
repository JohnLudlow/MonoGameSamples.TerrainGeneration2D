using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

/// <summary>
/// 2D camera for handling viewport transformation, zoom, and panning
/// </summary>
public class Camera2D
{
  private float _zoom;
  private Vector2 _position;
  private readonly Viewport _viewport;

  public const float MinZoom = 0.25f;
  public const float MaxZoom = 4.0f;
  public const float ZoomIncrement = 0.1f;

  /// <summary>
  /// Gets or sets the camera position in world coordinates
  /// </summary>
  public Vector2 Position
  {
    get => _position;
    set => _position = value;
  }

  /// <summary>
  /// Gets or sets the camera zoom level (0.25 to 4.0)
  /// </summary>
  public float Zoom
  {
    get => _zoom;
    set => _zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
  }

  /// <summary>
  /// Gets the viewport bounds in world coordinates
  /// </summary>
  public Rectangle ViewportWorldBounds
  {
    get
    {
      var inverseTransform = Matrix.Invert(GetTransformMatrix());
      var topLeft = Vector2.Transform(Vector2.Zero, inverseTransform);
      var bottomRight = Vector2.Transform(new Vector2(_viewport.Width, _viewport.Height), inverseTransform);

      return new Rectangle(
          (int)topLeft.X,
          (int)topLeft.Y,
          (int)(bottomRight.X - topLeft.X),
          (int)(bottomRight.Y - topLeft.Y)
      );
    }
  }

  public Camera2D(Viewport viewport)
  {
    _viewport = viewport;
    _zoom = 1.0f;
    _position = Vector2.Zero;
  }

  /// <summary>
  /// Gets the transformation matrix for SpriteBatch
  /// </summary>
  public Matrix GetTransformMatrix()
  {
    return Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0)) *
           Matrix.CreateScale(_zoom) *
           Matrix.CreateTranslation(new Vector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0));
  }

  /// <summary>
  /// Converts screen coordinates to world coordinates
  /// </summary>
  public Vector2 ScreenToWorld(Vector2 screenPosition)
  {
    var inverseTransform = Matrix.Invert(GetTransformMatrix());
    return Vector2.Transform(screenPosition, inverseTransform);
  }

  /// <summary>
  /// Converts world coordinates to screen coordinates
  /// </summary>
  public Vector2 WorldToScreen(Vector2 worldPosition)
  {
    return Vector2.Transform(worldPosition, GetTransformMatrix());
  }

  /// <summary>
  /// Moves the camera by the specified delta
  /// </summary>
  public void Move(Vector2 delta)
  {
    _position += delta;
  }

  /// <summary>
  /// Adjusts zoom level by the specified amount
  /// </summary>
  public void AdjustZoom(float delta)
  {
    Zoom += delta;
  }
}