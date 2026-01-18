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
internal sealed class OptionsGroupRuleGrid : OptionsGrid
{
  private readonly GroupRuleConfiguration _config;

  public OptionsGroupRuleGrid(string headerText, GroupRuleConfiguration config)
  {
    _config = config ?? throw new ArgumentNullException(nameof(config));

    // Add header label
    var header = new TextRuntime
    {
      Text = headerText,
      X = 0,
      Y = -25,
      WidthUnits = DimensionUnitType.RelativeToParent,
      Height = 20,
      Width = 0 // Fill width
    };
    AddChild(header);

    // MinGroupSizeX
    var sldMinGroupSizeX = new OptionsSlider
    {
      Minimum = 1,
      Maximum = 64,
      Value = _config.MinGroupSizeX
    };
    var txtMinGroupSizeX = new TextBox { Text = _config.MinGroupSizeX.ToString(CultureInfo.InvariantCulture) };
    sldMinGroupSizeX.ValueChangedEvent += (_, __) => { _config.MinGroupSizeX = (int)Math.Round(sldMinGroupSizeX.Value); txtMinGroupSizeX.Text = _config.MinGroupSizeX.ToString(CultureInfo.InvariantCulture); };
    txtMinGroupSizeX.TextChanged += (_, __) => { if (int.TryParse(txtMinGroupSizeX.Text, out var val)) { sldMinGroupSizeX.Value = val; _config.MinGroupSizeX = val; } };
    AddRow("Min Group Size X", sldMinGroupSizeX, txtMinGroupSizeX);

    // MinGroupSizeY
    var sldMinGroupSizeY = new OptionsSlider
    {
      Minimum = 1,
      Maximum = 64,
      Value = _config.MinGroupSizeY
    };
    var txtMinGroupSizeY = new TextBox { Text = _config.MinGroupSizeY.ToString(CultureInfo.InvariantCulture) };
    sldMinGroupSizeY.ValueChangedEvent += (_, __) => { _config.MinGroupSizeY = (int)Math.Round(sldMinGroupSizeY.Value); txtMinGroupSizeY.Text = _config.MinGroupSizeY.ToString(CultureInfo.InvariantCulture); };
    txtMinGroupSizeY.TextChanged += (_, __) => { if (int.TryParse(txtMinGroupSizeY.Text, out var val)) { sldMinGroupSizeY.Value = val; _config.MinGroupSizeY = val; } };
    AddRow("Min Group Size Y", sldMinGroupSizeY, txtMinGroupSizeY);

    // MaxGroupSizeX
    var sldMaxGroupSizeX = new OptionsSlider
    {
      Minimum = 1,
      Maximum = 64,
      Value = _config.MaxGroupSizeX
    };
    var txtMaxGroupSizeX = new TextBox { Text = _config.MaxGroupSizeX.ToString(CultureInfo.InvariantCulture) };
    sldMaxGroupSizeX.ValueChangedEvent += (_, __) => { _config.MaxGroupSizeX = (int)Math.Round(sldMaxGroupSizeX.Value); txtMaxGroupSizeX.Text = _config.MaxGroupSizeX.ToString(CultureInfo.InvariantCulture); };
    txtMaxGroupSizeX.TextChanged += (_, __) => { if (int.TryParse(txtMaxGroupSizeX.Text, out var val)) { sldMaxGroupSizeX.Value = val; _config.MaxGroupSizeX = val; } };
    AddRow("Max Group Size X", sldMaxGroupSizeX, txtMaxGroupSizeX);

    // MaxGroupSizeY
    var sldMaxGroupSizeY = new OptionsSlider
    {
      Minimum = 1,
      Maximum = 64,
      Value = _config.MaxGroupSizeY
    };
    var txtMaxGroupSizeY = new TextBox { Text = _config.MaxGroupSizeY.ToString(CultureInfo.InvariantCulture) };
    sldMaxGroupSizeY.ValueChangedEvent += (_, __) => { _config.MaxGroupSizeY = (int)Math.Round(sldMaxGroupSizeY.Value); txtMaxGroupSizeY.Text = _config.MaxGroupSizeY.ToString(CultureInfo.InvariantCulture); };
    txtMaxGroupSizeY.TextChanged += (_, __) => { if (int.TryParse(txtMaxGroupSizeY.Text, out var val)) { sldMaxGroupSizeY.Value = val; _config.MaxGroupSizeY = val; } };
    AddRow("Max Group Size Y", sldMaxGroupSizeY, txtMaxGroupSizeY);

    // ElevationMin
    var sldElevationMin = new OptionsSlider
    {
      Minimum = 0,
      Maximum = 1,
      Value = _config.ElevationMin
    };
    var txtElevationMin = new TextBox { Text = _config.ElevationMin.ToString("F2", CultureInfo.InvariantCulture) };
    sldElevationMin.ValueChangedEvent += (_, __) => { _config.ElevationMin = (float)sldElevationMin.Value; txtElevationMin.Text = _config.ElevationMin.ToString("F2", CultureInfo.InvariantCulture); };
    txtElevationMin.TextChanged += (_, __) => { if (float.TryParse(txtElevationMin.Text, out var val)) { sldElevationMin.Value = val; _config.ElevationMin = val; } };
    AddRow("Elevation Min", sldElevationMin, txtElevationMin);

    // ElevationMax
    var sldElevationMax = new OptionsSlider
    {
      Minimum = 0,
      Maximum = 1,
      Value = _config.ElevationMax
    };
    var txtElevationMax = new TextBox { Text = _config.ElevationMax.ToString("F2", CultureInfo.InvariantCulture) };
    sldElevationMax.ValueChangedEvent += (_, __) => { _config.ElevationMax = (float)sldElevationMax.Value; txtElevationMax.Text = _config.ElevationMax.ToString("F2", CultureInfo.InvariantCulture); };
    txtElevationMax.TextChanged += (_, __) => { if (float.TryParse(txtElevationMax.Text, out var val)) { sldElevationMax.Value = val; _config.ElevationMax = val; } };
    AddRow("Elevation Max", sldElevationMax, txtElevationMax);

    // NoiseThreshold (if not null)
    if (_config.NoiseThreshold.HasValue)
    {
      var sldNoiseThreshold = new OptionsSlider
      {
        Minimum = 0,
        Maximum = 1,
        Value = _config.NoiseThreshold.Value
      };
      var txtNoiseThreshold = new TextBox { Text = _config.NoiseThreshold.Value.ToString("F2", CultureInfo.InvariantCulture) };
      sldNoiseThreshold.ValueChangedEvent += (_, __) => { _config.NoiseThreshold = (float)sldNoiseThreshold.Value; txtNoiseThreshold.Text = _config.NoiseThreshold.Value.ToString("F2", CultureInfo.InvariantCulture); };
      txtNoiseThreshold.TextChanged += (_, __) => { if (float.TryParse(txtNoiseThreshold.Text, out var val)) { sldNoiseThreshold.Value = val; _config.NoiseThreshold = val; } };
      AddRow("Noise Threshold", sldNoiseThreshold, txtNoiseThreshold);
    }
  }
}