# MonoGame TerrainGeneration2D — AI Agent Guide

Concise, project-specific rules to get productive immediately.

## Big Picture
- Single game entry: [TerrainGeneration2D/TerrainGenerationGame.cs](../TerrainGeneration2D/TerrainGenerationGame.cs) pushes `GameScene` on init; only one scene runs.
- Core loop & services: [TerrainGeneration2D.Core/Core.cs](../TerrainGeneration2D.Core/Core.cs) wires graphics/content/input/audio and enforces safe `ChangeScene` (dispose + `GC.Collect` then `Initialize`).
- Core building blocks: `Camera2D`, `ChunkedTilemap`, WFC (`WfcProvider`, heuristics), `TileTypeRegistry`. See [docs/architecture-class-diagram.md](../docs/architecture-class-diagram.md).

## World & Saves
- 2048×2048 tiles, chunked 64×64. Buffer loads a 3×3-ish area around the viewport via `UpdateActiveChunks(viewport)`.
- Chunks auto-save/unload when leaving buffer; all dirty chunks saved on scene unload.
- Save format: gzipped `map_{cx}_{cy}.dat` under `Content/saves` at runtime (relative to `AppDomain.CurrentDomain.BaseDirectory`). Use `ClearAllSavedChunks()` or delete the folder to force regeneration.
  - Source: [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)

## Input & Camera
- Use [TerrainGeneration2D/GameController.cs](../TerrainGeneration2D/GameController.cs) only (scenes must not touch `InputManager`).
- Calls to use: `GetCameraMovement()`, `GetZoomDelta()`, `IsCameraPanActive()`, `GetMousePosition()`.
- Toggles: F10 settings panel, F11 fullscreen, F12 debug overlay.
- Movement: WASD/arrows + left stick. Zoom: mouse wheel + triggers. Right-click drag pans; scale by `Camera2D.Zoom`.

## UI & Settings
- Gum UI: `GameScene.Initialize()` clears `GumService.Default.Root` and rebuilds UI each scene. See [TerrainGeneration2D/UI/GameSceneUI.cs](../TerrainGeneration2D/UI/GameSceneUI.cs).
- Tooltips: [TerrainGeneration2D/UI/TooltipManager.cs](../TerrainGeneration2D/UI/TooltipManager.cs) uses `Camera2D.ScreenToWorld` + `ChunkedTilemap.TileToChunkCoordinates`. Text: `Tile:[x,y] Type:id Chunk:[cx,cy]` and updates only when tile coords change.
- Runtime settings (F10): adjust heuristics, WFC time budget, regenerate visible chunks, and clear saves via `RuntimeSettingsPanel`.

## Debug Overlay
- F12 draws active-chunk borders (orange=dirty, green=clean) and viewport bounds using `ChunkedTilemap.ActiveChunkInfo`; validate culling/buffer logic.

## Configuration
- `appsettings.json` drives terrain gen: sections `WfcWeights`, `TerrainRules`, `HeightMap`, `Heuristics`, `WfcRuntime` (time budget).
- `GameScene` reads config and constructs `ChunkedTilemap(tileset, 2048, 12345, saveDir, useWFC: true, …)` with those values.

## Content & Assets
- Content builder lives in [TerrainGeneration2D.Content/Builder](../TerrainGeneration2D.Content/Builder). Rebuild after asset edits.
- Load by relative names: `images/terrain-atlas`, `audio/theme`, `audio/ui`, `fonts/04b_30.fnt`.

## Gotchas
- Saves live under `Content/saves` at runtime. A clean that deletes the Content folder or a fresh clone will remove saves, causing regeneration on next run.
- Regeneration uses current config and heuristics; use F10 panel to tweak, then `RegenerateChunksInView(...)` or `ClearAllSavedChunks()` to see immediate effects.
- Scenes must not access `InputManager` directly—route through `GameController` to avoid lifecycle/input-state bugs.

## Diagnostics
- DEBUG builds set `EnablePerformanceDiagnostics = true` (see [TerrainGeneration2D/TerrainGenerationGame.cs](../TerrainGeneration2D/TerrainGenerationGame.cs)).
- Use console listener or external tools as in [TerrainGeneration2D.Core/Diagnostics/README.md](../TerrainGeneration2D.Core/Diagnostics/README.md). Custom event IDs start at 10 (`TerrainPerformanceEventSource` uses 10–17).

## Design & Documentation
- Place design notes and theory under [docs](../docs) with focused pages (e.g., WFC in [docs/map-generation/wfc/README.md](../docs/map-generation/wfc/README.md)).
- Define domain terms once and reuse; prefer short glossaries linked from [docs/README.md](../docs/README.md).
- When math helps, include KaTeX blocks; keep equations near the algorithm discussion.
- For component docs, add a short README next to code and link from the main docs index.

### Documentation Outputs (for feature elaboration)
- Deliver a standalone doc under [docs](../docs) or a component README alongside code; link it from [docs/README.md](../docs/README.md).
- Include: overview, intent/use-cases, architecture/data flow references (with links), domain terms, constraints, and example snippets.
- Use KaTeX for math: inline `$...$`, blocks `$$...$$` near the relevant algorithm.
- Reference concrete files (e.g., [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)) and config keys from [TerrainGeneration2D/appsettings.json](../TerrainGeneration2D/appsettings.json).
- Add “Follow-ups/Decisions” where trade-offs exist; keep a small changelog if behavior changes.
- Validate links via `scripts/check-doc-links.ps1`.

