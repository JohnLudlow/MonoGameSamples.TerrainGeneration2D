using System;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;
using Xunit;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public sealed class HeuristicsTests
{
  private static readonly int[] TestDomain1 = { 1, 2, 3 };
  private static readonly int[] TestDomain2 = { 1, 2 };
  private static readonly WfcWeightConfiguration DefaultWeights = new() { Base = 1, NeighborMatchBoost = 3 };

  [Fact]
  public void DomainEntropyProvider_ScoresByDomainSize()
  {
    var provider = new DomainEntropyProvider();
    var possibilities = new HashSet<int>?[2][]
    {
      [[.. TestDomain1], null],
      [[.. TestDomain2], null]
    };
    var output = new int[2][]
    {
      [-1, -1],
      [-1, -1]
    };

    var k00 = provider.GetScore(0, 0, possibilities, output, DefaultWeights);
    var k10 = provider.GetScore(1, 0, possibilities, output, DefaultWeights);

    Assert.True(k10 < k00); // 2 < 3
  }

  [Fact]
  public void ShannonEntropyProvider_LowersEntropyWithPeakedPriors()
  {
    var provider = new ShannonEntropyProvider();
    var possibilities = new HashSet<int>?[2][]
    {
      [[.. TestDomain1], null],
      [null, null]
    };
    var output = new int[2][]
    {
      [-1, 1],
      [-1, -1]
    };

    var h = provider.GetScore(0, 0, possibilities, output, DefaultWeights);

    // Compare with uniform entropy ln(3) ≈ 1.099; expect h < ln(3)
    Assert.True(h < Math.Log(3.0));
  }
}
