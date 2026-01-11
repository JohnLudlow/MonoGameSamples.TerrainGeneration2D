# UI Functional Area

## Overview

- Compact, scene-integrated UI built with Gum to support runtime control and debugging.
- Components are lightweight, per-scene constructs; toggles use centralized input via `GameController`.
- Rendering order keeps overlays readable while minimizing impact on frame time.

## Components

- [Game Scene UI](game-scene-ui.md): Pause/Game Over panels, hint text, audio feedback.
- [Runtime Settings Panel](runtime-settings-panel.md): F10 panel to adjust heuristics and regeneration.
- [Tooltip Manager](tooltip-manager.md): Tile and chunk info near cursor.
- [Debug Overlay](debug-overlay.md): F12 visualization of active chunks and viewport.

## Intent

- Keep UI lightweight and decoupled from core loops; regenerate or toggle features without impacting frame time.
- Centralize input through `GameController` to avoid lifecycle/input-state bugs.

## Navigation

- Up: [Features Hub](../README.md)

## References

- Gum initialization: [TerrainGeneration2D/TerrainGenerationGame.cs](../../../TerrainGeneration2D/TerrainGenerationGame.cs)
- Input mapping: [TerrainGeneration2D/GameController.cs](../../../TerrainGeneration2D/GameController.cs)
