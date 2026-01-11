using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;
using Xunit;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public sealed class UniformPickFractionTests
{
  private sealed class DeterministicRandomProvider : IRandomProvider
  {
    public int NextInt() => 0;
    public int NextInt(int maxValue) => 0;
    public int NextInt(int minValue, int maxValue) => minValue;
    public double NextDouble() => 0.0; // always triggers uniform path when fraction > 0
  }

  [Fact]
  public void UniformPickFraction_ForcesUniformSelection_OverContextWeights()
  {
    var registry = TileTypeRegistry.CreateDefault(5);
    var heuristics = new HeuristicsConfiguration
    {
      UseDomainEntropy = true,
      UseShannonEntropy = false,
      UseMostConstrainingTieBreak = false,
      ApplyInfluenceTieBreakForSingleHeuristic = false,
      PreferCentralCellTieBreak = false,
      UniformPickFraction = 1.0, // always take uniform path
      MostConstrainingBias = 0.0
    };

    var wfc = new WfcProvider(
      2,
      1,
      registry,
      new DeterministicRandomProvider(),
      new TerrainRuleConfiguration(),
      DefaultHeightProvider.Instance,
      Point.Zero,
      new WfcWeightConfiguration { Base = 1, NeighborMatchBoost = 5 },
      heuristics);

    var poss = TestHelpers.GetPrivateField<HashSet<int>?[][]>(wfc, "_possibilities");
    var output = TestHelpers.GetPrivateField<int[][]>(wfc, "_output");
    Assert.NotNull(poss);
    Assert.NotNull(output);

    // Initialize
    poss![0][0] = null; // decided
    output![0][0] = 2;  // left neighbor collapsed to tile 2

    // Right cell undecided with two candidates; weighted path would prefer tile 2 due to neighbor match
    poss[1][0] = new HashSet<int>(new[] { 1, 2 });
    output[1][0] = -1;

    // Invoke collapse directly to avoid propagation constraints influencing the outcome
    var mi = typeof(WfcProvider).GetMethod("CollapseCell", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, binder: null, types: new[] { typeof(int), typeof(int) }, modifiers: null);
    Assert.NotNull(mi);
    var collapsed = (bool)mi!.Invoke(wfc, new object[] { 1, 0 })!;
    Assert.True(collapsed);
    // Uniform path sorts candidates ascending and picks index 0 (tile 1) due to deterministic NextInt()
    Assert.Equal(1, output[1][0]);
  }
}
