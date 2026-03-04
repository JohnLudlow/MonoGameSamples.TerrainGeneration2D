## Overview

This document describes how to bind the runtime heuristics and terrain configuration objects to the Gum-generated `TerrainGenerationOptionsScreen` so UI controls reflect current values and updates push changes back into the running simulation.

## Definition of Terms

- `HeuristicsConfiguration`: runtime model containing WFC heuristic toggles and numeric parameters.
- `TileTypeRuleConfiguration`: terrain rules container used by WFC tile selection.
- `TerrainGenerationOptionsScreen`: Gum-generated partial class representing the options UI.
- Binding: wiring UI events to update model fields and model-to-UI sync when values change.

## Requirements

- The generated Gum screen must expose controls (named elements) that correspond to the settings you want to edit (sliders, checkboxes, text inputs, buttons).
- The screen partial class (generated) should either expose typed members for those controls or provide lookup-by-name via the Gum runtime API.
- Changes in the UI must update the referenced config instances used by `ChunkedTilemap` and WFC at runtime.

## Implementation Steps (code snippets)

1) Add a `Bind` method to the generated screen partial (or use `CustomInitialize`) to accept the config objects and callbacks.

File: Screens/TerrainGenerationOptionsScreen.cs (partial class)

```csharp
using System;

namespace TerrainGeneration2D.Screens
{
    partial class TerrainGenerationOptionsScreen
    {
        private HeuristicsConfiguration? _heuristics;
        private TileTypeRuleConfiguration? _terrainRules;

        public void Bind(HeuristicsConfiguration heuristics, TileTypeRuleConfiguration terrainRules, Func<int> getBudget, Action<int> setBudget, Action regenerateVisible, Action clearSaves)
        {
            _heuristics = heuristics;
            _terrainRules = terrainRules;

            // Example: wire a checkbox named "UseDomainEntropyCheckBox"
            try
            {
                var cb = this.GetElementByName("UseDomainEntropyCheckBox"); // adjust API to your generated runtime
                if (cb != null)
                {
                    cb.Click += (s, e) => { _heuristics.UseDomainEntropy = ! _heuristics.UseDomainEntropy; };
                    // initialize UI from model
                    cb.IsChecked = _heuristics.UseDomainEntropy;
                }
            }
            catch { }

            // Example: wire a budget slider named "WfcBudgetSlider"
            try
            {
                var slider = this.GetElementByName("WfcBudgetSlider");
                if (slider != null)
                {
                    slider.ValueChanged += (s, e) => { setBudget((int)slider.Value); };
                    slider.Value = getBudget();
                }
            }
            catch { }

            // Wire buttons
            try { this.GetElementByName("RegenerateButton")?.Click += (s,e) => regenerateVisible(); } catch { }
            try { this.GetElementByName("ClearSavesButton")?.Click += (s,e) => clearSaves(); } catch { }
        }
    }
}
```

Notes: replace `GetElementByName`, event names, and properties with the actual methods/properties exposed by your Gum runtime (the generated code often exposes typed members for named elements, e.g. `public Gum.Wireframe.GumRuntime.Element UseDomainEntropyCheckBox;`). If typed members exist, prefer using them directly (e.g., `UseDomainEntropyCheckBox.Click += ...`).

2) Call `Bind` from `GameScene` when building the screen (example snippet to paste into `LoadContent` where `_optionsScreen` is created).

File: Scenes/GameScene.cs (existing location where `_optionsScreen` is instantiated)

```csharp
// after creating _optionsScreen and atlas
if (_optionsScreen != null)
{
    _optionsScreen.Bind(
        heuristics,
        terrainConfig,
        getBudget: () => _chunkedTilemap?.WfcTimeBudgetMs ?? 50,
        setBudget: v => { if (_chunkedTilemap != null) _chunkedTilemap.WfcTimeBudgetMs = v; },
        regenerateVisible: () => { if (_chunkedTilemap != null && _camera != null) _chunkedTilemap.RegenerateChunksInView(_camera.ViewportWorldBounds, overwriteSaves: true); },
        clearSaves: () => { _chunkedTilemap?.ClearAllSavedChunks(); }
    );
}
```

3) Model -> UI updates

- If config values can change elsewhere, expose a method on the screen like `RefreshFromModel()` that copies current values into UI controls. Call this after any programmatic updates.

```csharp
public void RefreshFromModel()
{
    if (_heuristics == null) return;
    try { UseDomainEntropyCheckBox.IsChecked = _heuristics.UseDomainEntropy; } catch { }
    try { WfcBudgetSlider.Value = _heuristics.WfcTimeBudgetMs; } catch { }
}
```

4) Safety and threading

- Ensure UI events and model updates run on the main thread; MonoGame/Gum UI runs on the game thread so keep callbacks quick.

5) Testing

- Manually verify each control updates the corresponding model property and that pressing the regenerate/clear buttons calls the intended actions.
- Toggle the settings key (F10) and verify the screen shows/hides and remains in sync with the underlying model.

## Implementation Considerations

- Prefer typed generated members over reflection/string lookups for safety and compile-time checks.
- Keep binding logic small and isolated (in the screen partial) so tests can create the screen and call `Bind` with mock configs.
- Avoid long-running work from UI event handlers; offload heavy operations to background tasks if necessary.

## Follow-ups

- If the generated code already exposes a strongly-typed API for named controls, update the snippets above to use those members directly for better reliability.
