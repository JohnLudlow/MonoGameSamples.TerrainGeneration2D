using System;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

public sealed class RuntimeSettingsPanel : Panel
{
  private readonly AnimatedButton _btnDomain;
  private readonly AnimatedButton _btnShannon;
  private readonly AnimatedButton _btnMostConstraining;
  private readonly AnimatedButton _btnInfluenceSingle;
  private readonly AnimatedButton _btnCenterBias;
  private readonly OptionsSlider _sldUniform;
  private readonly OptionsSlider _sldBias;
  private readonly OptionsSlider _sldTimeBudget;
  private readonly OptionsSlider _sldMountainRangeMin;
  private readonly OptionsSlider _sldMountainRangeMax;
  private readonly OptionsSlider _sldMountainWidthMin;
  private readonly OptionsSlider _sldMountainWidthMax;
  private readonly AnimatedButton _btnRegenVisible;
  private readonly AnimatedButton _btnClearSaves;
  private readonly ContainerRuntime _scrollContent;
  private readonly AnimatedButton _btnScrollUp;
  private readonly AnimatedButton _btnScrollDown;
  private float _scrollOffset;

  private HeuristicsConfiguration? _heur;
  private Func<int>? _getBudget;
  private Action<int>? _setBudget;
  private JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes.TerrainRuleConfiguration? _rules;
  private Action? _regenerateVisible;
  private Action? _clearSaves;

  public RuntimeSettingsPanel(TextureAtlas atlas)
  {
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
      UseCustomFont = true,
      CustomFontFile = "fonts/04b_30.fnt",
      FontScale = 0.4f,
      X = 8,
      Y = 6,
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

    float y = 0f;
    _btnDomain = CreateToggle(atlas, "Domain: ", 8, y);
    _scrollContent.AddChild(_btnDomain.Visual);
    y += 20;
    _btnShannon = CreateToggle(atlas, "Shannon: ", 8, y);
    _scrollContent.AddChild(_btnShannon.Visual);
    y += 20;
    _btnMostConstraining = CreateToggle(atlas, "MostConstraining: ", 8, y);
    _scrollContent.AddChild(_btnMostConstraining.Visual);
    y += 20;
    _btnInfluenceSingle = CreateToggle(atlas, "Influence@Single: ", 8, y);
    _scrollContent.AddChild(_btnInfluenceSingle.Visual);
    y += 20;
    _btnCenterBias = CreateToggle(atlas, "CenterBias: ", 8, y);
    _scrollContent.AddChild(_btnCenterBias.Visual);
    y += 24;

    _sldUniform = new OptionsSlider(atlas)
    {
      X = 8,
      Y = y,
      Minimum = 0,
      Maximum = 1,
      Value = 0,
    };
    _sldUniform.Text = "Uniform Fraction";
    _sldUniform.ValueChanged += (_, __) =>
    {
      if (_heur != null) _heur.UniformPickFraction = _sldUniform.Value;
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldUniform.Visual);
    y += 44;

    _sldBias = new OptionsSlider(atlas)
    {
      X = 8,
      Y = y,
      Minimum = 0,
      Maximum = 1,
      Value = 0,
    };
    _sldBias.Text = "Influence Bias";
    _sldBias.ValueChanged += (_, __) =>
    {
      if (_heur != null) _heur.MostConstrainingBias = _sldBias.Value;
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldBias.Visual);
    y += 44;

    _sldTimeBudget = new OptionsSlider(atlas)
    {
      X = 8,
      Y = y,
      Minimum = 5,
      Maximum = 200,
      Value = 50
    };
    _sldTimeBudget.Text = "WFC Time Budget (ms)";
    _sldTimeBudget.ValueChanged += (_, __) =>
    {
      _setBudget?.Invoke((int)Math.Round(_sldTimeBudget.Value));
      _regenerateVisible?.Invoke();
    };
    _scrollContent.AddChild(_sldTimeBudget.Visual);
    y += 44;

    // Terrain rules section
    var rulesTitle = new TextRuntime
    {
      Text = "Terrain Rules",
      UseCustomFont = true,
      CustomFontFile = "fonts/04b_30.fnt",
      FontScale = 0.4f,
      X = 8,
      Y = y,
      WidthUnits = DimensionUnitType.RelativeToChildren
    };
    _scrollContent.AddChild(rulesTitle);
    y += 18;

    _sldMountainRangeMin = new OptionsSlider(atlas) { X = 8, Y = y, Minimum = 1, Maximum = 256, Value = 8 };
    _sldMountainRangeMin.Text = "Mountain Range Min";
    _sldMountainRangeMin.ValueChanged += (_, __) => ApplyMountainRangeMin();
    _scrollContent.AddChild(_sldMountainRangeMin.Visual);
    y += 44;

    _sldMountainRangeMax = new OptionsSlider(atlas) { X = 8, Y = y, Minimum = 1, Maximum = 256, Value = 48 };
    _sldMountainRangeMax.Text = "Mountain Range Max";
    _sldMountainRangeMax.ValueChanged += (_, __) => ApplyMountainRangeMax();
    _scrollContent.AddChild(_sldMountainRangeMax.Visual);
    y += 44;

    _sldMountainWidthMin = new OptionsSlider(atlas) { X = 8, Y = y, Minimum = 1, Maximum = 64, Value = 3 };
    _sldMountainWidthMin.Text = "Mountain Width Min";
    _sldMountainWidthMin.ValueChanged += (_, __) => ApplyMountainWidthMin();
    _scrollContent.AddChild(_sldMountainWidthMin.Visual);
    y += 44;

    _sldMountainWidthMax = new OptionsSlider(atlas) { X = 8, Y = y, Minimum = 1, Maximum = 64, Value = 12 };
    _sldMountainWidthMax.Text = "Mountain Width Max";
    _sldMountainWidthMax.ValueChanged += (_, __) => ApplyMountainWidthMax();
    _scrollContent.AddChild(_sldMountainWidthMax.Visual);
    y += 44;

    // Actions
    _btnRegenVisible = CreateToggle(atlas, "Apply Changes", 8, y); y += 20;
    _btnRegenVisible.Click += (_, __) => _regenerateVisible?.Invoke();
    _btnClearSaves = CreateToggle(atlas, "Clear Saves", 8, y); y += 20;
    _btnClearSaves.Click += (_, __) => _clearSaves?.Invoke();
    _scrollContent.AddChild(_btnRegenVisible.Visual);
    _scrollContent.AddChild(_btnClearSaves.Visual);

    UpdateScroll();
  }

