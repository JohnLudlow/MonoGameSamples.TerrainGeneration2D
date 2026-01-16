using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

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
      new TerrainRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);

    var poss = TestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = TestHelpers.GetPrivateField<int[][]>(wfc, "_output");
    Assert.NotNull(poss);
    Assert.NotNull(output);

    // Initialize all cells as decided (null possibilities)
    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }

    // Two candidate cells with equal domain size: corner (0,0) and center (1,1)
    poss![0][0] = [1, 2]; // influence will be 0 (neighbors are null)
    poss[1][1] = [1, 2]; // we will give it 4 undecided neighbors

    // Make center's neighbors undecided to raise influence
    poss[1][0] = [1];
    poss[1][2] = [1];
    poss[0][1] = [1];
    poss[2][1] = [1];

    // Invoke private FindLowestEntropy and assert it selects the center (1,1)
    var result = (ValueTuple<int, int>)TestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;
    Assert.Equal((1, 1), result);
  }

  [Fact]
  public void CentralTieBreak_PrefersCenter_WhenEntropyAndInfluenceTied()
  {
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
      new TerrainRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);

    var poss = TestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = TestHelpers.GetPrivateField<int[][]>(wfc, "_output");
    Assert.NotNull(poss);
    Assert.NotNull(output);

    // Initialize all cells as decided
    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }

    // Two candidates with equal domain and equal influence: corner (0,0) and center (1,1)
    poss![0][0] = [1, 2];
    poss[1][1] = [1, 2];

    // Give the corner exactly 2 undecided neighbors (max possible for a corner)
    poss[1][0] = [1];
    poss[0][1] = [1];

    // Give the center also exactly 2 undecided neighbors to tie influence
    poss[1][0] = [1]; // already set, shared neighbor
    poss[1][2] = [1];
    // Note: left/right of center remain decided (null) to keep influence equal to 2

    var result = (ValueTuple<int, int>)TestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;
    Assert.Equal((1, 1), result);
  }
}