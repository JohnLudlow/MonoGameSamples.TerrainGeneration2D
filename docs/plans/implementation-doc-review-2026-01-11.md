# Documentation Review and Fix Plan (2026-01-11)

## Overview

Review all documentation for correctness and adherence to repo standards, fix mismatches with implementation, and improve discoverability for existing runtime UI components. Address markdown lint noise from generated artifacts via ignore configuration.

## Definition of Terms

- EventSource: .NET tracing mechanism emitting events and counters for diagnostics.
- WFC (Wave Function Collapse): Constraint-based tile generation algorithm using domains, propagation, and optional backtracking.
- Heuristics: Selection strategies controlling WFC cell choice and tile weighting.
- Active Chunk Buffer: Expanded region of chunks around the camera viewport kept in memory.
- Time Budget: Per-chunk maximum time for WFC to prevent long stalls.

## Requirements

- Align docs with implemented behavior (debug overlay toggle, tooltip text, settings panel actions).
- Add component docs for `RuntimeSettingsPanel` and `TooltipManager` with architecture references.
- Keep docs consistent with conventions in `.github/copilot-instructions.md` (short sections, links to concrete files, math where relevant).
- Ensure doc links are valid using `scripts/check-doc-links.ps1`.
- Reduce markdownlint noise from generated benchmark artifacts without touching artifacts.

## Implementation Steps

1. Update performance/debugging doc
   - Replace "overlay plan" with implemented overlay description; link to [TerrainGeneration2D/Scenes/GameScene.cs](../../TerrainGeneration2D/Scenes/GameScene.cs) and [TerrainGeneration2D/GameController.cs](../../TerrainGeneration2D/GameController.cs).
   - Reference tooltip behavior via [TerrainGeneration2D/UI/TooltipManager.cs](../../TerrainGeneration2D/UI/TooltipManager.cs).
2. Add feature docs
   - `docs/features/runtime-settings-panel.md`: overview, intent, architecture links, config keys, usage example binding, performance notes.
   - `docs/features/tooltip-manager.md`: behavior and usage, camera/world conversion, text format, references, performance notes.
3. Update docs index
   - Link both new feature docs in [docs/README.md](../README.md).
4. Lint and link check
   - Run `npx markdownlint-cli **/*.md` and `scripts/check-doc-links.ps1` to validate edits.
5. Ignore generated artifacts (lint noise)
   - Add a root `.markdownlintignore` that excludes `BenchmarkDotNet.Artifacts/**` and similar generated files. Alternative: configure `markdownlint-cli` in CI to target `docs/**` only.

## Implementation Considerations

- Readability: Keep docs concise and scannable with short sections and code/file links; avoid deep hierarchies.
- Reliability: State implemented vs planned behavior clearly; avoid promising APIs not present in code.
- Testability: Provide command snippets for link checks and counters; point to code anchors for verification during runtime (F10/F12).
- Performance: Note hot-path constraints (e.g., tooltip updates only on tile change, WFC time budget usage).
- Future impact: Feature docs improve onboarding and reduce drift; lint ignore prevents CI noise from generated outputs.

## Testing

- Run doc link checker:

```powershell
scripts/check-doc-links.ps1
```

- Run markdown lint locally (target docs only to avoid artifacts):

```bash
npx markdownlint-cli docs/**/*.md
```

- Runtime validation:
- Launch the game, verify F10 opens the settings panel, Apply regenerates visible chunks, Clear Saves deletes saves; F12 toggles overlay; move cursor to see tooltip text update only when tile changes.
