using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon.Core.Mapping;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UnitTests.Core.Mapping;

public sealed class HeuristicsSelectionTests
{
  private sealed class DeterministicRandomProvider : IRandomProvider
  {
    public int NextInt() => 0;
    public int NextInt(int maxValue) => 0;
    public int NextInt(int minValue, int maxValue) => minValue;
    public double NextDouble() => 0.0;
  }

  [Fact]
  public void SingleHeuristic_InfluenceTieBreak_PicksMostConstrainingCell()
  {
    // TEST SCENARIO: Demonstrate that when multiple cells have the same entropy (domain size),
    // the WFC algorithm applies the "most constraining variable" heuristic to break ties
    // by preferring cells that can influence the most undecided neighbors.
    //
    // SETUP: Create a 3x3 grid with specific domain states:
    //
    //    ╔═══════╦═══════╦═══════╗
    //    ║ [1,2] ║  [1]  ║decided║   ← Row y=0: (0,0), (1,0), (2,0)
    //    ║   ●   ║   ●   ║       ║
    //    ╠═══════╬═══════╬═══════╣
    //    ║ [1]   ║[1,2]  ║ [1]   ║   ← Row y=1: (0,1), (1,1), (2,1)
    //    ║   ●   ║  ●●   ║   ●   ║
    //    ╠═══════╬═══════╬═══════╣
    //    ║decided║ [1]   ║decided║   ← Row y=2: (0,2), (1,2), (2,2)
    //    ║       ║   ●   ║       ║
    //    ╚═══════╩═══════╩═══════╝
    //    Col x:    0       1       2   [●] = undecided, [number] = domain
    //
    // KEY OBSERVATION: Both (0,0) and (1,1) have domain size 2 (lowest entropy among undecided cells).
    // However, (1,1) has 4 undecided neighbors: (1,0), (1,2), (0,1), (2,1)
    // While (0,0) has only 2 undecided neighbors: (1,0), (0,1)
    //
    // EXPECTED BEHAVIOR: With influence tie-break enabled, FindLowestEntropy should select
    // the cell with maximum influence: (1,1), because solving it first will constrain more
    // neighbors and reduce the search space more efficiently.
    //
    // HEURISTICS ENABLED:
    // - UseDomainEntropy=true: Consider domain size (|D|)
    // - ApplyInfluenceTieBreakForSingleHeuristic=true: Apply influence filter when single heuristic is used
    // - UseMostConstrainingTieBreak=true: Enable tie-breaking logic
    // - PreferCentralCellTieBreak=false: Don't apply spatial centering (would add second tie-breaker)

    var registry = TileTypeRegistry.CreateDefault(5);
    var heuristics = new HeuristicsConfiguration
    {
      UseDomainEntropy = true,
      UseShannonEntropy = false,
      UseMostConstrainingTieBreak = true,
      ApplyInfluenceTieBreakForSingleHeuristic = true,
      PreferCentralCellTieBreak = false
    };
    var wfc = new WfcProvider(
      3,
      3,
      registry,
      new DeterministicRandomProvider(),
      new TileTypeRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);

    var poss = MappingTestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = MappingTestHelpers.GetPrivateField<int[][]>(wfc, "_output");
    Assert.NotNull(poss);
    Assert.NotNull(output);

    // Initialize all cells as decided (null domain = collapsed state)
    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }

    // Set up domain constraints as described above
    poss![0][0] = [1, 2];      // Top-left: entropy 2
    poss[1][1] = [1, 2];       // Center: entropy 2 (same as top-left)
    poss[1][0] = [1];          // Top-center: entropy 1, influences (1,1) below
    poss[1][2] = [1];          // Bottom-center: entropy 1, influences (1,1) above
    poss[0][1] = [1];          // Middle-left: entropy 1, influences (1,1) to right
    poss[2][1] = [1];          // Middle-right: entropy 1, influences (1,1) to left

    var result = (ValueTuple<int, int>)MappingTestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;

