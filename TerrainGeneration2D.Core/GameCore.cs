using System;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Audio;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public class GameCore : Game
{
  private static GameCore? _instance;
  private ConsoleEventListener? _eventListener;

  /// <summary>
  /// Gets a reference to the Core instance.
  /// </summary>
  public static GameCore? Instance
  {
    get
    {
      return _instance;
    }
  }

  private static Scene? _activeScene;
  private static Scene? _nextScene;

  /// <summary>
  /// Gets the graphics device manager to control the presentation of graphics.
  /// </summary>
  public static GraphicsDeviceManager? Graphics { get; private set; }

  /// <summary>
  /// Gets the graphics device used to create graphical resources and perform primitive rendering.
  /// </summary>
  public new static GraphicsDevice? GraphicsDevice { get; private set; }

  /// <summary>
  /// Gets the sprite batch used for all 2D rendering.
  /// </summary>
  public static SpriteBatch? SpriteBatch { get; private set; }

  /// <summary>
  /// Gets the content manager used to load global assets.
  /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public new static ContentManager Content { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

  public static InputManager Input { get; private set; } = new InputManager();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static AudioController Audio { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

  public bool EnablePerformanceDiagnostics { get; set; }

  /// <summary>
  /// Creates a new Core instance.
  /// </summary>
  /// <param name="windowTitle">The title to display in the title bar of the game window.</param>
  /// <param name="windowWidthInPixels">The initial width, in pixels, of the game window.</param>
  /// <param name="windowHeightInPixels">The initial height, in pixels, of the game window.</param>
  /// <param name="isFullScreenWindow">Indicates if the game should start in fullscreen mode.</param>
  public GameCore(string windowTitle, int windowWidthInPixels, int windowHeightInPixels, bool isFullScreenWindow = false)
  {
    _instance = this;

    Graphics = new GraphicsDeviceManager(this)
    {
      PreferredBackBufferWidth = windowWidthInPixels,
      PreferredBackBufferHeight = windowHeightInPixels,
      IsFullScreen = isFullScreenWindow
    };
    Graphics.ApplyChanges();

    Window.Title = windowTitle;
    Window.AllowUserResizing = true;

    Content = base.Content;
    Content.RootDirectory = "Content";

    IsMouseVisible = true;
  }

  protected override void Initialize()
  {
    base.Initialize();
    GraphicsDevice = base.GraphicsDevice;
    SpriteBatch = new SpriteBatch(GraphicsDevice);
    Input = new InputManager();
    Audio = new AudioController();

    if (EnablePerformanceDiagnostics)
    {
      _eventListener = new ConsoleEventListener();
    }
  }

  protected override void Update(GameTime gameTime)
  {
    // Update the input manager.
    Input.Update(gameTime);
    Audio?.Update();

    if (_nextScene is not null)
      TransitionScene();

    _activeScene?.Update(gameTime);

    base.Update(gameTime);
  }

  protected override void UnloadContent()
  {
    Audio?.Dispose();
    base.UnloadContent();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _eventListener?.Dispose();
    }
    base.Dispose(disposing);
  }

  protected override void Draw(GameTime gameTime)
  {
    // If there is an active scene, draw it.
    if (_activeScene != null)
    {
      _activeScene.Draw(gameTime);
    }

    base.Draw(gameTime);
  }

  public static void ChangeScene(Scene next)
  {
    // Only set the next scene value if it is not the same
    // instance as the currently active scene.
    if (_activeScene != next)
    {
      _nextScene = next;
    }
  }

  private static void TransitionScene()
  {
    // If there is an active scene, dispose of it.
    if (_activeScene != null)
    {
      _activeScene.Dispose();
    }

    // Force the garbage collector to collect to ensure memory is cleared.
    GC.Collect();

    // Change the currently active scene to the new scene.

    _activeScene = _nextScene;

    // Null out the next scene value so it does not trigger a change over and over.
    _nextScene = null;

    // If the active scene now is not null, initialize it.
    // Remember, just like with Game, the Initialize call also calls the
    // Scene.LoadContent
    if (_activeScene != null)
    {
      _activeScene.Initialize();
    }
  }
}