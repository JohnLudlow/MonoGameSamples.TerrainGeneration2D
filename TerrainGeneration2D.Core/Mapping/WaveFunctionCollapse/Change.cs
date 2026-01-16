using System.Collections.Generic;
using System.Linq;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// A single reversible mutation. Depending on <see cref="ChangeKind"/>,
/// fields encode the minimum data needed to undo the change.
/// </summary>
internal readonly struct Change
{
  /// <summary>The mutation category.</summary>
  public ChangeKind Kind { get; }
  /// <summary>Target cell x-coordinate.</summary>
  public int X { get; }
  /// <summary>Target cell y-coordinate.</summary>
  public int Y { get; }
  /// <summary>When <see cref="ChangeKind.DomainRemoved"/>, the tile ID removed from the domain.</summary>
  public int RemovedTileId { get; }
  /// <summary>When <see cref="ChangeKind.CellCollapsed"/>, the tile ID chosen for the collapse.</summary>
  public int ChosenTileId { get; }
  /// <summary>When <see cref="ChangeKind.OutputSet"/>, the previous output tile ID.</summary>
  public int PrevOutput { get; }
  /// <summary>When <see cref="ChangeKind.OutputSet"/>, the new output tile ID.</summary>
  public int NextOutput { get; }
  /// <summary>Optional snapshot (e.g., full previous domain for a collapse).</summary>
  public IReadOnlyCollection<int>? PrevDomainSnapshot { get; }

  public Change(ChangeKind kind, int x, int y, int removedTileId, int chosenTileId, int prevOutput, int nextOutput, IReadOnlyCollection<int>? prevDomainSnapshot)
  {
    Kind = kind; X = x; Y = y; RemovedTileId = removedTileId; ChosenTileId = chosenTileId; PrevOutput = prevOutput; NextOutput = nextOutput; PrevDomainSnapshot = prevDomainSnapshot;
  }

  /// <summary>Create a domain-removal mutation for (x,y).</summary>
  public static Change DomainRemoved(int x, int y, int tileId) => new(ChangeKind.DomainRemoved, x, y, tileId, 0, 0, 0, null);
  /// <summary>Create a collapse mutation with a snapshot of the previous domain.</summary>
  public static Change CellCollapsed(int x, int y, IEnumerable<int> prevDomain, int chosen) => new(ChangeKind.CellCollapsed, x, y, 0, chosen, 0, 0, prevDomain.ToArray());
  /// <summary>Create an output-set mutation for (x,y) recording old and new values.</summary>
  public static Change OutputSet(int x, int y, int prev, int next) => new(ChangeKind.OutputSet, x, y, 0, 0, prev, next, null);
}