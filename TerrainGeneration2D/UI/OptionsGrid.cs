using System;
using System.Collections.Generic;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

/// <summary>
/// A grid control with three columns: labels on the left, text inputs in the middle, controls on the right.
/// Automatically arranges rows vertically and responds to content sizes.
/// </summary>
internal class OptionsGrid : Grid
{
  private int _currentRow = 0;
  private const float LabelWidth = 200f;
  private const float ControlWidth = 50f;
  private const float TextWidth = 60f;

  public OptionsGrid() : base(100, 3) // Large number of rows, 3 columns
  {
  }

  /// <summary>
  /// Adds a new row with a label, text input, and control.
  /// </summary>
  /// <param name="label">The text for the label column.</param>
  /// <param name="control">The control for the third column.</param>
  /// <param name="textBox">The text input for the second column.</param>
  public void AddRow(string label, FrameworkElement control, TextBox? textBox = null)
  {
    if (_currentRow >= 100) throw new InvalidOperationException("Too many rows added.");

    // Create label as TextRuntime
    var labelText = new TextRuntime
    {
      Text = label,
      // labelText.CustomFontFile = @"fonts/NotArial.fnt";
      // labelText.UseCustomFont = true;
      Width = LabelWidth,
      Height = 20, // Fixed height for simplicity
      WidthUnits = DimensionUnitType.Absolute,
      HeightUnits = DimensionUnitType.Absolute
    };

    // Add label at current row, column 0
    AddChild(labelText, _currentRow, 0);

    // Add text box at current row, column 1 if provided
    if (textBox != null)
    {
      textBox.Width = TextWidth;
      textBox.WidthUnits = DimensionUnitType.Absolute;
      textBox.Height = 20;
      textBox.HeightUnits = DimensionUnitType.Absolute;
      AddChild(textBox.Visual, _currentRow, 1);
    }

    // Add control at current row, column 2
    control.Width = ControlWidth;
    control.WidthUnits = DimensionUnitType.Absolute;
    control.HeightUnits = DimensionUnitType.RelativeToChildren;

    AddChild(control.Visual, _currentRow, 2);

    // After layout, center the label vertically in the row
    labelText.Y += 10;

    _currentRow++;
  }
}