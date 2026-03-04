using System;
using System.Globalization;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

/// <summary>
/// A grid control that binds to a GroupRuleConfiguration, displaying sliders for its numeric properties.
/// </summary>
internal sealed class OptionsGroupRuleGrid : ContainerRuntime
{
  private readonly GroupRuleConfiguration _config;
  public OptionsGroupRuleGrid(string headerText, GroupRuleConfiguration config)
  {
    _config = config ?? throw new ArgumentNullException(nameof(config));

    var header = new TextRuntime
    {
      Text = headerText,
      X = 0,
      Y = 0,
      WidthUnits = DimensionUnitType.RelativeToParent,
      Height = 20,
      Width = 0
    };
    AddChild(header);
  }
}