# Phase 08 - WFC Domains & Entropy (TDD)

Goal:

- Maintain a set of candidate tiles per cell (domain)
- Compute entropy and choose the next cell to collapse

## 0. Tests (TDD)

Create `TerrainGeneration2D.Tests/WfcDomainTests.cs`:

```csharp
namespace TerrainGeneration2D.Tests;

public class WfcDomainTests
{
    [Fact]
    public void Domains_InitializeWithAllTiles()
    {
        var wfc = new WfcEngine(width: 8, height: 8, tileCount: 16);
        var d = wfc.GetDomain(0,0);
        Assert.Equal(16, d.Count);
    }

    [Fact]
    public void Entropy_ReflectsCandidateCount()
    {
        var wfc = new WfcEngine(8,8,16);
        wfc.Prune(0,0, t => t % 2 == 0); // keep evens
        Assert.True(wfc.GetEntropy(0,0) < wfc.GetEntropy(0,1));
    }
}
```

## 1. Engine skeleton (Core)

Create `TerrainGeneration2D.Core/WfcEngine.cs` (conceptual; this repo uses `Mapping/WaveFunctionCollapse.cs`):

```csharp
namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

public sealed class WfcEngine
{
    private readonly int _w, _h, _tileCount;
    private readonly HashSet<int>?[,] _domains;

    public WfcEngine(int width, int height, int tileCount)
    {
        _w = width; _h = height; _tileCount = tileCount;
        _domains = new HashSet<int>?[width, height];
        var all = Enumerable.Range(0, tileCount).ToArray();
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            _domains[x,y] = new HashSet<int>(all);
    }

    public HashSet<int> GetDomain(int x, int y) => _domains[x,y]!;

    public void Prune(int x, int y, Func<int,bool> predicate)
    {
        var d = _domains[x,y]!;
        foreach (var v in d.ToArray()) if (!predicate(v)) d.Remove(v);
    }

    public float GetEntropy(int x, int y)
    {
        int c = _domains[x,y]!.Count;
        return c == 0 ? float.MaxValue : (float)Math.Log(c);
    }
}
```

Proceed to propagation in the next phase.

## See also

- Previous phase: [07 — Heightmap rules](07-heightmap.md)
- Next phase: [09 — WFC Propagation](09-wfc-propagation.md)
- Tutorial index: [README.md](README.md)
