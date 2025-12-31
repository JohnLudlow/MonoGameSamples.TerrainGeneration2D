
using System;
using System.IO;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Scenes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Scenes;

/// <summary>
/// Main game scene with camera, chunked tilemap, and controls
/// </summary>
public class GameScene : Scene
{
    private const int MapSizeInTiles = 2048;
    private const int MasterSeed = 12345;
    private const float CameraSpeed = 400f; // pixels per second
    
    private ChunkedTilemap? _chunkedTilemap;
    private Camera2D? _camera;
    private TooltipManager? _tooltipManager;
    private Vector2? _lastMouseDragPosition;
    private Label? _testLabel;

    private GameSceneUI _ui;
    
    public override void Initialize()
    {
      GumService.Default.Root.Children.Clear();

      base.Initialize();
      
      _ui = new GameSceneUI();
    }
    
    public override void LoadContent()
    {
        base.LoadContent();
        
        // Load terrain atlas texture and create tileset region
        var terrainAtlasTexture = Content.Load<Texture2D>("images/terrain-atlas");
        var tilesetRegion = new TextureRegion(terrainAtlasTexture, 0, 0, 20, 160);
        var tileset = new Tileset(tilesetRegion, 20, 20); // 20x20 tile size, 8 tiles vertically (0-7)
        
        // Create save directory in Content folder
        string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "saves");
        
        // Create chunked tilemap
        _chunkedTilemap = new ChunkedTilemap(tileset, MapSizeInTiles, MasterSeed, saveDir);
        
        // Create camera
        if (JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.GraphicsDevice != null)
        {
            _camera = new Camera2D(JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.GraphicsDevice.Viewport);
            
            // Start at center of map
            int centerTile = MapSizeInTiles / 2;
            _camera.Position = new Vector2(centerTile * tileset.TileWidth, centerTile * tileset.TileWidth);
        }
        
        // Create tooltip manager
        if (_camera != null && _chunkedTilemap != null)
        {
            _tooltipManager = new TooltipManager(_camera, _chunkedTilemap);
            _tooltipManager.Initialize();
        }

        // Test label to verify Gum is working
        _testLabel = new Label();
        _testLabel.X = 80;
        _testLabel.Y = 240;
        _testLabel.Text = "Test Label - Gum Working!";
        _testLabel.AddToRoot();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _ui.Update(gameTime);
        
        if (_camera == null || _chunkedTilemap == null)
        {
            return;
        }
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Handle fullscreen toggle
        if (GameController.ToggleFullscreen())
        {
            var graphics = JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.Graphics;
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
        int scrollDelta = GameController.GetZoomDelta();
        if (scrollDelta != 0)
        {
            float zoomDelta = Math.Sign(scrollDelta) * Camera2D.ZoomIncrement;
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
        
        // Update tooltip
        _tooltipManager?.Update(GameController.GetMousePosition());
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer
        JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.GraphicsDevice?.Clear(Color.Black);
        
        if (_camera == null || _chunkedTilemap == null)
        {
            base.Draw(gameTime);
            return;
        }
        
        var spriteBatch = JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Core.SpriteBatch;
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
        
        // Draw Gum UI
        GumService.Default.Draw();

        base.Draw(gameTime);
    }
    
    public override void UnloadContent()
    {
        // Save all chunks before exiting
        _chunkedTilemap?.SaveAll();
        
        base.UnloadContent();
    }
}
