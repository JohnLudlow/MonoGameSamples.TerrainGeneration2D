# Remove old code-only Gum UI — Implementation Plan

## Overview

This plan details removing the legacy code-only Gum UI and its demo-only assets (fonts, sprites, spritesheets, and custom UI configuration such as a 4x UI zoom) so the project can adopt a cleaner Gum-based UI authored in the Gum editor. The plan is staged to be reversible and to keep the repo buildable at each step.

## Table of contents

- Overview
- Plan issue
- Plan status
- Definition of terms
- Architectural considerations and constraints
- Implementation guide
- Implementation considerations
- Impacts & Risks
- Testing
- Migration notes
- References

## Plan issue

No issue created yet — add a link here once an issue/PR is opened to track the work.

## Plan status

- Completed (legacy code-only UI removed; content builder and docs updated)

## Definition of terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Code-only Gum UI | UI built in C# code rather than with Gum editor files (.gumx/.guc). | |
| Gum UI | UI authored with the Gum editor and loaded via `GumService`. | |
| Content builder | The content project that copies/builds runtime assets (TerrainGeneration2D.Content). | |
| 4x Zoom | The demo's UI/camera scale setting that effectively uses a 4× zoom for rendering text/icons. | |

## Architectural considerations and constraints

- `GumService.Default` is initialized from `TerrainGeneration2D/TerrainGenerationGame.cs` and used by scenes to add/remove UI nodes; preserve Gum initialization.
- Visual assets live in `TerrainGeneration2D.Content/Assets` and are included in the content builder (`TerrainGeneration2D.Content/Builder`). Remove asset files and builder includes when they are no longer used.
- Keep changes small and reversible: introduce a feature-flag/stub for UI removal and remove asset references from csproj/content builder only after code stops referencing them.

## Implementation guide

### Plan requirements

- (NOT STARTED) Remove runtime usage of code-only UI without breaking game startup
  - GIVEN the project builds and runs
  - WHEN code-only UI is removed behind a flag
  - THEN the game starts without the old UI and without runtime asset exceptions

- (NOT STARTED) Remove demo-only assets from the content project
  - GIVEN code no longer references a font/atlas
  - WHEN the content builder and csproj are updated
  - THEN the asset is not copied into runtime Content

### Phases

Phase 1 — Inventory (COMPLETE)

Objective

- Produce a complete list of code and asset references to the old UI.

Technical details

- Identified files: `TerrainGeneration2D/UI/GameSceneUI.cs`, `AnimatedButton.cs`, `TooltipManager.cs`, `RuntimeSettingsPanel.cs`, `OptionsGrid.cs`, `OptionsSlider.cs`, and references in `TerrainGeneration2D/Scenes/GameScene.cs`.
- Asset list: `images/terrain-atlas.png`, `images/atlas-definition.xml`, `fonts/04B_30.spritefont` / `04B_30_5x.spritefont`, `fonts/NotArial.fnt`.

Phase 2 — Add a no-op UI factory and feature flag (COMPLETED)

Objective

- During the transition a small no-op factory/shims were used so the project could be built and run while the legacy UI code was removed.

Technical details

- Temporary shim implementations (`OptionsGrid`, `OptionsSlider`, `RuntimeSettingsPanel`) were added to restore compilation when code references remained. Those shims were later removed once code references were eliminated.
- The changes to `GameScene` replaced direct legacy UI constructions with Gum-based usage and removed guarded legacy branches.

Phase 3 — Remove code-only classes (COMPLETED)

Objective

- The code-only UI source files under `TerrainGeneration2D/UI` were deleted and the solution was rebuilt to ensure no remaining references.

Phase 4 — Remove assets and content builder includes (COMPLETED)

Objective

- Demo fonts (`04B_30.spritefont`, `04B_30_5x.spritefont`, `NotArial.fnt`) and `Assets/images/logo.png` were removed from `TerrainGeneration2D.Content/Assets` and the content builder was updated accordingly. Required assets (terrain atlas and definitions) were left in place.

Phase 5 — Run build, tests, and manual verification (COMPLETED)

Objective

- The solution was built and unit/integration tests run. The game was launched locally and verified to run without the legacy UI and without missing-asset exceptions.

Phase 6 — Cleanup and docs (COMPLETED)

Objective

- Migration notes were added: `docs/features/ui/migration-notes.md` documents what was removed, verification steps, and next steps for adding Gum assets on a separate branch.

Migration notes: [docs/features/ui/migration-notes.md](../features/ui/migration-notes.md)

## Implementation considerations

- Readability: prefer small commits per phase and document changes in commit messages.
- Reliability: use the feature-flag/stub to keep the project runnable during incremental removal.
- Testability: add a smoke test that constructs `GameScene` and calls `Initialize()`/`LoadContent()` to ensure Gum root usage is safe.
- Performance: removing the demo UI will reduce startup allocations; ensure the new Gum-based UI initializes lazily.

## Impacts & Risks

- Possible build failures if references to deleted classes or assets remain — mitigate by staged removals and building between phases.
- Unit/integration tests that load UI assets may need mocking or updates.
- User-visible differences: the demo UI will disappear until replaced with new Gum editor UI.

## Testing

- Automated
  - `dotnet build TerrainGeneration2D.slnx` succeeds.
  - `dotnet test` for unit/integration/property tests succeeds.
  - Add a test that calls `GameScene.Initialize()` and `LoadContent()` and asserts no exceptions.

- Manual
  - Run `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj` and confirm the game starts without the old demo UI and no runtime exceptions for missing assets.
  - Verify camera controls and debug overlays still function.

## Migration notes

- Keep `GumService.Default.Initialize(...)` in `TerrainGenerationGame.cs` — new Gum UI should reuse this initialization.
- New Gum assets (.gumx/.guc) should be added to `TerrainGeneration2D.Content/Assets/UI` and referenced via the content builder.

## References

- `TerrainGeneration2D/UI/GameSceneUI.cs`
- `TerrainGeneration2D/Scenes/GameScene.cs`
- `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs`
