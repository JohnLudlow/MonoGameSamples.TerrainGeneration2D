# Runtime UI & Operations — Quick How-To

## Table of contents

- [F10 - Runtime Settings Panel](#f10---runtime-settings-panel)
- [Regenerate visible chunks](#regenerate-visible-chunks)
- [Clear all saved chunks](#clear-all-saved-chunks)
- [F12 - Debug Overlay](#f12---debug-overlay)
- [Saving behavior](#saving-behavior)
- [Best practices](#best-practices)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Visible regeneration | Regenerating only chunks within the camera's expanded viewport buffer |
| Overwrite saves | Whether RegenerateChunksInView writes over existing saved chunk files |

This page documents common runtime operations exposed via keyboard shortcuts and the F10 runtime settings panel.

### F10 - Runtime Settings Panel

- Opens the settings overlay enabling live changes to:
  - Heuristics (tie-breakers, selection strategy)
  - WFC runtime (WfcTimeBudgetMs, EnableBacktracking)
  - WFC weights and terrain rules
  - Buttons to Regenerate visible chunks and Clear all saved chunks

- Typical workflow:
  1) Open F10.
  2) Adjust WfcTimeBudgetMs or EnableBacktracking.
  3) Click "Regenerate Visible Chunks" to apply changes and optionally overwrite saves.

### Regenerate visible chunks

- Calls ChunkedTilemap.RegenerateChunksInView(viewportWorldBounds, overwriteSaves=true).
- Use to preview rule/heuristic changes without restarting the game.
- Note: regenerating with overwriteSaves=true will replace saved chunk files; keep backups if you need reproducibility.

### Clear all saved chunks

- Calls ChunkedTilemap.ClearAllSavedChunks which deletes files matching `map_*_*.dat` in the configured save directory.
- Use when changing global rules or when you want the world to be re-generated from the current settings on next load.

### F12 - Debug Overlay

- Toggles a debug overlay that visualizes active chunk boundaries and their dirty state:
  - Orange: dirty (unsaved/modified)
  - Green: clean
- Use it to verify culling, buffer sizes, and to spot unexpected churn.

### Saving behavior

- Chunks generated or modified at runtime are marked dirty and saved to disk when unloaded or when SaveAll() is invoked.
- Saved files are gzipped binary files with the magic header 'CHNK', version number and the chunk contents.

### Best practices

- After making big rule changes, ClearAllSavedChunks and then run the game to generate chunks with the new rules, or use Regenerate with overwrite.
- Use the WfcTimeBudgetMs setting to control CPU impact; prefer incremental changes and validation via the debug overlay.
