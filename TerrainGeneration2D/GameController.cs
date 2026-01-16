using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

/// <summary>
/// Provides a game-specific input abstraction that maps physical inputs
/// to game actions, bridging our input system with game-specific functionality.
/// </summary>
internal static class GameController
{
  private static KeyboardInfo s_keyboard => JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.Input.Keyboard;
  private static GamePadInfo s_gamePad => JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.Input.GamePads.ElementAt((int)PlayerIndex.One);
  private static MouseInfo s_mouse => JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.Input.Mouse;

  /// <summary>
  /// Returns true if the player has triggered the "move up" action.
  /// </summary>
  public static bool MoveUp()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Up) ||
           s_keyboard.WasKeyJustPressed(Keys.W) ||
           s_gamePad.WasButtonJustPressed(Buttons.DPadUp) ||
           s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickUp);
  }

  /// <summary>
  /// Returns true if the player has triggered the "move down" action.
  /// </summary>
  public static bool MoveDown()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Down) ||
           s_keyboard.WasKeyJustPressed(Keys.S) ||
           s_gamePad.WasButtonJustPressed(Buttons.DPadDown) ||
           s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickDown);
  }

  /// <summary>
  /// Returns true if the player has triggered the "move left" action.
  /// </summary>
  public static bool MoveLeft()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Left) ||
           s_keyboard.WasKeyJustPressed(Keys.A) ||
           s_gamePad.WasButtonJustPressed(Buttons.DPadLeft) ||
           s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickLeft);
  }

  /// <summary>
  /// Returns true if the player has triggered the "move right" action.
  /// </summary>
  public static bool MoveRight()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Right) ||
           s_keyboard.WasKeyJustPressed(Keys.D) ||
           s_gamePad.WasButtonJustPressed(Buttons.DPadRight) ||
           s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickRight);
  }

  /// <summary>
  /// Returns true if the player has triggered the "pause" action.
  /// </summary>
  public static bool Pause()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Escape) ||
           s_gamePad.WasButtonJustPressed(Buttons.Start);
  }

  /// <summary>
  /// Returns true if the player has triggered the "action" button,
  /// typically used for menu confirmation.
  /// </summary>
  public static bool Action()
  {
    return s_keyboard.WasKeyJustPressed(Keys.Enter) ||
           s_gamePad.WasButtonJustPressed(Buttons.A);
  }

  /// <summary>
  /// Gets the camera movement direction based on WASD/arrow keys or gamepad input.
  /// Returns a normalized direction vector, or Vector2.Zero if no movement input.
  /// </summary>
  public static Vector2 GetCameraMovement()
  {
    Vector2 direction = Vector2.Zero;

    // Keyboard input
    if (s_keyboard.IsKeyDown(Keys.W) || s_keyboard.IsKeyDown(Keys.Up))
    {
      direction.Y -= 1;
    }
    if (s_keyboard.IsKeyDown(Keys.S) || s_keyboard.IsKeyDown(Keys.Down))
    {
      direction.Y += 1;
    }
    if (s_keyboard.IsKeyDown(Keys.A) || s_keyboard.IsKeyDown(Keys.Left))
    {
      direction.X -= 1;
    }
    if (s_keyboard.IsKeyDown(Keys.D) || s_keyboard.IsKeyDown(Keys.Right))
    {
      direction.X += 1;
    }

    // Gamepad input (left stick)
    var leftStick = s_gamePad.CurrentState.ThumbSticks.Left;
    if (leftStick.LengthSquared() > 0.1f) // deadzone
    {
      direction.X += leftStick.X;
      direction.Y -= leftStick.Y; // Y is inverted on gamepad
    }

    // Normalize if not zero
    if (direction != Vector2.Zero)
    {
      direction.Normalize();
    }

    return direction;
  }

  /// <summary>
  /// Gets the zoom delta from mouse wheel or gamepad triggers.
  /// Returns 0 if no zoom input.
  /// </summary>
  public static int GetZoomDelta()
  {
    var delta = s_mouse.ScrollWheelDelta;

    // Gamepad triggers for zoom
    if (s_gamePad.CurrentState.Triggers.Right > 0.5f)
    {
      delta += 1;
    }
    if (s_gamePad.CurrentState.Triggers.Left > 0.5f)
    {
      delta -= 1;
    }

    return delta;
  }

  /// <summary>
  /// Returns true if the camera pan/drag action is active (right mouse button held).
  /// </summary>
  public static bool IsCameraPanActive()
  {
    return s_mouse.IsButtonDown(MouseButton.Right);
  }

  /// <summary>
  /// Gets the current mouse position for camera dragging or tooltip display.
  /// </summary>
  public static Vector2 GetMousePosition()
  {
    return new Vector2(s_mouse.X, s_mouse.Y);
  }

  /// <summary>
  /// Returns true if the fullscreen toggle was just triggered (F11 key).
  /// </summary>
  public static bool ToggleFullscreen()
  {
    return s_keyboard.WasKeyJustPressed(Keys.F11);
  }

  /// <summary>
  /// Returns true if the debug overlay toggle was just triggered (F12 key).
  /// </summary>
  public static bool ToggleDebugOverlay()
  {
    return s_keyboard.WasKeyJustPressed(Keys.F12);
  }

  /// <summary>
  /// Returns true if the settings panel toggle was just triggered (F10 key).
  /// </summary>
  public static bool ToggleSettingsPanel()
  {
    return s_keyboard.WasKeyJustPressed(Keys.F10);
  }
}