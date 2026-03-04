using System;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Scenes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using CoreGame = JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.GameCore;
using Gum.Managers;

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
    GameLoggerMessages.MonoGameInitBegin(_log, Environment.ProcessId);
    base.Initialize();

    GameLoggerMessages.MonoGameInitWindow(
      _log,
      GraphicsDevice!.PresentationParameters.BackBufferWidth,
      GraphicsDevice.PresentationParameters.BackBufferHeight);

    Window.ClientSizeChanged += OnClientSizeChanged;

    if (Audio is null) throw new InvalidOperationException($"Unable to start game if {nameof(Audio)} is null");

    Audio.SongVolume = 0;
    Audio.PlaySong(_themeSong!);

    InitializeGum();

#pragma warning disable CA2000 // Dispose objects before losing scope
    ChangeScene(new GameScene());
#pragma warning restore CA2000 // Dispose objects before losing scope
    GameLoggerMessages.MonoGameInitEnd(_log);
  }

  private void OnClientSizeChanged(object? sender, EventArgs e)
  {
    Graphics!.ApplyChanges();
  }

  protected override void LoadContent()
  {
    GameLoggerMessages.MonoGameLoadContentBegin(_log);
    base.LoadContent();
    if (Content is null) throw new InvalidOperationException($"Unable to start game if {nameof(Content)} is null");

    _themeSong = Content.Load<Song>("audio/theme");
    GameLoggerMessages.MonoGameLoadContentEnd(_log);
  }

  protected override void Update(GameTime gameTime)
  {
    GameLoggerMessages.MonoGameUpdateBegin(_log);
    base.Update(gameTime);
    GameLoggerMessages.MonoGameUpdateEnd(_log);
  }

  protected override void Draw(GameTime gameTime)
  {
    GameLoggerMessages.MonoGameDrawBegin(_log);
    base.Draw(gameTime);
    GameLoggerMessages.MonoGameDrawEnd(_log);
  }

  protected override void UnloadContent()
  {
    _themeSong?.Dispose();

    base.UnloadContent();
  }

  protected override void Dispose(bool disposing)
  {
    GameLoggerMessages.MonoGameExitBegin(_log);
    if (_disposed) return;

    if (disposing)
    {
      _themeSong?.Dispose();
      _disposed = true;
    }

    base.Dispose(disposing);
    GameLoggerMessages.MonoGameExitEnd(_log);
  }

  private void InitializeGum()
  {
    if (Content is null) throw new InvalidOperationException($"Unable to start game if {nameof(Content)} is null");

    GumService.Default.Initialize(this, "UI/TerrainGenerationOptionsScreen.gumx");
    var screen = ObjectFinder.Self?.GumProjectSave?.Screens[0]?.ToGraphicalUiElement() ?? throw new InvalidOperationException("Unable to get UI screen");
    screen.AddToRoot();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
    GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
  }
}