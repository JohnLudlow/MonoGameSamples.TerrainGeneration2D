using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;


/// <summary>
/// Adapter for legacy tile-based WFC API, preserving backward compatibility.
/// </summary>
public class LegacyTileWfcAdapter : IWfcSolver<(int x, int y), int>
{
  private readonly WfcProvider _legacyProvider;

  /// <summary>
  /// Initializes a new instance of the LegacyTileWfcAdapter class.
  /// </summary>
  /// <param name="legacyProvider">The legacy WFC provider to adapt</param>
  public LegacyTileWfcAdapter(WfcProvider legacyProvider) => _legacyProvider = legacyProvider;

  /// <inheritdoc />
  /// <param name="config">Configuration for the WFC solve (may be partially used or ignored for legacy compatibility)</param>
  public WfcSolution<(int x, int y), int>? Solve(WfcConfiguration<(int x, int y), int> config)
  {
    // Bridge call to legacy provider
    var success = _legacyProvider.Generate();
    if (!success) return null;

    // Convert legacy output to generic solution
    // ...implementation omitted...
    // Example: Extract assignments from legacy provider's output

    var assignments = new Dictionary<(int x, int y), int>(); // Populate from legacy output
    return new WfcSolution<(int x, int y), int>(assignments);
  }
}