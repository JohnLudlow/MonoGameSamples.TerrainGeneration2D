using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;

public class InputManager
{
  public KeyboardInfo Keyboard { get; }
  public MouseInfo Mouse { get; }
  public IReadOnlyCollection<GamePadInfo> GamePads { get; private set; }

  public InputManager()
  {
    Keyboard = new KeyboardInfo();
    Mouse = new MouseInfo();
    GamePads =
    [
      new GamePadInfo(PlayerIndex.One),
      new GamePadInfo(PlayerIndex.Two),
      new GamePadInfo(PlayerIndex.Three),
      new GamePadInfo(PlayerIndex.Four)
    ];
  }

  /// <summary>
  /// Updates the state information about all input devices.
  /// </summary>
  public void Update(GameTime gameTime)
  {
    Keyboard.Update();
    Mouse.Update();

    foreach (var gamePad in GamePads)
    {
      gamePad.Update(gameTime);
    }
  }
}