# Pull Request Checklist (Agent)

Use this checklist to ensure consistency and quality before completion.

- [ ] Document-before-implement: added/updated design or component README (see Per-Task Process).
- [ ] XML docs: public APIs have meaningful summaries/remarks; hot-path expectations noted.
- [ ] Build & Test: ran build-and-test skill.
  - Commands:
    - `dotnet build TerrainGeneration2D.slnx`
    - `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
- [ ] Benchmarks (if performance-sensitive): ran run-benchmarks skill and recorded notes.
  - Command:
    - `dotnet run --project TerrainGeneration2D.Benchmarks/TerrainGeneration2D.Benchmarks.csproj`
- [ ] Diagnostics (WFC/chunking changes): used enable-diagnostics; reviewed counters/events.
- [ ] Docs index: updated [docs/README.md](../docs/README.md) with links to new docs and ran check-doc-links.
  - Command:
    - `scripts/check-doc-links.ps1`
- [ ] Saves impact (terrain generation changes): clarified regeneration expectations; optionally cleared saves via F10.
- [ ] Formatting: respects [.editorconfig](../.editorconfig); minimal diffs; optional `dotnet format`.

If a box cannot be checked, explain why and the mitigation.
