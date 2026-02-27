# Update Mapping Features Documentation Plan

## Overview

This plan describes updating and clarifying the mapping feature documentation to reflect the current implementation in TerrainGeneration2D.Core (notably ChunkedTilemap and WFC behavior). The goal is to ensure docs in /docs/features/mapping accurately describe runtime behavior, configuration points, and developer-facing APIs used for generation, saving, and regeneration.

## Definition of Terms

- Chunk: 64×64 tile block managed as a unit of load/unload and persistence.
- WFC: Wave Function Collapse — constraint-based procedural generation used to synthesize coherent tile patterns.
- Backtracking: WFC option that allows the algorithm to undo prior collapses to resolve contradictions.
- Time budget: Upper bound on how long WFC is allowed to run per-chunk (`WfcTimeBudgetMs`).
- Dirty chunk: A chunk that has been modified since last save and should be persisted.

## Requirements

- Docs must reflect that WFC may enable backtracking and that the system falls back to randomized generation on WFC failure.
- Document WfcTimeBudgetMs and how to tune it via runtime settings (F10 panel) to trade quality vs. frame cost.
- Explain RegenerateChunksInView behavior and the overwriteSaves parameter.
- Explain ClearAllSavedChunks file pattern and the effect of deleting saves.
- Update changelogs and add implementation notes where appropriate.

## Implementation Steps

- Edit `docs/features/mapping/chunked-tilemap.md` to include implementation notes (WFC backtracking, fallback generation, WfcTimeBudgetMs, regeneration overwrite behavior, save file pattern). (DONE)
- Review `docs/features/mapping/feature-overview.md` and `docs/features/mapping/map-generation/wfc/README.md` for consistency and add cross-links if missing.
- Run link-check: `scripts/check-doc-links.ps1` to validate internal links.
- Run markdown linting: `npx markdownlint-cli **/*.md` and address any low-effort formatting issues.
- Iterate on any additional clarifications discovered during review (e.g., add examples for RegenerateChunksInView usage).

## Implementation Considerations

- Readability: Keep docs concise and use short bullet lists for runtime behaviors; avoid repeating implementation-level details better suited for code comments.
- Reliability: Verify statements against code (ChunkedTilemap.cs) to avoid drift; prefer quoting exact property/method names.
- Testability: Include steps to validate behavior manually (run game, toggle F10, regenerate view) and via the link-check script.
- Performance: Document trade-offs (time budget vs. generation quality) and recommend conservative defaults for low-end hardware.
- Future changes: Encourage adding new terms to the mapping glossary and updating the changelog.

## Testing

- Run `scripts/check-doc-links.ps1` and confirm zero missing links.
- Run `npx markdownlint-cli **/*.md` and fix any errors flagged in the edited docs.
- Manually verify cross-references from `docs/features/README.md` to mapping docs.

--

Notes: The first edit to `chunked-tilemap.md` was applied as part of this plan to add implementation notes; follow the Implementation Steps to complete the remaining tasks.
