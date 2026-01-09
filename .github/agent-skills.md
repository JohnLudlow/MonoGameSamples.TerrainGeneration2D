# Agent Skills

Concise, repeatable sequences for common repo workflows.

## build-and-test

- Purpose: Compile solution and run unit tests.
- Commands:
  - `dotnet build TerrainGeneration2D.slnx`
  - `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
- Inputs: optional `--filter` for tests.
- Outputs: successful build, test results (pass/fail).
- Preconditions: .NET SDK installed; Windows shell; repo restored.
- Verification: exit code 0; no new warnings if feasible; failing tests investigated.
- Rollback: revert recent changes if build/test fails; re-run.

## check-doc-links

- Purpose: Validate internal documentation links.
- Commands:
  - `scripts/check-doc-links.ps1`
- Inputs: changed doc paths.
- Outputs: link validation report.
- Preconditions: Windows PowerShell; docs updated.
- Verification: script reports no invalid links.
- Rollback: fix broken links or remove invalid references, then re-run.

## run-game

- Purpose: Launch the game for interactive validation.
- Commands:
  - `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`
- Inputs: optional config overrides.
- Outputs: running game window.
- Preconditions: assets built; Content present; DEBUG preferred for diagnostics.
- Verification: window opens; F10/F11/F12 toggles work; terrain renders.
- Rollback: stop run; revert recent changes causing runtime errors.

## clean-saves

- Purpose: Force terrain regeneration using current config.
- Preferred: Use the runtime F10 settings panel and select "Clear Saves".
- Manual: Delete the `Content/saves` folder under the app's base directory (created at runtime). Location is relative to `AppDomain.CurrentDomain.BaseDirectory`.
- Inputs: confirmation to clear saves.
- Outputs: saves removed; next run regenerates.
- Preconditions: understand impact on regeneration; backups if needed.
- Verification: `Content/saves` empty; chunks regenerate on next run.
- Rollback: restore saved files from backup if available.

## run-benchmarks

- Purpose: Capture performance baseline after code changes.
- Commands:
  - `dotnet run --project TerrainGeneration2D.Benchmarks/TerrainGeneration2D.Benchmarks.csproj`
- Inputs: scenario selection if supported.
- Outputs: benchmark metrics for key operations.
- Preconditions: bench project builds; stable machine state.
- Verification: results captured; compare against prior baseline.
- Rollback: revert perf-regressing changes; re-measure.

## enable-diagnostics

- Purpose: Observe performance events and counters during generation.
- Notes:
  - DEBUG builds set `EnablePerformanceDiagnostics = true`.
  - Use external tools documented in [TerrainGeneration2D.Core/Diagnostics/README.md](../TerrainGeneration2D.Core/Diagnostics/README.md).
- Example commands:
  - Install counters: `dotnet tool install --global dotnet-counters`
  - Monitor: `dotnet-counters monitor --process-id <PID> JohnLudlow.TerrainGeneration2D.Performance`
  - Install trace: `dotnet tool install --global dotnet-trace`
  - Trace: `dotnet-trace collect --process-id <PID> --providers JohnLudlow.TerrainGeneration2D.Performance`
- Inputs: process ID of running game.
- Outputs: live counters or trace files.
- Preconditions: tools installed; DEBUG or diagnostics enabled.
- Verification: events/counters visible; provider name matches.
- Rollback: stop monitoring; disable listener if needed.

## elaborate-feature-doc

- Purpose: Produce a feature/component documentation that adheres to repo standards.
- Steps:
  - Create or update a doc under [docs](../docs) or add a component README alongside code.
  - Include required elements: overview, intent/use-cases, architecture/data flow references, domain terms, constraints, example snippets, KaTeX math where relevant.
  - Reference concrete files and config (e.g., [TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs](../TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs), [TerrainGeneration2D/appsettings.json](../TerrainGeneration2D/appsettings.json)).
  - Link from [docs/README.md](../docs/README.md) and validate links.
- Commands:
  - `scripts/check-doc-links.ps1`
- Inputs: feature name, affected components, config keys.
- Outputs: new/updated doc path; docs index entry.
- Preconditions: gather architecture/context; decide doc location.
- Verification: link check passes; content follows principles; examples compile.
- Rollback: revert doc if requirements unmet; fix and re-run.
- Naming: place feature docs at `docs/features/<feature-name>.md`; use kebab-case (e.g., `chunked-tilemap.md`), no spaces, concise names.

## implement-refactor-process

- Purpose: Execute the docs-first implementation/refactor flow with tests and performance hygiene.
- Steps:
  - Document-before-implement: add/update short design or component README.
  - Implement with XML docs for public APIs; respect `.editorconfig`; avoid hot-path allocations/LINQ.
  - Build and test; add/update unit tests in [TerrainGeneration2D.Tests](../TerrainGeneration2D.Tests).
  - Run benchmarks when performance-sensitive and note results.
  - Update docs and cross-links; validate with link checker.
- Commands:
  - `dotnet build TerrainGeneration2D.slnx`
  - `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
  - `dotnet run --project TerrainGeneration2D.Benchmarks/TerrainGeneration2D.Benchmarks.csproj`
  - `scripts/check-doc-links.ps1`
- Inputs: change summary, target files/APIs.
- Outputs: code changes with XML docs; tests; updated docs.
- Preconditions: confirm constraints; plan small, reversible steps.
- Verification: build/test pass; benchmarks acceptable; docs updated and links valid.
- Rollback: revert changes if failing gates; iterate with smaller scope.

## regenerate-visible-chunks

- Purpose: Regenerate chunks currently within the camera’s expanded viewport using current settings.
- Steps:
  - Run the game; press F10 to open settings panel.
  - Use "Regenerate Visible" to refresh chunks in view.
  - Optionally toggle F12 to inspect dirty/clean chunk borders.
- Inputs: none.
- Outputs: refreshed chunk data; optional saves overwritten if enabled.
- Preconditions: game running; camera positioned; Content present.
- Verification: terrain changes reflect current heuristics/config; overlay shows updated states.
- Rollback: restore prior saves if necessary.

## update-docs-index

- Purpose: Add new documentation links to the main docs index and validate.
- Steps:
  - Edit [docs/README.md](../docs/README.md) to include the new doc link.
  - Keep section structure and link style consistent.
  - Run link checker.
- Commands:
  - `scripts/check-doc-links.ps1`
- Inputs: doc path and title.
- Outputs: updated docs index entry.
- Preconditions: doc exists and is committed.
- Verification: link check passes; index renders correctly.
- Rollback: remove or correct the entry if invalid.

## add-xml-docs

- Purpose: Ensure public APIs have meaningful XML documentation.
- Steps:
  - Identify public classes/members lacking summaries/remarks.
  - Add concise, purposeful XML docs; include performance notes for hot paths.
  - Build to confirm no XML doc-related warnings.
- Inputs: target files/namespaces.
- Outputs: updated source files with XML docs.
- Preconditions: follow [.editorconfig](../.editorconfig) and repo conventions.
- Verification: build passes; analyzers show reduced documentation warnings.
- Rollback: revert over-verbose or incorrect docs; refine.

## format-code (optional)

- Purpose: Apply code style consistent with repo settings.
- Steps:
  - Use IDE formatting aligned with [.editorconfig](../.editorconfig).
  - Optionally run `dotnet format` if installed and approved.
- Commands (optional):
  - `dotnet format`
- Inputs: changed files.
- Outputs: consistently formatted code.
- Preconditions: avoid broad, noisy diffs; focus on touched files.
- Verification: minimal diffs; style matches existing code.
- Rollback: revert unintended formatting changes.
