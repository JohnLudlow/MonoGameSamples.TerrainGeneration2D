# Include GumX Content Pipeline

## Table of contents

- [Overview](#overview)
- [Definition of Terms](#definition-of-terms)
- [Requirements](#requirements)
- [Implementation Steps](#implementation-steps)
- [Implementation Considerations](#implementation-considerations)
- [Testing](#testing)
- [Follow-ups / Decisions](#follow-ups--decisions)

## Plan issue

- (link to related GitHub issue(s) or attach issue numbers here)

## Plan status

- In development

## Overview

This plan describes how to integrate the new Gum `.gumx` UI project into the project's runtime Content pipeline so the exported runtime assets (atlas XML + images + screens) are available at runtime. It also documents the minimal code change required to make `GameScene` tolerate the Gum content loader not being available and fall back to the standard MonoGame `ContentManager`.

## Definition of Terms

- Gum: The UI editor used to author `.gumx` files and export runtime UI assets.
- .gumx: Gum project's source file used by the Gum editor (design-time).
- Atlas: The packed texture atlas produced by Gum containing UI element textures and an accompanying `atlas-definition.xml` file.
- TextureAtlas: The runtime helper used by the game to load the Gum-produced atlas (via `TextureAtlas.FromFile`).
- Content Builder / Content Pipeline: MonoGame content build step that converts source assets into runtime-ready assets and copies them to the game's `Content` output.

## Requirements

- The Gum editor or a Gum CLI/export step must produce the runtime files (`images/atlas-definition.xml` and the referenced atlas texture(s)) from the `.gumx` project.
- The generated runtime files must be included in the `TerrainGeneration2D.Content` output so they are accessible via `ContentManager` at runtime.
- The game must gracefully handle environments where `GumService.Default.ContentLoader.XnaContentManager` is not available (e.g., when Gum runtime content is not hooked into the Gum content loader), by falling back to the scene `Content` manager.

## Implementation Steps

1. Export Gum runtime files
   - Open the Gum project and export/generate runtime assets for `TerrainGenerationOptionsScreen.gumx` so the `images/atlas-definition.xml` and its textures are produced.

2. Add generated Gum runtime files to the MonoGame Content project
   - In `TerrainGeneration2D.Content` (the project's content builder), add the generated `images/atlas-definition.xml` and referenced atlas texture(s) as content items so the MonoGame Content Builder will produce runtime assets in the `Content` output folder.
   - Follow the MonoGame Content Builder guidance: [MonoGame Content Builder guide](https://docs.monogame.net/articles/getting_started/content_pipeline/content_builder_project.html?tabs=vscode)

3. Update `GameScene` to tolerate missing Gum XNA manager (minimal code change)
   - Replace the strict null-check that throws when `GumService.Default?.ContentLoader?.XnaContentManager` is null with a fallback to the scene `Content` manager. Example:

  ```csharp
  var contentManager = GumService.Default?.ContentLoader?.XnaContentManager ?? Content;
  var atlas = TextureAtlas.FromFile(contentManager, "images/atlas-definition.xml");
  ```

  - This keeps existing behavior when Gum's content manager exists but allows the game to load the atlas from the normal MonoGame `Content` when it does not.

4. Wire UI screen (Gum)

- The codebase now includes a Gum-generated screen partial class `TerrainGenerationOptionsScreen` (see `Screens/TerrainGenerationOptionsScreen.cs`). Ensure the Gum runtime export for this screen exists and is built into the Content output.
- At runtime, instantiate or initialize the generated `TerrainGenerationOptionsScreen` (or load it via `GumService`) and add it to the Gum root so the screen is available to the game. If the screen requires a `TextureAtlas`, load it via the content manager fallback described above.
- Remove usages of the old `RuntimeSettingsPanel` if they remain, and replace them with calls that show/hide or bind to `TerrainGenerationOptionsScreen` as appropriate.

5. Verify build and runtime
   - Build the content, run the game, and validate that the settings UI is visible and functional.

## Implementation Considerations

- Readability: Keep the `GameScene` change minimal and well-commented to make the fallback intention clear for future maintainers.
- Reliability: The fallback avoids runtime throws in setups where Gum's content loader isn't registered; this makes the game more robust across different dev environments or CI.
- Testability: Keep the atlas loading logic isolated so unit/integration tests can supply a mock `ContentManager` or a test atlas file.
- Performance: No runtime performance impact beyond standard texture atlas loading. Ensure large atlas textures are built with appropriate compression/settings in the content pipeline.
- Future features: If you later rely on Gum's runtime features (behaviors/screens), prefer wiring the Gum content loader and registering the Gum-generated files with the Gum runtime, rather than relying on the atlas-only fallback.

## Testing

1. Content build verification
   - Run the MonoGame Content Builder to produce runtime assets and confirm `images/atlas-definition.xml` and atlas textures are present in the `Content` output.

2. Runtime verification
   - Run the game and verify the `TerrainGenerationOptionsScreen` appears, is populated with current settings, and can update them.
   - Toggle the settings panel key (F10) and confirm visibility toggles and controls operate.

3. Fallback path verification
   - Intentionally disable or bypass Gum's XNA content manager (if possible) and verify `GameScene` loads the atlas from the scene `Content` manager without throwing.

4. Link checks
   - Run `scripts/check-doc-links.ps1` to ensure any documentation links added/changed are valid.

5. Markdown lint
   - Run `npx markdownlint-cli **/*.md` to validate formatting of the new plan file.

## Follow-ups / Decisions

- If the team prefers the Gum content loader to be the primary source, add a CI/content build step that runs Gum export and includes the outputs in `TerrainGeneration2D.Content` so the Gum loader and `ContentManager` point to the same files.

- For instructions and sample code to bind runtime config objects to the Gum screen, see docs/plans/bind-gum-screen-to-config-plan.md.
