# Link Checker Root-Escape Fix

## Overview

The `scripts/check-doc-links.ps1` script incorrectly flags repo-root escapes by counting `../` segments. This heuristic fails for shallow files (too few `../` can still escape) and deep files (many `../` may still be inside the repo). We will document a proper fix using absolute path resolution against the known repo root.

## Definition of Terms

- Root-escape: A relative link that resolves outside the repository directory.
- Repo root: The top-level folder containing the repository (parent of `scripts`).
- Absolute resolution: Converting a relative link to an absolute filesystem path for validation.

## Requirements

- Detect root-escapes by resolving targets to absolute paths and comparing against the repo root.
- Avoid fixed counts of `../`; rely on path comparison instead.
- Preserve existing allowances (external links) and rejections (absolute local paths, root-anchored `/`, unsupported schemes).
- Keep the script fast and robust across Windows paths.

## Implementation Steps

1. Determine repo root:
   - Use `$PSScriptRoot` to find the script’s directory and set `$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path`.
   - Fall back to `(Get-Location).Path` if needed when running from a different working directory.
2. Remove the `../`-count guard:
   - Delete the regex block that matches `^(\.\.\/){N,}` (currently `N=5`).
3. Resolve each relative target to an absolute path:
   - `$abs = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($file.DirectoryName, $pathFs))`.
4. Compare against repo root:
   - If `$abs` starts with `$repoRoot`, perform existence check (`Test-Path`).
   - Else, record `Resolved outside repo` as an error.
5. Keep existing checks for unsupported URI schemes, absolute local paths, and root-anchored paths.
6. Add a small unit of logging for links normalized (optional) to aid debugging.

## Implementation Considerations

- Readability: Centralize root resolution near the top of the script; remove redundant checks.
- Reliability: Use `[System.IO.Path]::GetFullPath` to normalize `..` segments and Windows path separators.
- Testability: Create sample docs with shallow and deep paths to verify both non-escape and escape behavior.
- Performance: Maintain single-pass scanning; avoid excessive filesystem calls beyond `Test-Path`.
- Backwards compatibility: Keep external link handling and existing error categories unchanged.

## Testing

- Positive cases:
  - Deep wiki file linking up multiple levels but staying within repo → passes.
  - Shallow root file linking `../hello` that escapes → fails as `Resolved outside repo`.
  - Valid intra-doc links and fragments → pass when targets exist.
- Negative cases:
  - Absolute local paths like `C:\foo\bar.md` → `Absolute local path`.
  - Root-anchored `/docs/foo.md` → `Root-anchored path`.
  - Unsupported schemes `file://`, `vscode://` → `Unsupported URI scheme`.
- Automation:
  - Run `.\\scripts\\check-doc-links.ps1` and ensure zero false positives/negatives across the docs folder.

## Status

- Temporary mitigation: threshold changed to 5 `../` segments (already applied).
- Planned fix: implement absolute path comparison per steps above.
