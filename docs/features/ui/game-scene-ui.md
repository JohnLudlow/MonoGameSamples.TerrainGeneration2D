# Game Scene UI

## Overview

- Lightweight Gum-based UI overlay created and managed per scene.
- Provides pause/game-over panels with buttons and a subtle on-screen hint for controls.

## Intent & Use-Cases

- Show pause and game-over states with clear actions (Resume/Retry/Quit).
- Surface runtime control hints for testers (F10/F11/F12) without clutter.
- Integrate UI feedback sounds via the shared audio controller.

## Architecture & References

- Component: [TerrainGeneration2D/UI/GameSceneUI.cs](../../../TerrainGeneration2D/UI/GameSceneUI.cs).
- Scene integration: constructed in `GameScene.Initialize()` and updated/drawn each frame.
  - See [TerrainGeneration2D/Scenes/GameScene.cs](../../../TerrainGeneration2D/Scenes/GameScene.cs).
- Input toggles: mapped in [TerrainGeneration2D/GameController.cs](../../../TerrainGeneration2D/GameController.cs) (F10 settings, F11 fullscreen, F12 debug overlay).
- Gum setup: initialized once in the game host; see [TerrainGeneration2D/TerrainGenerationGame.cs](../../../TerrainGeneration2D/TerrainGenerationGame.cs) `InitializeGum()`.

## Controls & Behavior

- Panels
  - Pause: centered panel with `RESUME` and `QUIT`; shows on pause; hides on resume.
  - Game Over: centered panel with `RETRY` and `QUIT`; shows on game over; hides on retry.
- Hints
  - Bottom-left text: "F10: Settings • F11: Fullscreen • F12: Debug".
- Focus management
  - Buttons gain focus when the panel is shown to support keyboard/gamepad navigation.
- UI feedback
  - Button clicks and focus events play a UI sound via the audio controller.

## Usage Example

```csharp
// [TerrainGeneration2D/Scenes/GameScene.cs]
public override void Initialize()
{
  GumService.Default.Root.Children.Clear();
  base.Initialize();
  _ui = new GameSceneUI();
}

public override void Update(GameTime gameTime)
{
  _ui.Update(gameTime);
}

public override void Draw(GameTime gameTime)
{
  // ... draw world
  GumService.Default.Draw();
}
```

## Performance Notes

- UI updates/draws are lightweight; avoid per-frame string churn outside of explicit user actions.
- Keep UI allocations out of hot paths; panels are created once during scene initialization.
