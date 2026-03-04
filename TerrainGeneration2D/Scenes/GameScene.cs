
using System;
using System.Collections.Generic;
using System.IO;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Scenes;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using Microsoft.Extensions.Logging;
using TerrainGeneration2D.Content.UI.Screens;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Scenes;

/// <summary>
/// Main game scene with camera, chunked tilemap, and controls
/// </summary>
internal sealed class GameScene : Scene
{
  private const int MapSizeInTiles = 2048;
#pragma warning disable CA1852
  private const int MasterSeed = 12345;
#pragma warning restore CA1852
  private const float CameraSpeed = 400f; // pixels per second

  private ChunkedTilemap? _chunkedTilemap;
  private Camera2D? _camera;
  // private TooltipManager? _tooltipManager;
  private Vector2? _lastMouseDragPosition;
#pragma warning disable CA2213 // Disposable fields should be disposed
  private Texture2D? _debugPixel;
#pragma warning restore CA2213 // Disposable fields should be disposed
  private bool _showDebugOverlay;
  private IReadOnlyCollection<ChunkedTilemap.ActiveChunkInfo> _activeChunkSnapshot = Array.Empty<ChunkedTilemap.ActiveChunkInfo>();
  private readonly ILogger _log = Log.Create<GameScene>();
  private HeuristicsConfiguration? _heuristicsConfig;
  private TerrainGenerationOptionsScreen? _optionsScreen;
  private bool _showSettings;
  private int _viewportWidth;
  private int _viewportHeight;

  public override void Initialize()
  {
    GumService.Default.Root.Children.Clear();

    base.Initialize();
  }

  public override void LoadContent()
  {
    base.LoadContent();

    // Load terrain atlas texture and create tileset region
    var terrainAtlasTexture = Content.Load<Texture2D>("images/terrain-atlas");
    var tilesetRegion = new TextureRegion(terrainAtlasTexture, 0, 0, 20, 160);
    var tileset = new Tileset(tilesetRegion, 20, 20); // 20x20 tile size, 8 tiles vertically (0-7)

    // Create save directory in Content folder
    var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "saves");

    // Create chunked tilemap
    // Read configurations from appsettings
    var cfg = Log.Config;
    var weightsSection = cfg.GetSection("WfcWeights");
    var weightConfig = new WfcWeightConfiguration
    {
      Base = weightsSection.GetValue<int>("Base", 1),
      NeighborMatchBoost = weightsSection.GetValue<int>("NeighborMatchBoost", 3)
    };

    var rulesSection = cfg.GetSection("TerrainRules");
    var terrainConfig = new TileTypeRuleConfiguration([
      // Ocean
      new GroupRuleConfiguration
      {
        Id = TerrainTileIds.Ocean,
        ElevationMax = rulesSection.GetValue<float>("OceanHeightMax", 0.34f)
      },
      // Beach
      new GroupRuleConfiguration
      {
        Id = TerrainTileIds.Beach,
        ElevationMin = rulesSection.GetValue<float>("BeachHeightMin", 0.33f),
        ElevationMax = rulesSection.GetValue<float>("BeachHeightMax", 0.48f),
        MinGroupSizeX = rulesSection.GetValue<int>("BeachOceanSizeMin", 12),
        MaxGroupSizeX = rulesSection.GetValue<int>("BeachOceanSizeMax", 180)
      },
      // Plains
      new GroupRuleConfiguration {
        Id = TerrainTileIds.Plains,
        ElevationMin = rulesSection.GetValue<float>("PlainsHeightMin", 0.35f),
        ElevationMax = rulesSection.GetValue<float>("PlainsHeightMax", 0.78f),
        MinGroupSizeX = rulesSection.GetValue<int>("BeachPlainsSizeMin", 20),
        MaxGroupSizeX = rulesSection.GetValue<int>("BeachPlainsSizeMax", 400)
      },
      // Forest
      new GroupRuleConfiguration {
        Id = TerrainTileIds.Forest,
        ElevationMin = rulesSection.GetValue<float>("ForestHeightMin", 0.42f),
        ElevationMax = rulesSection.GetValue<float>("ForestHeightMax", 0.88f)
      },
      // Snow
      new GroupRuleConfiguration {
        Id = TerrainTileIds.Snow,
        ElevationMin = rulesSection.GetValue<float>("SnowHeightMin", 0.82f)
      },
      // Mountain
      new GroupRuleConfiguration {
        Id = TerrainTileIds.Mountain,
        ElevationMin = rulesSection.GetValue<float>("MountainHeightMin", 0.76f),
        NoiseThreshold = rulesSection.GetValue<float>("MountainNoiseThreshold", 0.55f),
        MinGroupSizeX = rulesSection.GetValue<int>("MountainWidthMin", 3),
        MaxGroupSizeX = rulesSection.GetValue<int>("MountainWidthMax", 12),
        MinGroupSizeY = rulesSection.GetValue<int>("MountainRangeMin", 8),
        MaxGroupSizeY = rulesSection.GetValue<int>("MountainRangeMax", 48)
      }
    ]);

