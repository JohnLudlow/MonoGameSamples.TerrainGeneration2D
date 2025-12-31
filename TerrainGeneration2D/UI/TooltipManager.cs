using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Input;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

/// <summary>
/// Manages tooltip display for tile information
/// </summary>
internal sealed class TooltipManager
{
    private readonly Camera2D _camera;
    private readonly ChunkedTilemap _tilemap;
    private Panel? _tooltipPanel;
    private TextRuntime? _tooltipText;
    private Point _lastTileCoords;
    private bool _isVisible;
    
    public TooltipManager(Camera2D camera, ChunkedTilemap tilemap)
    {
        _camera = camera ?? throw new System.ArgumentNullException(nameof(camera));
        _tilemap = tilemap ?? throw new System.ArgumentNullException(nameof(tilemap));
        _lastTileCoords = new Point(-1, -1);
        _isVisible = false;
    }
    
    public void Initialize()
    {
        // Create a panel for the tooltip background
        _tooltipPanel = new Panel();
        _tooltipPanel.WidthUnits = DimensionUnitType.RelativeToChildren;
        _tooltipPanel.HeightUnits = DimensionUnitType.RelativeToChildren;
        _tooltipPanel.Width = 4; // Add small padding
        _tooltipPanel.Height = 4; // Add small padding
        _tooltipPanel.IsVisible = false;
        _tooltipPanel.AddToRoot();
        
        // Add a background rectangle
        var background = new ColoredRectangleRuntime();
        background.WidthUnits = DimensionUnitType.RelativeToContainer;
        background.HeightUnits = DimensionUnitType.RelativeToContainer;
        background.Width = 0;
        background.Height = 0;
        background.Color = new Color(0, 0, 0, 200); // Semi-transparent black
        _tooltipPanel.AddChild(background);
        
        // Add text on top
        _tooltipText = new TextRuntime();
        _tooltipText.X = 2;
        _tooltipText.Y = 2;
        _tooltipText.Text = "";
        _tooltipText.Color = Color.Yellow; // Bright yellow text
        _tooltipPanel.AddChild(_tooltipText);
    }
    
    public void Update(Vector2 mouseScreenPosition)
    {
        if (_tooltipPanel is null || _tooltipText is null)
        {
            return;
        }
        
        // Convert mouse position to world coordinates
        Vector2 worldPosition = _camera.ScreenToWorld(mouseScreenPosition);
        
        // Convert to tile coordinates
        int tileX = (int)(worldPosition.X / _tilemap.TileSize);
        int tileY = (int)(worldPosition.Y / _tilemap.TileSize);
        
        // Check if tile coordinates changed
        if (tileX != _lastTileCoords.X || tileY != _lastTileCoords.Y)
        {
            _lastTileCoords = new Point(tileX, tileY);
            
            // Check if tile is within map bounds
            if (tileX >= 0 && tileX < _tilemap.MapSizeInTiles &&
                tileY >= 0 && tileY < _tilemap.MapSizeInTiles)
            {
                int tileId = _tilemap.GetTile(tileX, tileY);
                Point chunkCoords = ChunkedTilemap.TileToChunkCoordinates(tileX, tileY);
                
                _tooltipText.Text = $"Tile:[{tileX},{tileY}] Type:{tileId} Chunk:[{chunkCoords.X},{chunkCoords.Y}]";
                _isVisible = true;
            }
            else
            {
                _isVisible = false;
            }
        }
        
        // Position tooltip offset from cursor (screen coordinates)
        _tooltipPanel.X = mouseScreenPosition.X + 10f;
        _tooltipPanel.Y = mouseScreenPosition.Y + 10f;
        _tooltipPanel.IsVisible = _isVisible;
    }
    
    public void Hide()
    {
        if (_tooltipPanel is not null)
        {
            _tooltipPanel.IsVisible = false;
            _isVisible = false;
        }
    }
}
