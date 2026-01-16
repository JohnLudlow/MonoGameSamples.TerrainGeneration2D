using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;

public enum MouseButton
{
  Left,
  Middle,
  Right,
  XButton1,
  XButton2
}

public class MouseInfo
{
  public MouseState PreviousState { get; set; }
  public MouseState CurrentState { get; set; }

  public Point Position
  {
    get => CurrentState.Position;
    set => Mouse.SetPosition(value.X, value.Y);
  }

  public int X
  {
    get => CurrentState.X;
    set => Mouse.SetPosition(value, CurrentState.Y);
  }

  public int Y
  {
    get => CurrentState.Y;
    set => Mouse.SetPosition(CurrentState.X, value);
  }

  public Point PositionDelta => CurrentState.Position - PreviousState.Position;
  public int XDelta => CurrentState.X - PreviousState.X;
  public int YDelta => CurrentState.Y - PreviousState.Y;
  public bool WasMoved => PositionDelta != Point.Zero;
  public int ScrollWheelValue => CurrentState.ScrollWheelValue;
  public int ScrollWheelDelta => CurrentState.ScrollWheelValue - PreviousState.ScrollWheelValue;

  public MouseInfo()
  {
    PreviousState = new MouseState();
    CurrentState = Mouse.GetState();
  }

  /// <summary>
  /// Returns a value that indicates if the specified mouse button is currently down.
  /// </summary>
  /// <param name="button">The mouse button to check.</param>
  /// <returns>true if the specified mouse button is currently down; otherwise, false.</returns>
  public bool IsButtonDown(MouseButton button)
  {
    return button switch
    {
      MouseButton.Left => CurrentState.LeftButton == ButtonState.Pressed,
      MouseButton.Middle => CurrentState.MiddleButton == ButtonState.Pressed,
      MouseButton.Right => CurrentState.RightButton == ButtonState.Pressed,
      MouseButton.XButton1 => CurrentState.XButton1 == ButtonState.Pressed,
      MouseButton.XButton2 => CurrentState.XButton2 == ButtonState.Pressed,
      _ => false,
    };
  }

  public bool IsButtonUp(MouseButton button)
  {
    return button switch
    {
      MouseButton.Left => CurrentState.LeftButton == ButtonState.Released,
      MouseButton.Middle => CurrentState.MiddleButton == ButtonState.Released,
      MouseButton.Right => CurrentState.RightButton == ButtonState.Released,
      MouseButton.XButton1 => CurrentState.XButton1 == ButtonState.Released,
      MouseButton.XButton2 => CurrentState.XButton2 == ButtonState.Released,
      _ => false,
    };
  }

  public bool WasButtonJustPressed(MouseButton button)
  {
    return button switch
    {
      MouseButton.Left => CurrentState.LeftButton == ButtonState.Pressed && PreviousState.LeftButton == ButtonState.Released,
      MouseButton.Middle => CurrentState.MiddleButton == ButtonState.Pressed && PreviousState.MiddleButton == ButtonState.Released,
      MouseButton.Right => CurrentState.RightButton == ButtonState.Pressed && PreviousState.RightButton == ButtonState.Released,
      MouseButton.XButton1 => CurrentState.XButton1 == ButtonState.Pressed && PreviousState.XButton1 == ButtonState.Released,
      MouseButton.XButton2 => CurrentState.XButton2 == ButtonState.Pressed && PreviousState.XButton2 == ButtonState.Released,
      _ => false,
    };
  }

  public bool WasButtonJustReleased(MouseButton button)
  {
    return button switch
    {
      MouseButton.Left => CurrentState.LeftButton == ButtonState.Released && PreviousState.LeftButton == ButtonState.Pressed,
      MouseButton.Middle => CurrentState.MiddleButton == ButtonState.Released && PreviousState.MiddleButton == ButtonState.Pressed,
      MouseButton.Right => CurrentState.RightButton == ButtonState.Released && PreviousState.RightButton == ButtonState.Pressed,
      MouseButton.XButton1 => CurrentState.XButton1 == ButtonState.Released && PreviousState.XButton1 == ButtonState.Pressed,
      MouseButton.XButton2 => CurrentState.XButton2 == ButtonState.Released && PreviousState.XButton2 == ButtonState.Pressed,
      _ => false,
    };
  }

  public void SetPosition(int x, int y)
  {
    Mouse.SetPosition(x, y);
    CurrentState = new MouseState(
      x,
      y,
      CurrentState.ScrollWheelValue,
      CurrentState.LeftButton,
      CurrentState.MiddleButton,
      CurrentState.RightButton,
      CurrentState.XButton1,
      CurrentState.XButton2
    );
  }

  public void Update()
  {
    PreviousState = CurrentState;
    CurrentState = Mouse.GetState();
  }
}