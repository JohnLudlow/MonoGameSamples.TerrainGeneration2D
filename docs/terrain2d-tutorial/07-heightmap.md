# Phase 07 - Noise-based heightmap rules

In this phase you will:

- Generate a coherent heightmap using FastNoiseLite
- Map heights to biomes (ocean, beach, plains, forest, mountain, snow)
- Write tests first (TDD) to validate biome mapping

## 0. Write tests (TDD)

Create `TerrainGeneration2D.Tests/HeightmapTests.cs`:

```csharp
namespace TerrainGeneration2D.Tests;

public class HeightmapTests
{
    [Fact]
    public void DefaultHeightProvider_IsDeterministic()
    {
        var a = new DefaultHeightProvider(12345);
        var b = new DefaultHeightProvider(12345);
        var sa = a.GetSample(100, 200);
        var sb = b.GetSample(100, 200);
        Assert.Equal(sa.Altitude, sb.Altitude);
        Assert.Equal(sa.DetailNoise, sb.DetailNoise);
        Assert.Equal(sa.MountainNoise, sb.MountainNoise);
    }

    [Fact]
    public void BiomeMapping_ProducesAllowedIds()
    {
        int Ocean = 0, Beach = 1, Plains = 2, Forest = 3, Mountain = 4, Snow = 5;
        int[] allowed = { Ocean, Beach, Plains, Forest, Mountain, Snow };
        var heights = new DefaultHeightProvider(123);

        int[,] output = new int[32,32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            var s = heights.GetSample(x,y);
            int tile = s.Altitude <= 0.3f ? Ocean
                : s.Altitude <= 0.35f ? Beach
                : s.Altitude <= 0.7f ? (s.DetailNoise > 0.5f ? Forest : Plains)
                : (s.Altitude > 0.9f && s.MountainNoise > 0.6f) ? Mountain
                : Snow;
            output[x,y] = tile;
        }

        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            Assert.Contains(output[x,y], allowed);
    }
}
```

Run:

```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

Tests should pass after adding the height provider.

## 1. Add FastNoiseLite to Core

```bash
cd TerrainGeneration2D.Core
dotnet add package AlvorEngine.FastNoiseLite --version 1.0.0
```

## 2. Create a height provider (Core)

Create `TerrainGeneration2D.Core/Heightmap.cs`:

```csharp
using AlvorEngine.FastNoiseLite;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public record HeightSample(float Altitude, float DetailNoise, float MountainNoise);

public interface IHeightProvider
{
    HeightSample GetSample(int x, int y);
}

public class DefaultHeightProvider : IHeightProvider
{
    private readonly FastNoiseLite _base;
    private readonly FastNoiseLite _detail;
    private readonly FastNoiseLite _mountain;

    public DefaultHeightProvider(int seed)
    {
        _base = new FastNoiseLite(seed) { Frequency = 0.003f };
        _detail = new FastNoiseLite(seed + 100) { Frequency = 0.02f };
        _mountain = new FastNoiseLite(seed + 200) { Frequency = 0.01f };
    }

    public HeightSample GetSample(int x, int y)
    {
        float altitude = (_base.GetNoise(x, y) + 1f) * 0.5f;   // 0..1
        float detail = (_detail.GetNoise(x, y) + 1f) * 0.5f;
        float mountain = (_mountain.GetNoise(x, y) + 1f) * 0.5f;
        return new HeightSample(altitude, detail, mountain);
    }
}
```

## 3. Map heights to tiles (Game)

In `GameHost.Initialize`, replace random selection with biome rules.

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
IHeightProvider heights = new DefaultHeightProvider(seed: 12345);

int Ocean = 0, Beach = 1, Plains = 2, Forest = 3, Mountain = 4, Snow = 5;

for (int y = 0; y < _mapHeight; y++)
for (int x = 0; x < _mapWidth; x++)
{
    var s = heights.GetSample(x, y);

    if (s.Altitude <= 0.3f) _map[x, y] = Ocean;
    else if (s.Altitude <= 0.35f) _map[x, y] = Beach;
    else if (s.Altitude <= 0.7f) _map[x, y] = (s.DetailNoise > 0.5f ? Forest : Plains);
    else if (s.Altitude > 0.9f && s.MountainNoise > 0.6f) _map[x, y] = Mountain;
    else _map[x, y] = Snow;
}
```

Run the game—you’ll see continents, shorelines, forests, mountains, and snow caps.

## 4. Optional: blend with adjacency filter

After choosing a biome, you can still apply the simple neighbor rule from phase 06 to reduce harsh borders.

## See also

- Previous phase: [06 — Adjacency rules](06-adjacency-rules.md)
- Next phase: [08 — WFC Domains & Entropy](08-wfc-domains.md)
- Tutorial index: [README.md](README.md)