    // ASSERTION: Should select (1,1) because:
    // 1. Both (0,0) and (1,1) have domain size 2 (equal entropy)
    // 2. (1,1) has influence=4 (touches 4 undecided neighbors)
    // 3. (0,0) has influence=2 (touches 2 undecided neighbors)
    // 4. Influence tie-break prefers maximum influence → select (1,1)
    Assert.Equal((1, 1), result);
  }

  [Fact]
  public void CentralTieBreak_PrefersCenter_WhenEntropyAndInfluenceTied()
  {
    // TEST SCENARIO: Verify that when entropy AND influence are tied across multiple cells,
    // the WFC algorithm applies a secondary tie-breaker: preferring cells closer to the
    // map center. This promotes more stable generation by starting from the center outward.
    //
    // SETUP: Create a 3x3 grid with symmetric domains at three corners and center:
    //
    //    ╔═══════╦═══════╦═══════╗
    //   (0,0): [1,2] (1,0): DECIDED (2,0): DECIDED
    //    ║  ●●   ║       ║       ║
    //    ╠═══════╬═══════╬═══════╣
    //   (0,1): DECIDED (1,1): [1,2]  (2,1): DECIDED
    //    ║       ║  ●●   ║       ║  ← CENTER of 3×3 grid is at (1,1)
    //    ╠═══════╬═══════╬═══════╣
    //   (0,2): DECIDED (1,2): DECIDED (2,2): [1,2]
    //    ║       ║       ║  ●●   ║
    //    ╚═══════╩═══════╩═══════╝
    //
    // KEY OBSERVATION: All three cells with domain [1,2] have:
    // - Same domain size (entropy = 2)
    // - Same influence score (each has 0 undecided neighbors, as corners are isolated)
    // - Different distances from center (1,1):
    //   • (0,0): distance = |0-1| + |0-1| = 2
    //   • (1,1): distance = |1-1| + |1-1| = 0 ← CLOSEST TO CENTER
    //   • (2,2): distance = |2-1| + |2-1| = 2
    //
    // EXPECTED BEHAVIOR: With central tie-break enabled, FindLowestEntropy should select
    // (1,1) because it's at the exact center of the grid and will provide stable generation.
    //
    // HEURISTICS ENABLED:
    // - PreferCentralCellTieBreak=true: Apply spatial centering when entropy/influence tied
    // - UseMostConstrainingTieBreak=true: First try influence filter
    // - ApplyInfluenceTieBreakForSingleHeuristic=true: Apply even with single heuristic
    // - UseDomainEntropy=true: Use domain size heuristic

    var registry = TileTypeRegistry.CreateDefault(5);
    var heuristics = new HeuristicsConfiguration
    {
      UseDomainEntropy = true,
      UseShannonEntropy = false,
      UseMostConstrainingTieBreak = true,
      ApplyInfluenceTieBreakForSingleHeuristic = true,
      PreferCentralCellTieBreak = true
    };

    var wfc = new WfcProvider(
      3,
      3,
      registry,
      new DeterministicRandomProvider(),
      new TileTypeRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);
    var poss = MappingTestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = MappingTestHelpers.GetPrivateField<int[][]>(wfc, "_output");

    Assert.NotNull(poss);
    Assert.NotNull(output);

    // Initialize all cells as decided (null = collapsed)
    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }

    // Set up symmetric corner domains + center domain with same entropy
    poss![0][0] = [1, 2];      // Top-left corner: entropy 2, influence 0, distance to center = 2
    poss[2][2] = [1, 2];       // Bottom-right corner: entropy 2, influence 0, distance to center = 2
    poss[1][1] = [1, 2];       // Center: entropy 2, influence 0, distance to center = 0

    var result = (ValueTuple<int, int>)MappingTestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;

    // ASSERTION: Should select (1,1) because:
    // 1. All three cells have same entropy (2) and influence (0)
    // 2. (1,1) is at distance 0 from grid center (most central position)
    // 3. (0,0) and (2,2) are both at distance 2 (corners)
    // 4. Central tie-break prefers minimum distance → select (1,1)
    Assert.Equal((1, 1), result);
  }
}