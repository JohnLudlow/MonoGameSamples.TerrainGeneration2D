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
  private readonly TextureAtlas _atlas;
  private readonly AnimatedButton _btnDomain;
  private readonly AnimatedButton _btnShannon;
  private readonly AnimatedButton _btnMostConstraining;
  private readonly AnimatedButton _btnInfluenceSingle;
  private readonly AnimatedButton _btnCenterBias;
  private readonly OptionsSlider _sldUniform;
  private readonly OptionsSlider _sldBias;
  private readonly OptionsSlider _sldTimeBudget;
  private readonly AnimatedButton _btnRegenVisible;
  private readonly AnimatedButton _btnClearSaves;
  private readonly ContainerRuntime _scrollContent;
  private readonly AnimatedButton _btnScrollUp;
  private readonly AnimatedButton _btnScrollDown;
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
    _atlas = atlas ?? throw new ArgumentNullException(nameof(atlas));
    Anchor(Gum.Wireframe.Anchor.TopLeft);
    WidthUnits = DimensionUnitType.Absolute;
    HeightUnits = DimensionUnitType.Absolute;
    Width = 320;
    Height = 520;
    X = 10;
    Y = 10;

    var title = new TextRuntime
    {
      Text = "Heuristics Settings",
      X = 6,
      Y = 4,
      WidthUnits = DimensionUnitType.RelativeToChildren
    };
    AddChild(title);

    // Scrollable content region below the title
    _scrollContent = new ContainerRuntime();
    _scrollContent.X = 0;
    _scrollContent.Y = 28f;
    _scrollContent.WidthUnits = DimensionUnitType.RelativeToParent;
    _scrollContent.HeightUnits = DimensionUnitType.RelativeToChildren;
    AddChild(_scrollContent);

    // Simple scroll buttons
    _btnScrollUp = new AnimatedButton(atlas);
    _btnScrollUp.Text = "▲";
    _btnScrollUp.Anchor(Gum.Wireframe.Anchor.TopRight);
    _btnScrollUp.X = -24f;
    _btnScrollUp.Y = 2f;
    _btnScrollUp.Click += (_, __) => { _scrollOffset = Math.Max(0, _scrollOffset - 20f); UpdateScroll(); };
    AddChild(_btnScrollUp);

    _btnScrollDown = new AnimatedButton(atlas);
    _btnScrollDown.Text = "▼";
    _btnScrollDown.Anchor(Gum.Wireframe.Anchor.BottomRight);
    _btnScrollDown.X = -24f;
    _btnScrollDown.Y = -4f;
    _btnScrollDown.Click += (_, __) => { _scrollOffset = _scrollOffset + 20f; UpdateScroll(); };
    AddChild(_btnScrollDown);

    var y = 0f;
    var controlHeight = ControlHeight;
    var controlSpacing = ControlSpacing;
    var labelFontScale = LabelFontScale;
    var sliderWidth = SliderWidth;

    _btnDomain = CreateToggle(atlas, "Domain: ", 6, y);
    _btnDomain.Visual.Height = controlHeight;
    // FontScale only if supported
    _scrollContent.AddChild(_btnDomain.Visual);
    y += controlHeight + controlSpacing;
    _btnShannon = CreateToggle(atlas, "Shannon: ", 6, y);
    _btnShannon.Visual.Height = controlHeight;
    _scrollContent.AddChild(_btnShannon.Visual);
    y += controlHeight + controlSpacing;
    _btnMostConstraining = CreateToggle(atlas, "MostConstraining: ", 6, y);
    _btnMostConstraining.Visual.Height = controlHeight;
    _scrollContent.AddChild(_btnMostConstraining.Visual);
    y += controlHeight + controlSpacing;
    _btnInfluenceSingle = CreateToggle(atlas, "Influence@Single: ", 6, y);
    _btnInfluenceSingle.Visual.Height = controlHeight;
    _scrollContent.AddChild(_btnInfluenceSingle.Visual);
    y += controlHeight + controlSpacing;
    _btnCenterBias = CreateToggle(atlas, "CenterBias: ", 6, y);
    _btnCenterBias.Visual.Height = controlHeight;
    _scrollContent.AddChild(_btnCenterBias.Visual);
    y += controlHeight + controlSpacing * 2;

    _sldUniform = new OptionsSlider
    {
      X = 6,
      Y = y,
      Minimum = 0,
      Maximum = 1,
      Value = 0,
      Width = sliderWidth,
      Height = controlHeight,
      Text = "Uniform Fraction"
    };
    // FontScale not supported on OptionsSlider
    _sldUniform.ValueChanged += (_, __) =>
    {
      if (_heur != null) _heur.UniformPickFraction = _sldUniform.Value;
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldUniform.Visual);
    y += controlHeight + controlSpacing;

    _sldBias = new OptionsSlider()
    {
      X = 6,
      Y = y,
      Minimum = 0,
      Maximum = 1,
      Value = 0,
      Width = sliderWidth,
      Height = controlHeight
    };
    _sldBias.Text = "Influence Bias";
    // FontScale not supported on OptionsSlider
    _sldBias.ValueChanged += (_, __) =>
    {
      if (_heur != null) _heur.MostConstrainingBias = _sldBias.Value;
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldBias.Visual);
    y += controlHeight + controlSpacing;

    _sldTimeBudget = new OptionsSlider()
    {
      X = 6,
      Y = y,
      Minimum = 5,
      Maximum = 200,
      Value = 50,
      Width = sliderWidth,
      Height = controlHeight
    };
    _sldTimeBudget.Text = "WFC Time Budget (ms)";
    // FontScale not supported on OptionsSlider
    _sldTimeBudget.ValueChanged += (_, __) =>
    {
      _setBudget?.Invoke((int)Math.Round(_sldTimeBudget.Value));
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldTimeBudget.Visual);
    y += controlHeight + controlSpacing * 2;

    _currentY = y;

    // Actions
    _btnRegenVisible = CreateToggle(atlas, "Apply Changes", 6, y); y += controlHeight + controlSpacing;
    _btnRegenVisible.Visual.Height = controlHeight;
    _btnRegenVisible.Click += (_, __) => _regenerateVisible?.Invoke();
    _btnClearSaves = CreateToggle(atlas, "Clear Saves", 6, y); y += controlHeight + controlSpacing;
    _btnClearSaves.Visual.Height = controlHeight;
    _btnClearSaves.Click += (_, __) => _clearSaves?.Invoke();
    _scrollContent.AddChild(_btnRegenVisible.Visual);
    _scrollContent.AddChild(_btnClearSaves.Visual);

    UpdateScroll();
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

    // Add terrain rules controls
    var y = _currentY;
    var controlHeight = ControlHeight;
    var controlSpacing = ControlSpacing;
    var sliderWidth = SliderWidth;

    // Terrain rules section
    var rulesTitle = new TextRuntime
    {
      Text = "Terrain Rules",
      FontScale = LabelFontScale + 0.04f,
      X = 6,
      Y = y,
      WidthUnits = DimensionUnitType.RelativeToChildren
    };
    _scrollContent.AddChild(rulesTitle);
    y += controlHeight + controlSpacing;

    // For each terrain type, add a group of controls for all GroupRuleConfiguration parameters
    foreach (var rule in _rules.Rules)
    {
      var groupLabel = new TextRuntime
      {
        Text = $"Type {rule.Id}",
        FontScale = LabelFontScale,
        X = 10,
        Y = y,
        WidthUnits = DimensionUnitType.RelativeToChildren
      };
      _scrollContent.AddChild(groupLabel);
      y += controlHeight;

      // MinGroupSizeX
      var sldMinGroupX = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 1,
        Maximum = 64,
        Value = rule.MinGroupSizeX,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldMinGroupX.Text = "MinGroupSizeX";
      sldMinGroupX.ValueChanged += (_, __) => { rule.MinGroupSizeX = (int)Math.Round(sldMinGroupX.Value); _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldMinGroupX.Visual);
      y += controlHeight;

      // MinGroupSizeY
      var sldMinGroupY = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 1,
        Maximum = 64,
        Value = rule.MinGroupSizeY,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldMinGroupY.Text = "MinGroupSizeY";
      sldMinGroupY.ValueChanged += (_, __) => { rule.MinGroupSizeY = (int)Math.Round(sldMinGroupY.Value); _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldMinGroupY.Visual);
      y += controlHeight;

      // MaxGroupSizeX
      var sldMaxGroupX = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 1,
        Maximum = 256,
        Value = rule.MaxGroupSizeX,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldMaxGroupX.Text = "MaxGroupSizeX";
      sldMaxGroupX.ValueChanged += (_, __) => { rule.MaxGroupSizeX = (int)Math.Round(sldMaxGroupX.Value); _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldMaxGroupX.Visual);
      y += controlHeight;

      // MaxGroupSizeY
      var sldMaxGroupY = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 1,
        Maximum = 256,
        Value = rule.MaxGroupSizeY,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldMaxGroupY.Text = "MaxGroupSizeY";
      sldMaxGroupY.ValueChanged += (_, __) => { rule.MaxGroupSizeY = (int)Math.Round(sldMaxGroupY.Value); _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldMaxGroupY.Visual);
      y += controlHeight;

      // ElevationMin
      var sldElevationMin = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 0,
        Maximum = 1,
        Value = rule.ElevationMin,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldElevationMin.Text = "ElevationMin";
      sldElevationMin.ValueChanged += (_, __) => { rule.ElevationMin = (float)sldElevationMin.Value; _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldElevationMin.Visual);
      y += controlHeight;

      // ElevationMax
      var sldElevationMax = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 0,
        Maximum = 1,
        Value = rule.ElevationMax,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldElevationMax.Text = "ElevationMax";
      sldElevationMax.ValueChanged += (_, __) => { rule.ElevationMax = (float)sldElevationMax.Value; _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldElevationMax.Visual);
      y += controlHeight;

      // NoiseProvider (dropdown or text input)
      var txtNoiseProvider = new TextBox
      {
        X = 14,
        Y = y,
        Width = sliderWidth,
        Height = controlHeight,
        Text = rule.NoiseProvider ?? ""
      };
      txtNoiseProvider.TextChanged += (_, __) => { rule.NoiseProvider = txtNoiseProvider.Text; _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(txtNoiseProvider);
      y += controlHeight;

      // NoiseThreshold (nullable float)
      var sldNoiseThreshold = new OptionsSlider()
      {
        X = 14,
        Y = y,
        Minimum = 0,
        Maximum = 1,
        Value = rule.NoiseThreshold ?? 0,
        Width = sliderWidth,
        Height = controlHeight
      };
      sldNoiseThreshold.Text = "NoiseThreshold";
      sldNoiseThreshold.ValueChanged += (_, __) => { rule.NoiseThreshold = (float)sldNoiseThreshold.Value; _regenerateVisible?.Invoke(); };
      _scrollContent.AddChild(sldNoiseThreshold.Visual);
      y += controlHeight + controlSpacing * 2;
    }

    _currentY = y;

    ApplyHeuristicsToUi();

    _btnDomain.Click += (_, __) => { _heur.UseDomainEntropy = !_heur.UseDomainEntropy; UpdateToggle(_btnDomain, "Domain: ", _heur.UseDomainEntropy); _regenerateVisible?.Invoke(); };
    _btnShannon.Click += (_, __) => { _heur.UseShannonEntropy = !_heur.UseShannonEntropy; UpdateToggle(_btnShannon, "Shannon: ", _heur.UseShannonEntropy); _regenerateVisible?.Invoke(); };
    _btnMostConstraining.Click += (_, __) => { _heur.UseMostConstrainingTieBreak = !_heur.UseMostConstrainingTieBreak; UpdateToggle(_btnMostConstraining, "MostConstraining: ", _heur.UseMostConstrainingTieBreak); _regenerateVisible?.Invoke(); };
    _btnInfluenceSingle.Click += (_, __) => { _heur.ApplyInfluenceTieBreakForSingleHeuristic = !_heur.ApplyInfluenceTieBreakForSingleHeuristic; UpdateToggle(_btnInfluenceSingle, "Influence@Single: ", _heur.ApplyInfluenceTieBreakForSingleHeuristic); _regenerateVisible?.Invoke(); };
    _btnCenterBias.Click += (_, __) => { _heur.PreferCentralCellTieBreak = !_heur.PreferCentralCellTieBreak; UpdateToggle(_btnCenterBias, "CenterBias: ", _heur.PreferCentralCellTieBreak); _regenerateVisible?.Invoke(); };

    UpdateScroll();
  }

  private void ApplyHeuristicsToUi()
  {
    if (_heur == null) return;
    UpdateToggle(_btnDomain, "Domain: ", _heur.UseDomainEntropy);
    UpdateToggle(_btnShannon, "Shannon: ", _heur.UseShannonEntropy);
    UpdateToggle(_btnMostConstraining, "MostConstraining: ", _heur.UseMostConstrainingTieBreak);
    UpdateToggle(_btnInfluenceSingle, "Influence@Single: ", _heur.ApplyInfluenceTieBreakForSingleHeuristic);
    UpdateToggle(_btnCenterBias, "CenterBias: ", _heur.PreferCentralCellTieBreak);
    _sldUniform.Value = _heur.UniformPickFraction;
    _sldBias.Value = _heur.MostConstrainingBias;
    _sldTimeBudget.Value = _getBudget != null ? _getBudget() : 50;
  }

#pragma warning disable CA1822 // Mark members as static
  private AnimatedButton CreateToggle(TextureAtlas atlas, string label, float x, float y)
#pragma warning restore CA1822 // Mark members as static
  {
    var b = new AnimatedButton(atlas);
    b.X = x;
    b.Y = y;
    b.Text = label + "OFF";
    // Parent decides where to add
    return b;
  }

  private void UpdateScroll()
  {
    _scrollContent.Y = 28f - _scrollOffset;
  }

  private static void UpdateToggle(AnimatedButton b, string label, bool on)
  {
    b.Text = label + (on ? "ON" : "OFF");
  }
}