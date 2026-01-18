// Integration with boundary constraints for chunk seaming
// using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

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

/// <summary>
/// Generic configuration for WFC solver, holding settings, domains, and constraints.
/// </summary>
/// <typeparam name="TCell">Cell coordinate type</typeparam>
/// <typeparam name="TValue">Value type</typeparam>
public class WfcConfiguration<TCell, TValue> : WfcConfiguration
{
  public WfcConfiguration(IReadOnlyDictionary<TCell, ISet<TValue>> initialDomains, IRuleTable<TValue> ruleTable)
  {
    InitialDomains = initialDomains;
    RuleTable = ruleTable;
  }

  /// <summary>
  /// Gets or sets the initial domain for each cell (possible values).
  /// </summary>
  public IReadOnlyDictionary<TCell, ISet<TValue>> InitialDomains { get; set; }

  /// <summary>
  /// Gets or sets the rule table defining allowed neighbor relationships.
  /// </summary>
  public IRuleTable<TValue> RuleTable { get; set; }
}