### Documentation Principles
- Structure for scanability: short sections, descriptive headers, 4–6 bullets each.
- Write with purpose: explain “why” behind design and constraints; minimize filler.
- Consistent terminology: reuse defined domain terms; avoid synonyms that dilute meaning.
- Code samples: compile-ready and minimal; include XML docs for public APIs and non-trivial methods.

## Workflows
- Build: `dotnet build TerrainGeneration2D.slnx`
- Run: `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`
- Test: `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`

## Conventions
- MonoGame lifecycle order: `Initialize`, `LoadContent`, `Update`, `Draw`, then unload/cleanup.
- Member order: private fields, properties, ctors, public methods, private methods. Fields use `_camelCase` prefix `_`.
- Prefer `var` when type is evident. Avoid public settable fields. Favor get-only properties.
- Follow repo analyzers/formatting in [.editorconfig](../.editorconfig).

## Coding & Quality Gates
- Provide clear, minimal, compile-ready samples; include XML docs for public APIs and non-trivial methods.
- Keep hot-path allocations low in rendering and WFC; avoid LINQ/boxing inside per-frame loops. Use `WfcTimeBudgetMs` for runtime limits.
- Respect scene lifecycle: avoid touching `InputManager` directly; don’t allocate new `SpriteBatch`/`GraphicsDevice` instances.
- Strive for zero new warnings; match styles from [.editorconfig](../.editorconfig). Prefer fast paths over convenience in tight loops.
- When changing production code, add or update tests in [TerrainGeneration2D.Tests](../TerrainGeneration2D.Tests) to maintain coverage and behavior.

## Tooling Policy
- Allowed: `dotnet build/test/run`; `scripts/check-doc-links.ps1`; diagnostics via `ConsoleEventListener`, `dotnet-counters`, `dotnet-trace`, PerfView.
- Allowed: Content pipeline via [TerrainGeneration2D.Content/Builder](../TerrainGeneration2D.Content/Builder); Gum UI components.
- Avoid: introducing heavy new dependencies without discussion; modifying `.editorconfig` or global solution settings; direct `InputManager` access from scenes.
- Prefer: small, isolated changes with references to the exact files updated; link new docs from [docs/README.md](../docs/README.md).

## Per-Task Process
- Understand intent: restate goal; list affected files/APIs (link them).
- Plan: outline minimal steps and risks; prefer small, reversible changes.
- Implement: respect `.editorconfig`, scene lifecycle, and performance constraints.
- Build/tests:
  - `dotnet build TerrainGeneration2D.slnx`
  - `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
- Validate docs (when edited): `scripts/check-doc-links.ps1`.
- Run (when applicable): `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`.
- Save behavior: if changes affect generation, use F10 panel and optionally clear `Content/saves`.
- Hand-off: summarize changes, link files, call out follow-ups.

### Implementation & Refactoring (docs-first)
- Document-before-implement: add/update a short design or component README describing intent, constraints, and impacts.
- XML documentation: ensure meaningful summaries/remarks for public APIs; call out performance expectations for hot paths.
- Tests & coverage: update/add unit tests in [TerrainGeneration2D.Tests](../TerrainGeneration2D.Tests); keep behavior consistent unless intentionally changed.
- Benchmarks: when performance-sensitive, run [TerrainGeneration2D.Benchmarks](../TerrainGeneration2D.Benchmarks) and capture notes.
  - `dotnet run --project TerrainGeneration2D.Benchmarks/TerrainGeneration2D.Benchmarks.csproj`
- Diagnostics: use `ConsoleEventListener`, `dotnet-counters`, or `dotnet-trace` when changing WFC or chunking.
- Performance hygiene: avoid LINQ/boxing in per-frame loops; use `WfcTimeBudgetMs`; minimize allocations in draw/update.
- Propagate docs: update references in [docs/README.md](../docs/README.md) and validate with the link checker.

## Instructions vs Skills
- Use these instructions (this file) for high-level process, architecture, constraints, and conventions shared across tasks.
- Use skills for repeatable, automatable sequences the agent should execute (e.g., "build-and-test", "check-doc-links", "run-game", "clean-saves").
- Guidance:
  - If it’s a rule or preference → document here.
  - If it’s a command sequence the agent should run → define a skill.
  - If it’s repo-specific knowledge (paths, toggles, config sections) → document here and reference from skills.
 - Skills catalog: see [agent-skills.md](agent-skills.md).

## Templates & Conventions
- Feature docs: prefer `docs/features/<feature-name>.md` or component README alongside code. Use [docs/templates/feature-doc-template.md](../docs/templates/feature-doc-template.md) to standardize structure.
- PRs: use the checklist at [pull-request-checklist.md](pull-request-checklist.md) to enforce docs-first, tests/benchmarks, diagnostics, and formatting.
 - Filename convention: use kebab-case for `<feature-name>` (e.g., `chunked-tilemap.md`); avoid spaces; keep names concise and representative of the component or capability.

## Key References
- Scene system and services: [TerrainGeneration2D.Core/Core.cs](../TerrainGeneration2D.Core/Core.cs)
- Scene implementation: [TerrainGeneration2D/Scenes/GameScene.cs](../TerrainGeneration2D/Scenes/GameScene.cs)
- Camera & drawing: [TerrainGeneration2D.Core/Graphics/Camera2D.cs](../TerrainGeneration2D.Core/Graphics/Camera2D.cs), [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs)
- Mapping & WFC: [TerrainGeneration2D.Core/Mapping](../TerrainGeneration2D.Core/Mapping)
- Tutorials: [docs/terrain2d-tutorial/README.md](../docs/terrain2d-tutorial/README.md)
