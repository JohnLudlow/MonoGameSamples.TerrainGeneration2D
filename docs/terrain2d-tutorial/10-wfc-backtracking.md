# Phase 10 - WFC Backtracking (TDD)

Goal:
- Handle contradictions by backtracking to previous decisions
- Use a decision stack and retry alternate candidates

## 0. Tests (TDD)
Create `TerrainGeneration2D.Tests/WfcBacktrackingTests.cs`:
```csharp
namespace TerrainGeneration2D.Tests;

public class WfcBacktrackingTests
{
    [Fact]
    public void Backtracking_ResolvesContradictions()
    {
        var engine = new WfcEngine(2,2,2);
        // simple rule: tile 0 cannot be north of tile 1
        var rules = new Dictionary<(int tile, Direction dir), HashSet<int>>
        {
            [(1, Direction.South)] = new HashSet<int>{ 1 },
            [(1, Direction.North)] = new HashSet<int>{ 1 },
        };
        var success = engine.Solve(rules, seed: 123);
        Assert.True(success);
    }
}
```

## 1. Engine: Solve with backtracking
Update `TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs`:
```csharp
// ...existing code...
public bool Solve(Dictionary<(int tile, Direction dir), HashSet<int>> rules, int seed)
{
    var rng = new Random(seed);
    var stack = new Stack<(int x,int y, List<int> candidates, int index)>();

    while (true)
    {
        // pick lowest entropy cell
        int bx=-1, by=-1; float best = float.MaxValue;
        for (int y = 0; y < _h; y++) for (int x = 0; x < _w; x++)
        {
            var d = _domains[x,y]!; if (d.Count == 1) continue;
            var e = GetEntropy(x,y);
            if (e < best) { best = e; bx = x; by = y; }
        }
        if (bx == -1) return true; // all collapsed

        var choices = _domains[bx,by]!.OrderBy(_ => rng.Next()).ToList();
        stack.Push((bx,by,choices,0));

        while (stack.Count > 0)
        {
            var (cx,cy,cands, idx) = stack.Pop();
            if (idx >= cands.Count) { if (!RevertLast()) return false; continue; }
            var chosen = cands[idx];
            Collapse(cx,cy, chosen);
            Propagate(rules);

            // check contradiction
            if (_domains[cx,cy]!.Count == 0 || HasEmptyDomain())
            {
                // try next candidate
                stack.Push((cx,cy,cands, idx+1));
                continue;
            }
            else
            {
                // pick next lowest entropy and continue outer loop
                break;
            }
        }
    }
}

private bool HasEmptyDomain()
{
    for (int y=0;y<_h;y++) for (int x=0;x<_w;x++) if (_domains[x,y]!.Count==0) return true;
    return false;
}

private bool RevertLast()
{
    // For simplicity in docs: reinitialize domains; a production impl would snapshot domains.
    var all = Enumerable.Range(0, _tileCount).ToArray();
    for (int y = 0; y < _h; y++) for (int x = 0; x < _w; x++) _domains[x,y] = new HashSet<int>(all);
    return true;
}
```

Proceed to integration in the next phase.

## See also
- Previous phase: [09 — WFC Propagation](docs/terrain2d-tutorial/09-wfc-propagation.md)
- Next phase: [11 — WFC Integration](docs/terrain2d-tutorial/11-wfc-integration.md)
- Tutorial index: [docs/terrain2d-tutorial/README.md](docs/terrain2d-tutorial/README.md)
