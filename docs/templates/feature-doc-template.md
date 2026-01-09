# <Feature/Component Name>

## Overview

- Brief description of the feature/component and its role in the game.
- State the primary goals and the “why” behind the design.

## Intent & Use-Cases

- Enumerate key use-cases and user flows.
- Call out constraints (runtime limits, memory, platform specifics).

## Architecture & Data Flow

- Describe how this integrates with existing systems (scene lifecycle, services).
- Reference concrete files by path for clarity (e.g., TerrainGeneration2D.Core/Core.cs, TerrainGeneration2D/Scenes/GameScene.cs, TerrainGeneration2D.Core/Graphics/Camera2D.cs, TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs).
- Add a simple flow diagram in text or link to an image in the docs folder if available.

## Domain Terms

- Define domain-specific terms once.
- Keep terminology consistent across docs and code.

## Configuration

- Reference relevant keys in TerrainGeneration2D/appsettings.json.
- Explain defaults and how runtime toggles (F10) influence behavior.

## Algorithms & Math

- Keep equations near algorithm discussion.
- Inline examples: $H = w_c C + w_m M + w_d D$.
- Block examples:

$$
E = -\sum_i p_i \log p_i
$$

## Examples

- Minimal, compile-ready examples with XML docs.
- Include the relevant API and usage snippets.

```csharp
/// <summary>
/// Selects the next cell using Shannon entropy when enabled.
/// </summary>
/// <remarks>
/// Avoid allocations; called in hot path. See WfcTimeBudgetMs.
/// </remarks>
public int SelectNextCell(/* params */)
{
  // example body
  return 0;
}
```

## Performance Notes

- Call out hot paths; avoid LINQ/boxing in per-frame loops.
- Reference diagnostics tools: counters, traces (see Diagnostics README).

## Follow-ups / Decisions

- Open questions, trade-offs, and future work.
- Record decisions that impact behavior or constraints.

## Changelog

- Date and summary of changes as feature evolves.

---

Validation

- Update [docs/README.md](../README.md) with a link to this document.
- Run link validation:

```powershell
scripts/check-doc-links.ps1
```