  public void Bind(HeuristicsConfiguration heuristics,
                   JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes.TerrainRuleConfiguration rules,
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

    ApplyHeuristicsToUi();
    ApplyRulesToUi();

    _btnDomain.Click += (_, __) => { _heur.UseDomainEntropy = !_heur.UseDomainEntropy; UpdateToggle(_btnDomain, "Domain: ", _heur.UseDomainEntropy); _regenerateVisible?.Invoke(); };
    _btnShannon.Click += (_, __) => { _heur.UseShannonEntropy = !_heur.UseShannonEntropy; UpdateToggle(_btnShannon, "Shannon: ", _heur.UseShannonEntropy); _regenerateVisible?.Invoke(); };
    _btnMostConstraining.Click += (_, __) => { _heur.UseMostConstrainingTieBreak = !_heur.UseMostConstrainingTieBreak; UpdateToggle(_btnMostConstraining, "MostConstraining: ", _heur.UseMostConstrainingTieBreak); _regenerateVisible?.Invoke(); };
    _btnInfluenceSingle.Click += (_, __) => { _heur.ApplyInfluenceTieBreakForSingleHeuristic = !_heur.ApplyInfluenceTieBreakForSingleHeuristic; UpdateToggle(_btnInfluenceSingle, "Influence@Single: ", _heur.ApplyInfluenceTieBreakForSingleHeuristic); _regenerateVisible?.Invoke(); };
    _btnCenterBias.Click += (_, __) => { _heur.PreferCentralCellTieBreak = !_heur.PreferCentralCellTieBreak; UpdateToggle(_btnCenterBias, "CenterBias: ", _heur.PreferCentralCellTieBreak); _regenerateVisible?.Invoke(); };
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

  private void ApplyRulesToUi()
  {
    if (_rules == null) return;
    _sldMountainRangeMin.Value = _rules.MountainRangeMin;
    _sldMountainRangeMax.Value = _rules.MountainRangeMax;
    _sldMountainWidthMin.Value = _rules.MountainWidthMin;
    _sldMountainWidthMax.Value = _rules.MountainWidthMax;
  }

  private void ApplyMountainRangeMin()
  {
    if (_rules == null) return;
    var v = (int)Math.Round(_sldMountainRangeMin.Value);
    if (v > _rules.MountainRangeMax)
    {
      _rules.MountainRangeMax = v;
      _sldMountainRangeMax.Value = v;
    }
    _rules.MountainRangeMin = v;
    _regenerateVisible?.Invoke();
  }

  private void ApplyMountainRangeMax()
  {
    if (_rules == null) return;
    var v = (int)Math.Round(_sldMountainRangeMax.Value);
    if (v < _rules.MountainRangeMin)
    {
      _rules.MountainRangeMin = v;
      _sldMountainRangeMin.Value = v;
    }
    _rules.MountainRangeMax = v;
    _regenerateVisible?.Invoke();
  }

  private void ApplyMountainWidthMin()
  {
    if (_rules == null) return;
    var v = (int)Math.Round(_sldMountainWidthMin.Value);
    if (v > _rules.MountainWidthMax)
    {
      _rules.MountainWidthMax = v;
      _sldMountainWidthMax.Value = v;
    }
    _rules.MountainWidthMin = v;
    _regenerateVisible?.Invoke();
  }

  private void ApplyMountainWidthMax()
  {
    if (_rules == null) return;
    var v = (int)Math.Round(_sldMountainWidthMax.Value);
    if (v < _rules.MountainWidthMin)
    {
      _rules.MountainWidthMin = v;
      _sldMountainWidthMin.Value = v;
    }
    _rules.MountainWidthMax = v;
    _regenerateVisible?.Invoke();
  }

  private AnimatedButton CreateToggle(TextureAtlas atlas, string label, float x, float y)
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
