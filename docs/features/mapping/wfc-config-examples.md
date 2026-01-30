# WFC Configuration Examples & Guidance

## Table of contents

- [Examples](#examples)
- [Fallback behavior](#fallback-behavior)
- [Diagnostics & Visual Validation](#diagnostics--visual-validation)
- [Recommendations](#recommendations)

## Definition of terms

| Term | Meaning |
| ---- | ------- |
| Time budget | WfcTimeBudgetMs — ms allowed per-chunk for WFC work |
| Backtracking | WFC undo mechanism to recover from contradictions |

This short guide gives concrete parameter examples and explains trade-offs for WFC in this project.

## Examples

Example 1 — Conservative (fast, lower quality)

```json
{
  "WfcRuntime": { "WfcTimeBudgetMs": 20, "EnableBacktracking": false }
}
```

- Behavior: Fast per-chunk generation, higher chance of contradictions; the system will fall back to randomized generation when WFC fails for a chunk.

Example 2 — Balanced (default)

```json
{
  "WfcRuntime": { "WfcTimeBudgetMs": 50, "EnableBacktracking": true, "MaxBacktrackSteps": 2048 }
}
```

- Behavior: Good balance between visual coherence and CPU cost. Backtracking enabled to resolve many contradictions.

Example 3 — Quality (slower, higher success)

```json
{
  "WfcRuntime": { "WfcTimeBudgetMs": 200, "EnableBacktracking": true, "MaxBacktrackSteps": 8192 }
}
```

- Behavior: Higher success rate and more coherent patches at the cost of longer chunk-generation time. May impact frame times if executed synchronously.

## Fallback behavior

- If WFC fails (contradiction or time/iteration limits), ChunkedTilemap falls back to a deterministic random-based generator (heightmap + rules) for that chunk and marks the chunk dirty so it will be saved.
- Use RegenerateChunksInView to re-run generation after tuning parameters; set overwriteSaves=true to overwrite saved chunk files.

## Diagnostics & Visual Validation

- Use the F12 debug overlay to inspect active chunk boundaries and dirty/clean state (dirty chunks are shown in orange, clean in green).
- Enable diagnostics via performance event sources to trace WFC begin/end and chunk generate outcomes.

## Recommendations

- Start with the balanced example and tune WfcTimeBudgetMs down in steps if CPU is constrained.
- Enable backtracking when iteration limits alone do not produce coherent results.
