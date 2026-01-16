using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;

/// <summary>
/// Scores a cell by its domain size (fewest candidates preferred).
/// </summary>
public sealed class DomainEntropyProvider : ICellEntropyProvider
{
  public double GetScore(int x, int y, HashSet<int>?[][] possibilities, int[][] output, WfcWeightConfiguration weightConfig)
  {
    var poss = possibilities[x][y];
    if (poss == null || poss.Count <= 1)
      return double.PositiveInfinity; // settled or contradiction handled elsewhere
    return poss.Count; // k
  }
}