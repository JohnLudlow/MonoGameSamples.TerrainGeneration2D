using System;
using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;

/// <summary>
/// Scores a cell using Shannon entropy from neighbor-weighted priors.
/// Lower H indicates a more decisive cell to collapse.
/// </summary>
public sealed class ShannonEntropyProvider : ICellEntropyProvider
{
  public double GetScore(int x, int y, HashSet<int>?[,] possibilities, int[,] output, WfcWeightConfiguration weightConfig)
  {
    var poss = possibilities[x, y];
    if (poss == null || poss.Count <= 1)
      return double.PositiveInfinity;

    // Collect already-collapsed neighbors
    var neighbors = new List<int>(4);
    if (y > 0 && output[x, y - 1] != -1) neighbors.Add(output[x, y - 1]);
    if (y < output.GetLength(1) - 1 && output[x, y + 1] != -1) neighbors.Add(output[x, y + 1]);
    if (x > 0 && output[x - 1, y] != -1) neighbors.Add(output[x - 1, y]);
    if (x < output.GetLength(0) - 1 && output[x + 1, y] != -1) neighbors.Add(output[x + 1, y]);

    // Compute weights using neighbor-match boost
    var weights = new List<int>(poss.Count);
    int sum = 0;
    foreach (var tile in poss)
    {
      var matches = 0;
      for (var i = 0; i < neighbors.Count; i++)
      {
        if (neighbors[i] == tile) matches++;
      }
      var w = Math.Max(1, weightConfig.Base + matches * weightConfig.NeighborMatchBoost);
      weights.Add(w);
      sum += w;
    }

    // Shannon entropy H = - sum p_i log p_i (natural log)
    double h = 0.0;
    foreach (var w in weights)
    {
      var p = (double)w / sum;
      h -= p * Math.Log(p);
    }
    return h;
  }
}
