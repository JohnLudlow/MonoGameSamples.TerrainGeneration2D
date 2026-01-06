# Phase 09 - WFC Propagation (Arc-consistency) (TDD)

Goal:
- After collapsing a cell, propagate constraints to neighbors
- Maintain a queue of affected cells until no changes remain

## 0. Tests (TDD)
Create `TerrainGeneration2D.Tests/WfcPropagationTests.cs`:
```csharp
namespace TerrainGeneration2D.Tests;

public class WfcPropagationTests
{
    [Fact]
    public void Propagation_RemovesInvalidNeighbors()
    {
        var rules = new Dictionary<(int tile, Direction dir), HashSet<int>>()
        {
            [(1, Direction.North)] = new HashSet<int>{ 2 },
            [(1, Direction.East)] = new HashSet<int>{ 2 },
            [(1, Direction.South)] = new HashSet<int>{ 2 },
            [(1, Direction.West)] = new HashSet<int>{ 2 },
        };
        var engine = new WfcEngine(3,3,3);
        engine.Collapse(1,1, tile: 1);
        engine.Propagate(rules);
        // neighbors of (1,1) should only allow 2
        Assert.All(new[]{(1,0),(2,1),(1,2),(0,1)}, p =>
        {
            var d = engine.GetDomain(p.Item1,p.Item2);
            Assert.Equal(new[]{2}, d.OrderBy(x=>x));
        });
    }
}
```

## 1. Engine propagation methods (Core)
Update `TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs`:
```csharp
// ...existing code...
public void Collapse(int x, int y, int tile)
{
    _domains[x,y]!.Clear();
    _domains[x,y]!.Add(tile);
}

public void Propagate(Dictionary<(int tile, Direction dir), HashSet<int>> rules)
{
    var q = new Queue<(int x,int y)>();
    for (int y = 0; y < _h; y++) for (int x = 0; x < _w; x++) q.Enqueue((x,y));

    while (q.Count > 0)
    {
        var (cx,cy) = q.Dequeue();
        var domain = _domains[cx,cy]!;
        // neighbors
        foreach (var (nx,ny, dir) in Neighbors(cx,cy))
        {
            var nd = _domains[nx,ny]!;
            var allowed = new HashSet<int>();
            foreach (var t in domain)
            {
                if (rules.TryGetValue((t, dir), out var a))
                    foreach (var v in a) allowed.Add(v);
                else
                    for (int v = 0; v < _tileCount; v++) allowed.Add(v);
            }
            bool changed = false;
            foreach (var v in nd.ToArray()) if (!allowed.Contains(v)) { nd.Remove(v); changed = true; }
            if (changed) q.Enqueue((nx,ny));
        }
    }
}

private IEnumerable<(int x,int y, Direction dir)> Neighbors(int x, int y)
{
    if (y > 0) yield return (x,y-1, Direction.North);
    if (x < _w-1) yield return (x+1,y, Direction.East);
    if (y < _h-1) yield return (x,y+1, Direction.South);
    if (x > 0) yield return (x-1,y, Direction.West);
}
```

Proceed to backtracking in the next phase.

## Heuristics: Entropy and Selection
- Entropy: pick the cell with the smallest domain (fewest candidates). For advanced tuning, consider Shannon entropy $H = -\sum p_i \log p_i$ with tile priors.
- Tie-breaking: resolve equal-entropy cells via an injected randomness provider for stability in production, and a deterministic provider in tests.
- Weights: when choosing a tile for a cell, favor neighbors of the same type with a simple boost (e.g., `1 + k * matches`). Keep candidate ordering stable (sort by tile id) to avoid nondeterminism.
- Determinism: use `IRandomProvider` to control randomness in unit tests and sort candidates to eliminate HashSet iteration variability.
- Tuning: start with small multipliers; monitor contradictions/backtracks via diagnostics and adjust. See the deeper discussion in [docs/wfc/wfc-implementation-roadmap.md](docs/wfc/wfc-implementation-roadmap.md) and implementation in [TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs](TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs).

> Try It
>
> - Deterministic tests: implement a test-only `IRandomProvider` that returns fixed values and pass it to `WfcProvider` to make collapse choices predictable. See the example in [TerrainGeneration2D.Tests/MappingTests.cs](TerrainGeneration2D.Tests/MappingTests.cs).
> - Runtime tuning idea: add a config value (e.g., `WfcWeights.NeighborMatchBoost`) in appsettings and thread it into `WfcProvider` to scale the neighbor-match multiplier. Start small (e.g., 1–3) and observe contradictions/backtracks via diagnostics.

## See also
- Previous phase: [08 — WFC Domains & Entropy](docs/terrain2d-tutorial/08-wfc-domains.md)
- Next phase: [10 — WFC Backtracking](docs/terrain2d-tutorial/10-wfc-backtracking.md)
- Tutorial index: [docs/terrain2d-tutorial/README.md](docs/terrain2d-tutorial/README.md)
