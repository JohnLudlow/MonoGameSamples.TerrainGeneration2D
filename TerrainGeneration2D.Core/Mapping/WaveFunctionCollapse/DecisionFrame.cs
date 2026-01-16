using System;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Represents a single branching point for the solver.
/// Stores the target cell, an ordered candidate list, and the
/// change-log bookmark required to roll back mutations made while
/// attempting a candidate.
/// </summary>
internal sealed class DecisionFrame
{
  /// <summary>The x-coordinate of the chosen cell.</summary>
  public int X { get; init; }
  /// <summary>The y-coordinate of the chosen cell.</summary>
  public int Y { get; init; }
  /// <summary>Ordered list of tile IDs to try at (X,Y).</summary>
  public int[] Candidates { get; init; } = Array.Empty<int>();
  /// <summary>Index of the next candidate to attempt.</summary>
  public int NextIndex { get; set; }
  /// <summary>Checkpoint into the change log taken before trying any candidate.</summary>
  public int ChangesMark { get; init; }
  /// <summary>Depth of this frame in the decision stack (for diagnostics).</summary>
  public int Depth { get; init; }
}