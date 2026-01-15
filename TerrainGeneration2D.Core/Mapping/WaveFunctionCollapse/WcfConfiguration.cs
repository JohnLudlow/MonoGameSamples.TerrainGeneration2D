// Integration with boundary constraints for chunk seaming
// using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse
{
  /// <summary>
  /// Sample configuration class aggregating all WFC-related settings for chunk generation.
  /// </summary>
  public class WfcConfiguration
  {
    /// <summary>
    /// Tile selection weights.
    /// </summary>
    public WfcWeightConfiguration Weights { get; set; } = new WfcWeightConfiguration();

    /// <summary>
    /// Heuristic and entropy settings.
    /// </summary>
    public HeuristicsConfiguration Heuristics { get; set; } = new HeuristicsConfiguration();

    /// <summary>
    /// Time budget for WFC generation (in milliseconds).
    /// </summary>
    public int TimeBudgetMs { get; set; } = 50;

    /// <summary>
    /// Additional configuration fields as needed.
    /// </summary>
    // public ...
  }
}