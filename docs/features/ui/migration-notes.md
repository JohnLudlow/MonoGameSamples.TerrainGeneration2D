# Migration Notes — Code-only Gum UI Removal

Summary
- The legacy code-only Gum UI and its demo assets were removed to make way for a Gum-editor authored UI. This document records the changes, verification steps, and next actions for adding the Gum UI on a separate branch.

What was removed
- Source files deleted from `TerrainGeneration2D/UI`:
  - GameSceneUI.cs
  - AnimatedButton.cs
  - TooltipManager.cs
  - RuntimeSettingsPanel.cs (original)
  - OptionsGrid.cs (original)
  - OptionsSlider.cs (original)
- Temporary shim files added earlier and later removed:
  - `OptionsGrid.cs` (shim) — removed
  - `OptionsSlider.cs` (shim) — removed
  - `RuntimeSettingsPanel.cs` (shim) — removed
- Content assets removed from `TerrainGeneration2D.Content/Assets`:
  - `Assets/fonts/NotArial.fnt`, `04B_30.spritefont`, `04B_30_5x.spritefont` (removed)
  - `Assets/images/logo.png` (removed)

Content builder and build changes
- `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs` was updated to stop including the demo fonts and `logo.png` while preserving required assets:
  - `images/terrain-atlas.png`, `images/atlas-definition.xml`, and `images/terrain-tileset-definition.xml` remain included (required by draw code).
- The content builder was executed to produce output; the run succeeded with all required assets present.

Runtime notes
- The game was manually run locally and confirmed working (UI absent as expected). The rendering depends on `images/terrain-atlas.png` — do not remove it.
- Gum initialization remains in `TerrainGeneration2D/TerrainGenerationGame.cs` and should not be changed; new Gum UI will re-use `GumService.Default`.

Next steps (on a separate branch)
- Add Gum editor assets (`.gumx`, `.guc`) to `TerrainGeneration2D.Content/Assets/UI`.
- Update `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs` to include the new UI folder (e.g., `contentCollection.IncludeCopy<WildcardRule>("UI/**");`).
- Implement Gum-based UI components and remove any leftover references to deleted types.
- Replace other ad-hoc UI helpers (like `OptionsGroupRuleGrid`) with Gum equivalents or keep small runtime helpers inside `docs/features/ui` guidance.

Verification commands
- Build solution and run content builder:
  - `dotnet build TerrainGeneration2D.slnx`
  - `dotnet run --project TerrainGeneration2D.Content/TerrainGeneration2D.Content.csproj -c Debug`
- Run the game locally:
  - `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`

Rollback guidance
- If needed, restore the deleted UI files from the branch that contained them. Until the Gum UI is added, keep small shims (if desired) to preserve any tool that expects the old types.

Related docs
- Implementation plan: [docs/plans/remove-old-code-ui-plan.md](../plans/remove-old-code-ui-plan.md)
- Feature overview: [docs/features/ui/feature-overview.md](feature-overview.md)

Contact
- For follow-ups about placing Gum assets or wiring the runtime settings, ask to: (A) add the Gum `.gumx` files and a minimal loader, or (B) prepare a PR that wires the new UI into `GameScene`.
