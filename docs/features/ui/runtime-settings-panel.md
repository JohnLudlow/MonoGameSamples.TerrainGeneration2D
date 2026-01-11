# Runtime Settings Panel

## Overview

- Compact in-game UI (F10) to adjust terrain generation heuristics and runtime parameters.
- Applies changes by regenerating chunks within the camera’s expanded viewport and optionally clearing saved chunks.

## Intent & Use-Cases

- Tune `Heuristics` and `TerrainRules` without recompiling.
- Control Wave Function Collapse (WFC) time budget per chunk to balance quality vs. frame time.
- Quickly iterate: regenerate visible chunks; clear persisted saves for a clean slate.

## Architecture & References

- Scene integration: [TerrainGeneration2D/Scenes/GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs) constructs `RuntimeSettingsPanel` and binds callbacks.
- UI component: [TerrainGeneration2D/UI/RuntimeSettingsPanel.cs](../../../TerrainGeneration2D/UI/RuntimeSettingsPanel.cs) (panel visuals and controls).
- Tilemap operations: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../../../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs) provides `RegenerateChunksInView(...)`, `ClearAllSavedChunks()`, and `WfcTimeBudgetMs`.
- Input: F10 toggle via [TerrainGeneration2D/GameController.cs](../../../TerrainGeneration2D/GameController.cs).

## Domain Terms

- Heuristics: WFC selection strategies (domain entropy, Shannon entropy, most-constraining tie-break, etc.).
- Time budget: Per-chunk WFC time limit (ms) to keep generation work bounded.
- Visible regeneration: Refresh only chunks in the camera’s expanded viewport buffer to preview changes.

## Configuration Keys

Values come from [TerrainGeneration2D/appsettings.json](../../../TerrainGeneration2D/appsettings.json):

- `WfcWeights`: `Base`, `NeighborMatchBoost`
- `TerrainRules`: altitude thresholds and noise parameters
- `HeightMap`: scale and weight parameters for continent/mountain/detail noise
- `Heuristics`: boolean switches and biases
- `WfcRuntime`: `TimeBudgetMs`

## Usage Example

- Toggle panel (F10) at runtime.
- Adjust sliders/toggles, then:
  - Apply: calls `RegenerateChunksInView(...)` with the current camera viewport (overwrites saves when enabled).
  - Clear Saves: calls `ClearAllSavedChunks()` to delete `Content/saves/*.dat`; next loads regenerate using current settings.
- Time budget: get/set bound to `ChunkedTilemap.WfcTimeBudgetMs`.

```csharp
// [TerrainGeneration2D/Scenes/GameScene.cs]
_settingsPanel.Bind(
  heuristics,
  terrainConfig,
  getBudget: () => _chunkedTilemap?.WfcTimeBudgetMs ?? timeBudgetMs,
  setBudget: v => { if (_chunkedTilemap != null) _chunkedTilemap.WfcTimeBudgetMs = v; },
  regenerateVisible: () => { if (_chunkedTilemap != null && _camera != null) _chunkedTilemap.RegenerateChunksInView(_camera.ViewportWorldBounds, overwriteSaves: true); },
  clearSaves: () => { _chunkedTilemap?.ClearAllSavedChunks(); }
);
```

## Performance Notes

- Regeneration is bounded by `WfcTimeBudgetMs`; large buffers may still cause short bursts of work.
- Persisted saves minimize regeneration on subsequent runs; clearing saves forces full regeneration using current settings.

## Follow-ups / Decisions

- Consider batching regeneration over multiple frames to avoid spikes on very large viewports.
- Optionally add a busy indicator while regeneration is in progress.
