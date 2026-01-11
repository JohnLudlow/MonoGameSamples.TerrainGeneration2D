# Tooltip Manager

## Overview

- Displays a small tooltip near the cursor with tile and chunk information.
- Updates only when tile coordinates change to reduce per-frame work.

## Intent & Use-Cases

- Provide quick visibility into world coordinates, tile IDs, and chunk boundaries.
- Aid debugging and validation of camera transforms and chunk mapping.

## Architecture & References

- Component: [TerrainGeneration2D/UI/TooltipManager.cs](../../../TerrainGeneration2D/UI/TooltipManager.cs).
- Camera: [TerrainGeneration2D.Core/Graphics/Camera2D.cs](../../../TerrainGeneration2D.Core/Graphics/Camera2D.cs) for `ScreenToWorld(...)` and viewport math.
- Tilemap: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) for `TileToChunkCoordinates(...)` and tile lookups.

## Behavior

- Converts screen coordinates to world coordinates via `Camera2D.ScreenToWorld(...)`.
- Computes tile indices from world position using `tile = floor(world/tileSize)`.
- Reads tile ID with `ChunkedTilemap.GetTile(tileX, tileY)` and chunk position using `ChunkedTilemap.TileToChunkCoordinates(...)`.
- Text format: `Tile:[x,y] Type:id Chunk:[cx,cy]`.
- Positions the tooltip panel with a small offset from the cursor and hides it when outside map bounds.

## Example

```csharp
// [TerrainGeneration2D/UI/TooltipManager.cs]
var world = _camera.ScreenToWorld(mouseScreenPosition);
var tileX = (int)(world.X / _tilemap.TileSize);
var tileY = (int)(world.Y / _tilemap.TileSize);
if (tileX >= 0 && tileX < _tilemap.MapSizeInTiles && tileY >= 0 && tileY < _tilemap.MapSizeInTiles)
{
  var tileId = _tilemap.GetTile(tileX, tileY);
  var chunk = ChunkedTilemap.TileToChunkCoordinates(tileX, tileY);
  _tooltipText.Text = $"Tile:[{tileX},{tileY}] Type:{tileId} Chunk:[{chunk.X},{chunk.Y}]";
  _tooltipPanel.IsVisible = true;
}
else
{
  _tooltipPanel.IsVisible = false;
}
```

## Performance Notes

- Tooltip updates only when tile coordinates change; avoids unnecessary string allocations each frame.
- Keep UI work light; color and panel are static elements.
