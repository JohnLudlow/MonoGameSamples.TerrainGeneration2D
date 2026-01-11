# Phase 11 - WFC Integration (Chunked generation, instrumentation) (TDD)

Goal:

- Use WFC for chunk generation
- Instrument performance and logging

## 0. Tests (TDD)

Create `TerrainGeneration2D.Tests/WfcIntegrationTests.cs`:

```csharp
namespace TerrainGeneration2D.Tests;

public class WfcIntegrationTests
{
    [Fact]
    public void Solve_CompletesWithoutEmptyDomains()
    {
        var rules = new Dictionary<(int tile, Direction dir), HashSet<int>>
        {
            // allow all neighbors by default
        };
        var engine = new WfcEngine(8,8,8);
        var ok = engine.Solve(rules, seed: 123);
        Assert.True(ok);
    }
}
```

## 1. Wire into GameScene (pseudo)

- Replace random fill with WFC:
  - Initialize `WfcEngine` with chunk size
  - Collapse cells until solved
  - Copy result into chunk tiles
- Emit `TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin/End` around solve
- Use `GenLog.GenerateBegin/End` for structured logs

This completes WFC integration.

## See also

- Previous phase: [10 — WFC Backtracking](10-wfc-backtracking.md)
- Earlier phases: [08 — Domains](08-wfc-domains.md), [09 — Propagation](09-wfc-propagation.md)
- Tutorial index: [README.md](README.md)
