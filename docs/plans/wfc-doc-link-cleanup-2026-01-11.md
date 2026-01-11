# WFC Doc Link Cleanup Plan

## Overview

Refine the Wave Function Collapse (WFC) documentation after relocating it under the Mapping functional area. Remove or correct deep code links, fix configuration file paths, and ensure navigation works without escaping the repo root.

## Definition of Terms

- WFC: Wave Function Collapse, a constraint-based map generation algorithm.
- Entropy: A score used to rank cells for observation; domain size or Shannon entropy.
- Backtracking: Strategy to recover from contradictions by rolling back to a prior decision.
- AppSettings: Project runtime configuration file (`TerrainGeneration2D/TerrainGeneration2D/appsettings.json`).

## Requirements

- All links in `docs/features/mapping/map-generation/wfc/*` resolve within the repo.
- References to `TerrainGeneration2D.Core/*` use correct relative depth from WFC pages.
- All references to `appsettings.json` point to `TerrainGeneration2D/TerrainGeneration2D/appsettings.json` with correct relative paths per doc location.
- Navigation sections present and accurate in WFC pages.

## Implementation Steps

1. Sweep WFC pages for deep code links and adjust relative depth:
   - Replace `../../../TerrainGeneration2D.Core/...` with `../../../../TerrainGeneration2D.Core/...`.
   - Replace `../../../TerrainGeneration2D.Tests/...` with `../../../../TerrainGeneration2D.Tests/...`.
2. Replace code file references on conceptual pages with local overview references where appropriate (e.g., Propagation page).
3. Fix configuration links to `TerrainGeneration2D/TerrainGeneration2D/appsettings.json` in:
   - `docs/features/mapping/*`
   - `docs/features/ui/*`
   - `docs/terrain2d-tutorial/*`
   - `docs/plans/*`
4. Validate navigation sections: Up/Previous/Next links in WFC pages.
5. Run link checker and markdown lint on `docs/**` and iterate on any remaining issues.

## Implementation Considerations

- Readability: Prefer local overview references over deep code links on conceptual docs.
- Reliability: Ensure all relative paths are correct from each file’s location; avoid repo-root escapes.
- Testability: Use the link checker script to verify targets; keep changes minimal and focused.
- Performance: N/A for docs; avoid large diffs and unnecessary reformatting.
- Future Impact: A stable documentation structure simplifies future cross-references and onboarding.

## Testing

- Link Check: Run `scripts/check-doc-links.ps1` and confirm zero errors for `docs/**`.
- Lint: Run markdown lint (respect `.markdownlintignore`) and confirm pass.
- Spot check: Open WFC pages and confirm code references resolve and navigation works.
