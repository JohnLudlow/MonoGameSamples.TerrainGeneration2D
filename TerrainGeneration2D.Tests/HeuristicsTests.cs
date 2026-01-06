using System;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;
using Xunit;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public sealed class HeuristicsTests
{
  [Fact]
  public void DomainEntropyProvider_ScoresByDomainSize()
  {
    var provider = new DomainEntropyProvider();
    var possibilities = new HashSet<int>?[2,2];
    var output = new int[2,2];
    for (int y=0;y<2;y++) for (int x=0;x<2;x++) output[x,y] = -1;

    possibilities[0,0] = new HashSet<int>(new[]{1,2,3});
    possibilities[1,0] = new HashSet<int>(new[]{1,2});

    var weight = new WfcWeightConfiguration{ Base = 1, NeighborMatchBoost = 3 };
    var k00 = provider.GetScore(0,0, possibilities, output, weight);
    var k10 = provider.GetScore(1,0, possibilities, output, weight);

    Assert.True(k10 < k00); // 2 < 3
  }

  [Fact]
  public void ShannonEntropyProvider_LowersEntropyWithPeakedPriors()
  {
    var provider = new ShannonEntropyProvider();
    var possibilities = new HashSet<int>?[2,2];
    var output = new int[2,2];
    for (int y=0;y<2;y++) for (int x=0;x<2;x++) output[x,y] = -1;

    // Domain has 3 candidates
    possibilities[0,0] = new HashSet<int>(new[]{1,2,3});

    // Set a neighbor to match tile 1 to create peaked prior via NeighborMatchBoost
    output[0,1] = 1; // below (y+1)

    var weight = new WfcWeightConfiguration{ Base = 1, NeighborMatchBoost = 3 };
    var h = provider.GetScore(0,0, possibilities, output, weight);

    // Compare with uniform entropy ln(3) ≈ 1.099; expect h < ln(3)
    Assert.True(h < Math.Log(3.0));
  }
}
