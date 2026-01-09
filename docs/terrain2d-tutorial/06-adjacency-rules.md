# Phase 06 - Apply adjacency / nearness rules

In this phase you will:
- Introduce simple neighbor constraints
- Ensure certain tiles don’t appear adjacent
- Write tests first (TDD) to drive rule implementation

## 0) Write tests (TDD)
Create `TerrainGeneration2D.Tests/AdjacencyRulesTests.cs`:
```csharp
namespace TerrainGeneration2D.Tests;

public class AdjacencyRulesTests
{
    [Fact]
    public void NotAdjacentRule_FiltersForbiddenNeighbor()
    {
        var rule = new NotAdjacentRule(forbiddenTileId: 5); // e.g., Snow
        Assert.True(rule.Allows(tileId: 1, neighborTileId: 4, Direction.North));
        Assert.False(rule.Allows(tileId: 1, neighborTileId: 5, Direction.North));
    }

    [Fact]
    public void DirectionalBlockRule_BlocksSpecificDirection()
    {
        var rule = new DirectionalBlockRule(tile: 2, blockedNeighbor: 6, blockedDirection: Direction.North); // Beach vs Mountain
        Assert.False(rule.Allows(tileId: 2, neighborTileId: 6, Direction.North));
        Assert.True(rule.Allows(tileId: 2, neighborTileId: 6, Direction.South));
    }

    [Fact]
    public void ProximityRule_BlocksMountainNearOcean()
    {
        int[,] map = TestHelpers.CreateMap(16, 16, fill: 3); // plains
        map[8,8] = 1; // ocean
        bool blocked = ProximityRules.IsMountainTooCloseToOcean(map, x: 10, y: 8, candidateTileId: 6, mountainId: 6, oceanId: 1, radius: 5);
        Assert.True(blocked);
    }
}
```
Run:
```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```
Tests should fail until you add the rules below.

## 1) Add rule interfaces (Core)
Create the file in your Core project:
```bash
# from repository root
mkdir -p TerrainGeneration2D.Core
# create the interfaces and rules file
echo > TerrainGeneration2D.Core/Rules.cs
```
Then edit `TerrainGeneration2D.Core/Rules.cs` and paste:

```csharp
namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public enum Direction { North, South, East, West }

// Interface defining a hard constraint evaluated against a single neighbor
public interface ITileRule
{
    bool Allows(int tileId, int neighborTileId, Direction neighborDirection);
}

// Example 1: Never place a tile next to a specific tile ID
public class NotAdjacentRule : ITileRule
{
    private readonly int _forbidden;
    public NotAdjacentRule(int forbiddenTileId) => _forbidden = forbiddenTileId;
    public bool Allows(int tileId, int neighborTileId, Direction dir) => neighborTileId != _forbidden;
}

// Example 2: Only allow certain pairs (e.g., beach must border ocean or plains)
public class AllowedNeighborPairsRule : ITileRule
{
    private readonly HashSet<(int tileId, int neighborId, Direction dir)> _allowed;
    public AllowedNeighborPairsRule(IEnumerable<(int tileId, int neighborId, Direction dir)> allowed)
    {
        _allowed = new(allowed);
    }
    public bool Allows(int tileId, int neighborTileId, Direction dir) =>
        _allowed.Contains((tileId, neighborTileId, dir));
}

// Example 3: Direction-specific restriction (e.g., mountains should not be south of beach)
public class DirectionalBlockRule : ITileRule
{
    private readonly int _tile;
    private readonly int _blockedNeighbor;
    private readonly Direction _blockedDirection;
    public DirectionalBlockRule(int tile, int blockedNeighbor, Direction blockedDirection)
    {
        _tile = tile; _blockedNeighbor = blockedNeighbor; _blockedDirection = blockedDirection;
    }
    public bool Allows(int tileId, int neighborTileId, Direction dir)
    {
        if (tileId == _tile && neighborTileId == _blockedNeighbor && dir == _blockedDirection)
            return false;
        return true;
    }
}

// Optional: soft preference via weights instead of hard allow/deny
public interface IWeightedTileRule
{
    // Return multiplier for a candidate based on a neighbor (>=0)
    float Weight(int candidateTileId, int neighborTileId, Direction neighborDirection);
}

public class PreferSameTileRule : IWeightedTileRule
{
    private readonly float _bonus;
    public PreferSameTileRule(float bonus = 2f) => _bonus = bonus;
    public float Weight(int candidate, int neighbor, Direction dir) => candidate == neighbor ? _bonus : 1f;
}

// Example 4: Prohibit mountains within N tiles of ocean (radius check)
public static class ProximityRules
{
    public static bool IsMountainTooCloseToOcean(int[,] map, int x, int y, int candidateTileId, int mountainId, int oceanId, int radius)
    {
        if (candidateTileId != mountainId) return false; // only applies to mountains

        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int minX = Math.Max(0, x - radius);
        int maxX = Math.Min(width - 1, x + radius);
        int minY = Math.Max(0, y - radius);
        int maxY = Math.Min(height - 1, y + radius);

        for (int yy = minY; yy <= maxY; yy++)
        {
            for (int xx = minX; xx <= maxX; xx++)
            {
                // Manhattan distance (alternative: Euclidean)
                int dist = Math.Abs(xx - x) + Math.Abs(yy - y);
                if (dist <= radius && map[xx, yy] == oceanId)
                    return true; // found nearby ocean
            }
        }
        return false;
    }
}
```

