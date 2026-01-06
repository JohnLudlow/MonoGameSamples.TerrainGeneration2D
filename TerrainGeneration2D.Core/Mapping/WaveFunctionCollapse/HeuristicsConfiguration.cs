namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Configures cell-selection heuristics for WFC.
/// </summary>
public sealed class HeuristicsConfiguration
{
  /// <summary>
  /// When true, selects cells by smallest domain size (fewest candidates).
  /// </summary>
  public bool UseDomainEntropy { get; init; } = true;

  /// <summary>
  /// When true, selects cells by Shannon entropy using weighted priors.
  /// </summary>
  public bool UseShannonEntropy { get; init; } = false;

  /// <summary>
  /// When both domain and Shannon are enabled, prefer the cell that constrains the most neighbors.
  /// </summary>
  public bool UseMostConstrainingTieBreak { get; init; } = true;
}
