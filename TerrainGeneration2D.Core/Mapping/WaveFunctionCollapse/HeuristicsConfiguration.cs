namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Configures cell-selection heuristics for WFC.
/// </summary>
public sealed class HeuristicsConfiguration
{
  /// <summary>
  /// When true, selects cells by smallest domain size (fewest candidates).
  /// </summary>
  public bool UseDomainEntropy { get; set; } = true;

  /// <summary>
  /// When true, selects cells by Shannon entropy using weighted priors.
  /// </summary>
  public bool UseShannonEntropy { get; set; } = false;

  /// <summary>
  /// When both domain and Shannon are enabled, prefer the cell that constrains the most neighbors.
  /// </summary>
  public bool UseMostConstrainingTieBreak { get; set; } = true;

  /// <summary>
  /// When true, apply the "most constraining" tie-break even when only a single heuristic is enabled.
  /// </summary>
  public bool ApplyInfluenceTieBreakForSingleHeuristic { get; set; } = true;

  /// <summary>
  /// When true, in remaining ties prefer cells closest to the grid center (Manhattan distance).
  /// </summary>
  public bool PreferCentralCellTieBreak { get; set; } = false;

  /// <summary>
  /// Blend factor for uniform vs. weighted tile selection during non-backtracking collapse.
  /// Range: 0..1 where 0 = fully weighted, 1 = fully uniform. Default 0.
  /// </summary>
  public double UniformPickFraction { get; set; } = 0.0;

  /// <summary>
  /// Soft bias strength favoring higher-influence cells during tie-breaks.
  /// If 0, a hard filter (max influence) is applied when enabled. If > 0,
  /// candidates are selected via weighted random with weights (1 + bias * influence).
  /// </summary>
  public double MostConstrainingBias { get; set; } = 0.0;
}
