using System;
using Gum.Forms;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Scenes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Scenes;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using Microsoft.Extensions.Logging;
using CoreGame = JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

internal sealed class TerrainGenerationGame : CoreGame
{
  private Song? _themeSong;
  private readonly ILogger _log = Log.Create<TerrainGenerationGame>();

  private bool _disposed;

  public TerrainGenerationGame() : base("Dungeon Slime", 1280, 720, false)
  {
#if DEBUG
    EnablePerformanceDiagnostics = true;
#endif
  }

  protected override void Initialize()
  {
    base.Initialize();

    _log.LogInformation("Game initialize: resolution={width}x{height} fullscreen={fullscreen}",
      GraphicsDevice.PresentationParameters.BackBufferWidth,
      GraphicsDevice.PresentationParameters.BackBufferHeight,
      CoreGame.Graphics?.IsFullScreen ?? false);

    if (Audio is null) throw new InvalidOperationException($"Unable to start game if {nameof(Audio)} is null");
    
    Audio.SongVolume = 0;
    Audio.PlaySong(_themeSong);

    InitializeGum();    

    ChangeScene(new GameScene());
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    if (Content is null) throw new InvalidOperationException($"Unable to start game if {nameof(Content)} is null");

    _themeSong = Content.Load<Song>("audio/theme");
    _log.LogInformation("Content loaded: theme song and assets ready");
  }

  protected override void UnloadContent()
  {
    _themeSong?.Dispose();

    base.UnloadContent();
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed) return;

    if (disposing)
    {
      _themeSong?.Dispose();
    }

    base.Dispose(disposing);
  }

  private void InitializeGum()
  {
    if (Content is null) throw new InvalidOperationException($"Unable to start game if {nameof(Content)} is null");    

    GumService.Default.Initialize(this, DefaultVisualsVersion.V3);

    GumService.Default.ContentLoader.XnaContentManager = Content;
    FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
    FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
    
    FrameworkElement.TabReverseKeyCombos.Add(
      new KeyCombo { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up }
    );

    FrameworkElement.TabKeyCombos.Add(
      new KeyCombo { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down }
    );

    GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
    GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
  }
}
