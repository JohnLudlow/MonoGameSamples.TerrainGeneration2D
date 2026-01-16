using System;
using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Append-only list of reversible mutations with checkpoint/rollback support.
/// Call <see cref="Mark"/> to capture a checkpoint, record mutations as they
/// occur, and <see cref="RollbackTo"/> to restore domains and output to the
/// state at a given checkpoint.
/// </summary>
public sealed class ChangeLog
{
  private readonly List<Change> _changes = new();
  /// <summary>Create a checkpoint representing the current tail of the log.</summary>
  public int Mark() => _changes.Count;
  /// <summary>Record removal of a single tile from a domain at (x,y).</summary>
  public void RecordDomainRemoved(int x, int y, int tileId) => _changes.Add(Change.DomainRemoved(x, y, tileId));
  /// <summary>Record collapsing a domain to a single tile, with a snapshot for undo.</summary>
  public void RecordCellCollapsed(int x, int y, IEnumerable<int> prevDomain, int chosen) => _changes.Add(Change.CellCollapsed(x, y, prevDomain, chosen));
  /// <summary>Record setting the output at (x,y) to a new tile, keeping the previous value.</summary>
  public void RecordOutputSet(int x, int y, int previous, int next) => _changes.Add(Change.OutputSet(x, y, previous, next));
  /// <summary>Undo all mutations recorded after <paramref name="mark"/>.</summary>
  public void RollbackTo(int mark, HashSet<int>?[][] domains, int[][] output)
  {
    ArgumentNullException.ThrowIfNull(output);

    for (var i = _changes.Count - 1; i >= mark; i--)
    {
      var c = _changes[i];
      switch (c.Kind)
      {
        case ChangeKind.OutputSet:
          output[c.X][c.Y] = c.PrevOutput;
          break;
          
        case ChangeKind.CellCollapsed:
          domains?[c.X][c.Y] = new HashSet<int>(c.PrevDomainSnapshot ?? Array.Empty<int>());
          break;

        case ChangeKind.DomainRemoved:
          var d = domains?[c.X][c.Y];
          d?.Add(c.RemovedTileId);
          
          break;
      }
    }
    if (mark < _changes.Count)
    {
      _changes.RemoveRange(mark, _changes.Count - mark);
    }
  }
}
