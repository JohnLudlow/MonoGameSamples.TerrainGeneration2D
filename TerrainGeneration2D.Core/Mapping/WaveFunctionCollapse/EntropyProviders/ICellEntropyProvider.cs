using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse.EntropyProviders;

/// <summary>
/// Strategy interface to score a cell's entropy for selection.
/// Lower scores indicate a better candidate to collapse next.
/// </summary>
public interface ICellEntropyProvider
{
  /// <summary>
  /// Compute an entropy score for the cell (x,y).
  /// Return double.PositiveInfinity for settled cells (no selection).
  /// </summary>
  double GetScore(int x, int y,
    HashSet<int>?[][] possibilities,
    int[][] output,
    WfcWeightConfiguration weightConfig);
}
