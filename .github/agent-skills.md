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
