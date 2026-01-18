using System;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

internal sealed class RuntimeSettingsPanel : Panel
{
  private const float ControlHeight = 16f;
  private const float ControlSpacing = 4f;
  private const float LabelFontScale = 0.18f;
  private const float SliderWidth = 120f;
  private float _scrollOffset;
  private float _currentY;

  private HeuristicsConfiguration? _heur;
  private Func<int>? _getBudget;
  private Action<int>? _setBudget;
  private TileTypeRuleConfiguration? _rules;
  private Action? _regenerateVisible;
  private Action? _clearSaves;

  public RuntimeSettingsPanel(TextureAtlas atlas)
  {
    var registry = TileTypeRegistry.CreateDefault(100);

    Anchor(Gum.Wireframe.Anchor.Left);
    Dock(Gum.Wireframe.Dock.Left);
    WidthUnits = DimensionUnitType.Absolute;
    HeightUnits = DimensionUnitType.RelativeToParent;
    Width = 320;
    Height = 0; // Fill vertical space
    X = 0;
    Y = 0;

    var title = new TextRuntime
    {
      Text = "Heuristics Settings",
      X = 6,
      Y = 4,
      WidthUnits = DimensionUnitType.RelativeToChildren
    };
    AddChild(title);
  }

  public void Bind(HeuristicsConfiguration heuristics,
                   TileTypeRuleConfiguration rules,
                   Func<int> getBudget,
                   Action<int> setBudget,
                   Action regenerateVisible,
                   Action clearSaves)
  {
    _heur = heuristics;
    _rules = rules;
    _getBudget = getBudget;
    _setBudget = setBudget;
    _regenerateVisible = regenerateVisible;
    _clearSaves = clearSaves;

    // Create scrollable listview for terrain rules
    var scrollViewer = new ScrollViewer
    {
      X = 6,
      Y = 30,
      Width = 300,
      Height = 0,
      WidthUnits = DimensionUnitType.Absolute,
      HeightUnits = DimensionUnitType.RelativeToParent
    };

    float y = 0;
    foreach (var rule in _rules.Rules)
    {
      var grid = new OptionsGroupRuleGrid(rule.Id.ToString(), rule)
      {
        X = 0,
        Y = y,
        WidthUnits = DimensionUnitType.RelativeToParent
      };
      scrollViewer.AddChild(grid);
      y += 300; // Approximate height per grid
    }

    AddChild(scrollViewer);

    // Add buttons at the bottom
    var regenerateButton = new Button
    {
      Text = "Regenerate Visible",
      X = 6,
      Y = 435,
      Width = 140,
      Height = 25,
      WidthUnits = DimensionUnitType.Absolute,
      HeightUnits = DimensionUnitType.Absolute
    };
    regenerateButton.Click += (_, __) => _regenerateVisible?.Invoke();
    AddChild(regenerateButton);

    var clearButton = new Button
    {
      Text = "Clear Saves",
      X = 156,
      Y = 435,
      Width = 140,
      Height = 25,
      WidthUnits = DimensionUnitType.Absolute,
      HeightUnits = DimensionUnitType.Absolute
    };
    clearButton.Click += (_, __) => _clearSaves?.Invoke();
    AddChild(clearButton);
  }  
}