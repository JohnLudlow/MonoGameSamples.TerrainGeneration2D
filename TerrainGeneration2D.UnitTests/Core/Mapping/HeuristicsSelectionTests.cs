using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon.Core.Mapping;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UnitTests.Core.Mapping;

public sealed class HeuristicsSelectionTests
{
  public object NappingTestHelpers { get; private set; }

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
      new TileTypeRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);
    var poss = MappingTestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = MappingTestHelpers.GetPrivateField<int[][]>(wfc, "_output");
    Assert.NotNull(poss);
    Assert.NotNull(output);
    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }
    poss![0][0] = new HashSet<int> { 1, 2 };
    poss[1][1] = new HashSet<int> { 1, 2 };
    poss[1][0] = new HashSet<int> { 1 };
    poss[1][2] = new HashSet<int> { 1 };
    poss[0][1] = new HashSet<int> { 1 };
    poss[2][1] = new HashSet<int> { 1 };
    var result = (System.ValueTuple<int, int>)MappingTestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;
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
      new TileTypeRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration(),
      heuristics);
    var poss = MappingTestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = MappingTestHelpers.GetPrivateField<int[][]>(wfc, "_output");

    Assert.NotNull(poss);
    Assert.NotNull(output);

    for (var y = 0; y < 3; y++)
    {
      for (var x = 0; x < 3; x++)
      {
        poss![x][y] = null;
        output![x][y] = -1;
      }
    }
    poss![0][0] = [1, 2];
    poss[2][2] = [1, 2];
    poss[1][1] = [1, 2];
    var result = (ValueTuple<int, int>)MappingTestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;
    Assert.Equal((1, 1), result);
  }
}
