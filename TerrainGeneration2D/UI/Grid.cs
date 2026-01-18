using System;
using System.Collections.Generic;
using Gum.DataTypes;
using Gum.Wireframe;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

/// <summary>
/// A basic grid control with defined rows and columns.
/// Child controls are placed at specific row/column positions.
/// Row heights and column widths are determined by the maximum content size in each row/column.
/// </summary>
internal class Grid : ContainerRuntime
{
  private readonly int _rows;
  private readonly int _columns;
  private readonly List<List<GraphicalUiElement?>> _gridCells;
  private readonly List<float> _rowHeights;
  private readonly List<float> _columnWidths;

  public Grid(int rows, int columns)
  {
    if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
    if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be positive.");

    _rows = rows;
    _columns = columns;
    _gridCells = [];
    _rowHeights = [.. new float[rows]];
    _columnWidths = [.. new float[columns]];

    for (var i = 0; i < rows; i++)
    {
      _gridCells.Add([.. new GraphicalUiElement?[columns]]);
    }

    Anchor(Gum.Wireframe.Anchor.Left);
    Dock(Gum.Wireframe.Dock.Left);

    WidthUnits = DimensionUnitType.RelativeToChildren;
    HeightUnits = DimensionUnitType.RelativeToChildren;
    Width = 0;
    Height = 0;
  }

  /// <summary>
  /// Adds a child control at the specified row and column.
  /// </summary>
  /// <param name="child">The control to add.</param>
  /// <param name="row">The row index (0-based).</param>
  /// <param name="column">The column index (0-based).</param>
  public void AddChild(GraphicalUiElement child, int row, int column)
  {
    if (row < 0 || row >= _rows) throw new ArgumentOutOfRangeException(nameof(row));
    if (column < 0 || column >= _columns) throw new ArgumentOutOfRangeException(nameof(column));
    if (_gridCells[row][column] != null) throw new InvalidOperationException("Cell already occupied.");

    _gridCells[row][column] = child;
    base.AddChild(child);

    UpdateGridLayout();
  }

  /// <summary>
  /// Updates the layout of the grid based on child sizes.
  /// </summary>
  private void UpdateGridLayout()
  {
    // Reset sizes
    for (var i = 0; i < _rows; i++) _rowHeights[i] = 0;
    for (var j = 0; j < _columns; j++) _columnWidths[j] = 0;

    // Calculate max sizes
    for (var r = 0; r < _rows; r++)
    {
      for (var c = 0; c < _columns; c++)
      {
        var child = _gridCells[r][c];
        if (child != null)
        {
          _rowHeights[r] = Math.Max(_rowHeights[r], child.Height);
          _columnWidths[c] = Math.Max(_columnWidths[c], child.Width);
        }
      }
    }

    // Position children
    float yOffset = 0;
    for (var r = 0; r < _rows; r++)
    {
      float xOffset = 0;
      for (var c = 0; c < _columns; c++)
      {
        var child = _gridCells[r][c];
        if (child != null)
        {
          child.X = xOffset;
          child.Y = yOffset;
        }
        xOffset += _columnWidths[c];
      }
      yOffset += _rowHeights[r];
    }

    // Update grid size
    Width = 0;
    for (var j = 0; j < _columns; j++) Width += _columnWidths[j];

    Height = 0;
    for (var i = 0; i < _rows; i++) Height += _rowHeights[i];
  }
}