If you haven’t already, ensure the Game project references the Core project:
```bash
cd src
dotnet add TerrainGeneration2D/TerrainGeneration2D.csproj reference TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
```

## 2) Constrain random selection with rules
Update map generation in `GameHost.Initialize` to choose a tile that doesn’t violate rules of already-placed neighbors.

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
// Example tile IDs
int Ocean = 0, Beach = 1, Plains = 2, Forest = 3, Mountain = 4, Snow = 5;

var hardRules = new List<ITileRule>
{
    // Beach can’t be directly north of Mountain
    new DirectionalBlockRule(Beach, Mountain, Direction.North),

    // Forest tiles must not touch Snow (avoid harsh edges)
    new NotAdjacentRule(Snow),

    // Explicit allowed pairs (optional, more strict)
    new AllowedNeighborPairsRule(new[]
    {
        (Beach, Ocean, Direction.West),
        (Beach, Ocean, Direction.East),
        (Beach, Plains, Direction.North),
        (Beach, Plains, Direction.South)
    })
};

var softRules = new List<IWeightedTileRule>
{
    // Prefer repeating tiles to form larger coherent areas
    new PreferSameTileRule(1.5f)
};

for (int y = 0; y < _mapHeight; y++)
for (int x = 0; x < _mapWidth; x++)
{
    // Build candidate list (all tiles in atlas)
    List<int> candidates = Enumerable.Range(0, tileCount).ToList();

    // Filter by neighbors (only those already assigned)
    if (x > 0) candidates = candidates.Where(t => hardRules.All(r => r.Allows(t, _map[x-1,y], Direction.West))).ToList();
    if (y > 0) candidates = candidates.Where(t => hardRules.All(r => r.Allows(t, _map[x,y-1], Direction.North))).ToList();

    if (candidates.Count == 0)
    {
        _map[x, y] = Plains; // fallback
        continue;
    }

    // Apply soft weighting
    float[] weights = new float[candidates.Count];
    for (int i = 0; i < candidates.Count; i++)
    {
        float w = 1f;
        if (x > 0) foreach (var sr in softRules) w *= sr.Weight(candidates[i], _map[x-1,y], Direction.West);
        if (y > 0) foreach (var sr in softRules) w *= sr.Weight(candidates[i], _map[x,y-1], Direction.North);
        weights[i] = MathF.Max(0.0001f, w);
    }

    // Prohibit mountains within 5 tiles of ocean (radius proximity rule)
    for (int i = weights.Length - 1; i >= 0; i--)
    {
        int candidate = candidates[i];
        if (ProximityRules.IsMountainTooCloseToOcean(_map, x, y, candidate, Mountain, Ocean, radius: 5))
        {
            candidates.RemoveAt(i);
            weights = weights.Where((_, idx) => idx != i).ToArray();
        }
    }

    if (candidates.Count == 0)
    {
        _map[x, y] = Plains; // fallback after proximity filter
        continue;
    }

    // Weighted random pick
    float total = weights.Sum();
    float roll = (float)rng.NextDouble() * total;
    int chosen = candidates[0];
    float acc = 0f;
    for (int i = 0; i < candidates.Count; i++)
    {
        acc += weights[i];
        if (roll <= acc) { chosen = candidates[i]; break; }
    }

    _map[x, y] = chosen;
}
```

Now forbidden tiles won’t appear next to each other horizontally or vertically, preferred tiles will tend to form larger patches, and mountains will not be placed within 5 tiles of any ocean.

## 3) Example setups (with implementations)

### Water/land interface
- Forbid Mountain adjacent to Ocean; allow Beach between them:
```csharp
hardRules.Add(new NotAdjacentRule(Ocean));        // avoid ocean neighbors for most land
hardRules.Add(new DirectionalBlockRule(Mountain, Ocean, Direction.West));
hardRules.Add(new DirectionalBlockRule(Mountain, Ocean, Direction.East));
// ensure beach bridges ocean/plains in allowed pairs rule listed above
```

### Climate bands
- Forbid Snow next to Forest; prefer Plains next to Forest:
```csharp
hardRules.Add(new NotAdjacentRule(Snow)); // already present for forest harsh edges
softRules.Add(new PreferSameTileRule(1.2f)); // helps form bands
```

### Cliff logic
- Block Beach north of Mountain (directional):
```csharp
hardRules.Add(new DirectionalBlockRule(Beach, Mountain, Direction.North));
```

### Mountains away from coasts
- Prohibit mountains within 5 tiles of ocean (radius proximity):
```csharp
// already enforced in the generation loop via ProximityRules.IsMountainTooCloseToOcean(...)
```

## 4) Visual check
Run the game and look for edge consistency. Tweak tile IDs, rule parameters, and radius until the generated map looks coherent.

## Notes
- This is not full WFC; it’s a greedy neighbor filter (fast, simple). You can combine hard rules with soft weights for decent results.
- Proximity checks run against already placed tiles; if you implement multi-pass generation, you can tighten or relax constraints in later passes.
- In the next phase we’ll introduce heightmaps to drive tile selection and further constrain options.

## See also
- Previous phase: [05 — Random tiles](05-random-tiles.md)
- Next phase: [07 — Heightmap rules](07-heightmap.md)
- Tutorial index: [README.md](README.md)