    var hmSection = cfg.GetSection("HeightMap");
    var heightConfig = new HeightMapConfiguration
    {
      ContinentScale = hmSection.GetValue<float>("ContinentScale", 0.0045f),
      MountainScale = hmSection.GetValue<float>("MountainScale", 0.02f),
      DetailScale = hmSection.GetValue<float>("DetailScale", 0.1f),
      ContinentWeight = hmSection.GetValue<float>("ContinentWeight", 0.75f),
      MountainWeight = hmSection.GetValue<float>("MountainWeight", 0.35f),
      DetailWeight = hmSection.GetValue<float>("DetailWeight", 0.25f)
    };

    var heurSection = cfg.GetSection("Heuristics");
    var heuristics = new HeuristicsConfiguration
    {
      UseDomainEntropy = heurSection.GetValue<bool>("UseDomainEntropy", true),
      UseShannonEntropy = heurSection.GetValue<bool>("UseShannonEntropy", false),
      UseMostConstrainingTieBreak = heurSection.GetValue<bool>("UseMostConstrainingTieBreak", true),
      ApplyInfluenceTieBreakForSingleHeuristic = heurSection.GetValue<bool>("ApplyInfluenceTieBreakForSingleHeuristic", true),
      PreferCentralCellTieBreak = heurSection.GetValue<bool>("PreferCentralCellTieBreak", false),
      UniformPickFraction = heurSection.GetValue<double>("UniformPickFraction", 0.0),
      MostConstrainingBias = heurSection.GetValue<double>("MostConstrainingBias", 0.0)
    };
    _heuristicsConfig = heuristics;

    var runtimeSection = cfg.GetSection("WfcRuntime");
    var timeBudgetMs = runtimeSection.GetValue<int>("TimeBudgetMs", 50);

    _chunkedTilemap = new ChunkedTilemap(tileset, MapSizeInTiles, MasterSeed, saveDir, useWaveFunctionCollapse: true, terrainRuleConfiguration: terrainConfig, heightMapConfiguration: heightConfig, weightConfig: weightConfig, heuristicsConfig: heuristics, logger: _log, wfcTimeBudgetMs: timeBudgetMs);

    // Settings UI: prefer Gum's XNA content manager when available, otherwise fall back to the scene Content manager
    var contentManager = GumService.Default?.ContentLoader?.XnaContentManager ?? Content;
    var atlas = TextureAtlas.FromFile(contentManager, "images/atlas-definition.xml");

    // Instantiate the Gum-generated screen for terrain generation options.
    // The generated screen class is a partial class under TerrainGeneration2D.Screens.
    _optionsScreen = new TerrainGenerationOptionsScreen();

    // If the generated screen exposes visibility or IsVisible, try to enable it.
    // Use dynamic to avoid hard compile-time coupling to generated members.
    if (_optionsScreen != null)
    {
      dynamic s = _optionsScreen;
      try { s.IsVisible = true; } catch { }
      try { s.Visible = true; } catch { }
    }

