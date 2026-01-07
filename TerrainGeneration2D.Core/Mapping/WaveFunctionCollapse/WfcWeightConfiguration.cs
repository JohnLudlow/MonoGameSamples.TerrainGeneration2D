namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Configuration for tile selection weights during WFC.
/// </summary>
public sealed class WfcWeightConfiguration
{
  /// <summary>
  /// Base weight applied to all candidates.
  /// </summary>
  public int Base { get; set; } = 1;

  /// <summary>
  /// Multiplier applied per neighbor match when computing candidate weights.
  /// </summary>
  public int NeighborMatchBoost { get; set; } = 3;
}
