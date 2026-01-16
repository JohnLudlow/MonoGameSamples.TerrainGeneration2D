using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public sealed class MostConstrainingBiasTests
{
  private sealed class HighRollRandomProvider : IRandomProvider
  {
    public int NextInt() => 0;
    public int NextInt(int maxValue) => 0;
    public int NextInt(int minValue, int maxValue) => minValue;
    public double NextDouble() => 0.999999; // force selection of the last weighted bucket
  }

  [Fact]
  public void InfluenceWeightedTieBreak_SelectsHigherInfluenceCell_WhenBiasEnabled()
  {
    var registry = TileTypeRegistry.CreateDefault(5);
    var heuristics = new HeuristicsConfiguration
    {
      UseDomainEntropy = true,
      UseShannonEntropy = false,
      UseMostConstrainingTieBreak = true,
      ApplyInfluenceTieBreakForSingleHeuristic = true,
      PreferCentralCellTieBreak = false,
      MostConstrainingBias = 0.5
    };

    var wfc = new WfcProvider(
      3,
      3,
      registry,
      new HighRollRandomProvider(),
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
    poss![0][0] = new HashSet<int>(new[] { 1, 2 }); // lower influence
    poss[1][1] = new HashSet<int>(new[] { 1, 2 }); // higher influence

    // Make center's neighbors undecided to raise influence
    poss[1][0] = new HashSet<int>(new[] { 1 });
    poss[1][2] = new HashSet<int>(new[] { 1 });
    poss[0][1] = new HashSet<int>(new[] { 1 });
    poss[2][1] = new HashSet<int>(new[] { 1 });

    // Leave corner with no undecided neighbors to keep its influence minimal

    // Invoke private FindLowestEntropy and assert it selects the center (1,1)
    var result = (ValueTuple<int, int>)TestHelpers.InvokePrivateMethod(wfc, "FindLowestEntropy")!;
    Assert.Equal((1, 1), result);
  }
}