    // Create camera
    if (Core.GameCore.GraphicsDevice != null)
    {
      _camera = new Camera2D(Core.GameCore.GraphicsDevice.Viewport);
      _viewportWidth = Core.GameCore.GraphicsDevice.Viewport.Width;
      _viewportHeight = Core.GameCore.GraphicsDevice.Viewport.Height;

      // Start at center of map
      var centerTile = MapSizeInTiles / 2;
      _camera.Position = new Vector2(centerTile * tileset.TileWidth, centerTile * tileset.TileWidth);
    }

    // Create tooltip manager
    // if (_camera != null && _chunkedTilemap != null)
    // {
    //   _tooltipManager = new TooltipManager(_camera, _chunkedTilemap);
    //   _tooltipManager.Initialize();
    // }

    var graphicsDevice = Core.GameCore.GraphicsDevice;
    if (graphicsDevice != null)
    {
      _debugPixel = new Texture2D(graphicsDevice, 1, 1);
      _debugPixel.SetData(new[] { Color.White });
    }

    // Hint text is provided by GameSceneUI; remove temporary test label.
  }

  public override void Update(GameTime gameTime)
  {
    GameLoggerMessages.SceneUpdateBegin(_log);
    base.Update(gameTime);

    if (_camera == null || _chunkedTilemap == null)
    {
      return;
    }

    var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

    // Handle fullscreen toggle
    if (GameController.ToggleFullscreen())
    {
      var graphics = Core.GameCore.Graphics;
      if (graphics != null)
      {
        graphics.IsFullScreen = !graphics.IsFullScreen;
        graphics.ApplyChanges();
      }
    }

    // Handle camera movement
    Vector2 moveDirection = GameController.GetCameraMovement();
    if (moveDirection != Vector2.Zero)
    {
      _camera.Move(moveDirection * CameraSpeed * deltaTime / _camera.Zoom);
    }

    // Handle zoom
    var scrollDelta = GameController.GetZoomDelta();
    if (scrollDelta != 0)
    {
      var zoomDelta = Math.Sign(scrollDelta) * Camera2D.ZoomIncrement;
      _camera.AdjustZoom(zoomDelta);
    }

    // Handle camera pan with mouse drag
    if (GameController.IsCameraPanActive())
    {
      Vector2 currentMousePos = GameController.GetMousePosition();

      if (_lastMouseDragPosition.HasValue)
      {
        Vector2 delta = _lastMouseDragPosition.Value - currentMousePos;
        _camera.Move(delta / _camera.Zoom);
      }

      _lastMouseDragPosition = currentMousePos;
    }
    else
    {
      _lastMouseDragPosition = null;
    }

    // Update active chunks based on camera viewport
    _chunkedTilemap.UpdateActiveChunks(_camera.ViewportWorldBounds);

    // Handle window resize
    var graphicsDevice = Core.GameCore.GraphicsDevice;
    if (graphicsDevice != null && (graphicsDevice.Viewport.Width != _viewportWidth || graphicsDevice.Viewport.Height != _viewportHeight))
    {
      _viewportWidth = graphicsDevice.Viewport.Width;
      _viewportHeight = graphicsDevice.Viewport.Height;
      var oldPos = _camera.Position;
      var oldZoom = _camera.Zoom;
      _camera = new Camera2D(graphicsDevice.Viewport);
      _camera.Position = oldPos;
      _camera.Zoom = oldZoom;
    }

    if (GameController.ToggleDebugOverlay())
    {
      _showDebugOverlay = !_showDebugOverlay;
    }

    if (GameController.ToggleSettingsPanel())
    {
      _showSettings = !_showSettings;
      if (_optionsScreen != null)
      {
        dynamic s = _optionsScreen;
        try { s.IsVisible = _showSettings; } catch { }
        try { s.Visible = _showSettings; } catch { }
      }
    }

    if (_showDebugOverlay)
    {
      _activeChunkSnapshot = _chunkedTilemap.GetActiveChunkInfos();
    }

    // Update tooltip
    // _tooltipManager?.Update(GameController.GetMousePosition());
    GameLoggerMessages.SceneUpdateEnd(_log);
  }

  public override void Draw(GameTime gameTime)
  {
    GameLoggerMessages.SceneDrawBegin(_log);
    // Clear the back buffer
    Core.GameCore.GraphicsDevice?.Clear(Color.Black);

    if (_camera == null || _chunkedTilemap == null)
    {
      base.Draw(gameTime);
      return;
    }

    var spriteBatch = Core.GameCore.SpriteBatch;
    if (spriteBatch == null)
    {
      base.Draw(gameTime);
      return;
    }

    // Begin sprite batch with camera transform
    spriteBatch.Begin(
        samplerState: SamplerState.PointClamp,
        transformMatrix: _camera.GetTransformMatrix()
    );

    // Draw visible chunks
    _chunkedTilemap.Draw(spriteBatch, _camera.ViewportWorldBounds);

    spriteBatch.End();

    if (_showDebugOverlay)
    {
      spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
      DrawDebugOverlay(spriteBatch);
      spriteBatch.End();
    }

    // Draw Gum UI
    GumService.Default.Draw();
    GameLoggerMessages.SceneDrawEnd(_log);

    base.Draw(gameTime);
  }

  private void DrawDebugOverlay(SpriteBatch spriteBatch)
  {
    if (_camera == null || _chunkedTilemap == null || _debugPixel == null)
    {
      return;
    }

    foreach (var chunkInfo in _activeChunkSnapshot)
    {
      DrawChunkBoundary(spriteBatch, chunkInfo);
    }

    DrawViewportBoundary(spriteBatch);
  }

  private void DrawChunkBoundary(SpriteBatch spriteBatch, ChunkedTilemap.ActiveChunkInfo info)
  {
    if (_camera == null || _chunkedTilemap == null)
    {
      return;
    }

    var tileSize = _chunkedTilemap.TileSize;
    var chunkWorldSize = new Vector2(Chunk.ChunkSize * tileSize, Chunk.ChunkSize * tileSize);
    var worldTopLeft = new Vector2(info.WorldTilePosition.X * tileSize, info.WorldTilePosition.Y * tileSize);

    var topLeftScreen = _camera.WorldToScreen(worldTopLeft);
    var bottomRightScreen = _camera.WorldToScreen(worldTopLeft + chunkWorldSize);
    var rect = ConvertToScreenRect(topLeftScreen, bottomRightScreen);

    var color = info.IsDirty ? Color.OrangeRed : Color.LimeGreen;
    DrawRectangle(spriteBatch, rect, color, thickness: 2);
  }

  private void DrawViewportBoundary(SpriteBatch spriteBatch)
  {
    if (_camera == null)
    {
      return;
    }

    var viewport = _camera.ViewportWorldBounds;
    var topLeftScreen = _camera.WorldToScreen(new Vector2(viewport.Left, viewport.Top));
    var bottomRightScreen = _camera.WorldToScreen(new Vector2(viewport.Right, viewport.Bottom));
    var rect = ConvertToScreenRect(topLeftScreen, bottomRightScreen);

    DrawRectangle(spriteBatch, rect, Color.Cyan, thickness: 3);
  }

  private static Rectangle ConvertToScreenRect(Vector2 topLeft, Vector2 bottomRight)
  {
    var x = Math.Min(topLeft.X, bottomRight.X);
    var y = Math.Min(topLeft.Y, bottomRight.Y);
    var width = Math.Max(1f, Math.Abs(bottomRight.X - topLeft.X));
    var height = Math.Max(1f, Math.Abs(bottomRight.Y - topLeft.Y));

    return new Rectangle((int)x, (int)y, (int)width, (int)height);
  }

  private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness = 1)
  {
    if (_debugPixel == null)
    {
      return;
    }

    spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
    spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
    spriteBatch.Draw(_debugPixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
  }

  public override void UnloadContent()
  {
    // Save all chunks before exiting
    _chunkedTilemap?.SaveAll();

    _debugPixel?.Dispose();
    _debugPixel = null;

    base.UnloadContent();
  }
}