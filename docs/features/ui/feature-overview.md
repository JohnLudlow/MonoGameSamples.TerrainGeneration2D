# UI System: Developer Onboarding Overview

## Purpose

This document introduces the UI system in MonoGameSamples.TerrainGeneration2D for new contributors. It explains the overall design, integration with scenes, and best practices for extending or customizing UI components.

## What is the UI System?

- The UI is built using Gum, a lightweight, scene-friendly UI toolkit.
- All UI is constructed per scene (e.g., GameScene), and is rebuilt on scene change to avoid lifecycle/input bugs.
- Input is routed through a central [GameController.cs](../../../TerrainGeneration2D/GameController.cs) abstraction, not directly from Gum or MonoGame input APIs.

## Key Components

- **Game Scene UI**: Pause and game-over panels, on-screen hints, and audio feedback. See [game-scene-ui.md](game-scene-ui.md).
- **Runtime Settings Panel**: F10 panel for adjusting heuristics and triggering chunk regeneration. See [runtime-settings-panel.md](runtime-settings-panel.md).
- **Tooltip Manager**: Shows tile/chunk info near the cursor for debugging. See [tooltip-manager.md](tooltip-manager.md).
- **Debug Overlay**: F12 overlay for visualizing chunk/viewport state. See [debug-overlay.md](debug-overlay.md).

## Extending or Customizing the UI

- **To add a new UI panel or overlay**: Create a new Gum component and instantiate it in the relevant scene's Initialize method. Add it to the Gum root.
- **To add new input actions**: Extend [GameController.cs](../../../TerrainGeneration2D/GameController.cs) and use its methods in your UI logic.
- **To wire up runtime settings**: Bind UI controls to the appropriate properties or callbacks in [RuntimeSettingsPanel.cs](../../../TerrainGeneration2D/UI/RuntimeSettingsPanel.cs).

## Key Files & Entry Points

- [GameSceneUI.cs](../../../TerrainGeneration2D/UI/GameSceneUI.cs): Main UI overlay for the game scene.
- [RuntimeSettingsPanel.cs](../../../TerrainGeneration2D/UI/RuntimeSettingsPanel.cs): Settings panel logic and Gum controls.
- [TooltipManager.cs](../../../TerrainGeneration2D/UI/TooltipManager.cs): Tooltip logic and camera/tilemap integration.
- [GameController.cs](../../../TerrainGeneration2D/GameController.cs): Input abstraction for all UI and scene logic.
- [GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs): Scene lifecycle and UI construction.

## Best Practices

- Always clear and rebuild Gum UI on scene change to avoid stale state.
- Use GameController for all input checks to ensure correct lifecycle and avoid input bugs.
- Keep UI logic lightweight; avoid per-frame allocations or string churn.
- Document new UI components in /docs/features/ui and link from the area README.

## Further Reading

- [UI Area Index](README.md)
- [Game Scene UI](game-scene-ui.md)
- [Runtime Settings Panel](runtime-settings-panel.md)
- [Tooltip Manager](tooltip-manager.md)
- [Debug Overlay](debug-overlay.md)
