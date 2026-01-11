# Debug Overlay

## Overview

- In-game overlay that visualizes active chunk boundaries and the current viewport.
- Toggle with F12; colors indicate save/dirty state of chunks.

## Intent & Use-Cases

- Validate culling and the active chunk buffer while scrolling.
- Inspect viewport bounds vs. loaded chunk set to detect thrash or gaps.
- Confirm save/unload behavior as chunks leave the buffer.

## Architecture & References

- Scene logic and rendering: [TerrainGeneration2D/Scenes/GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs) (`_showDebugOverlay`, `DrawDebugOverlay`, chunk/viewport rectangles).
- Input toggle: [TerrainGeneration2D/GameController.cs](../../../TerrainGeneration2D/GameController.cs) (`ToggleDebugOverlay()` via F12).
- Camera transforms: [TerrainGeneration2D.Core/Graphics/Camera2D.cs](../../../TerrainGeneration2D.Core/Graphics/Camera2D.cs) for `WorldToScreen(...)` and `ViewportWorldBounds`.
- Chunk info: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) for `GetActiveChunkInfos()` (position, dirty flag) and `TileSize`.

## Behavior

- Colors: green = clean (saved), orange-red = dirty (pending save).
- Viewport rectangle: cyan outline to compare camera bounds with active chunks.
- Draw order: overlay renders after world tiles to ensure visibility.
- Pixel-based rectangles: uses a 1×1 texture to draw outlined rectangles via `SpriteBatch`.

## Usage Example

```csharp
// [TerrainGeneration2D/Scenes/GameScene.cs]
if (GameController.ToggleDebugOverlay())
{
  _showDebugOverlay = !_showDebugOverlay;
}

if (_showDebugOverlay)
{
  _activeChunkSnapshot = _chunkedTilemap.GetActiveChunkInfos();
}

// Later in Draw()
if (_showDebugOverlay)
{
  spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
  DrawDebugOverlay(spriteBatch);
  spriteBatch.End();
}
```

## Performance Notes

- Overlay draws are lightweight (lines only); keep disabled during regular play.
- Snapshot collection (`GetActiveChunkInfos`) happens when overlay is enabled; avoid extra allocations when disabled.
