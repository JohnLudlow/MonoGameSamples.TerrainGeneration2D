using System;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

/// <summary>
/// A custom slider control that inherits from Gum's Slider class.
/// </summary>
internal sealed class OptionsSlider : Slider
{

  // Reference to the rectangle that visually represents the current value
  private ColoredRectangleRuntime _fillRectangle;

  /// <summary>
  /// Event raised when the value changes.
  /// </summary>
  public event EventHandler? ValueChangedEvent;

  /// <summary>
  /// Creates a new OptionsSlider instance with a simple, functional design.
  /// </summary>
  public OptionsSlider()
  {
    // Create the top-level container for all visual elements
    var topLevelContainer = new ContainerRuntime
    {
      Height = 40f,
      Width = 264f
    };

    // Create the container for the slider track
    var innerContainer = new ContainerRuntime
    {
      Height = 10f,
      Width = 241f,
      X = 10f,
      Y = 26f
    };
    topLevelContainer.AddChild(innerContainer);

    // Create the track background
    var trackBackground = new ColoredRectangleRuntime
    {
      Color = Color.DarkGray
    };
    trackBackground.Dock(Gum.Wireframe.Dock.Fill);
    innerContainer.AddChild(trackBackground);

    // Create the interactive track that responds to clicks
    // The special name "TrackInstance" is required for Slider functionality
    var trackInstance = new ContainerRuntime();
    trackInstance.Name = "TrackInstance";
    trackInstance.Dock(Gum.Wireframe.Dock.Fill);
    trackInstance.Height = -2f;
    trackInstance.Width = -2f;
    trackBackground.AddChild(trackInstance);

    // Create the fill rectangle that visually displays the current value
    _fillRectangle = new ColoredRectangleRuntime
    {
      Color = Color.LightBlue
    };
    _fillRectangle.Dock(Gum.Wireframe.Dock.Left);
    _fillRectangle.Width = 90f; // Default to 90% - will be updated by value changes
    _fillRectangle.WidthUnits = DimensionUnitType.PercentageOfParent;
    trackInstance.AddChild(_fillRectangle);

    // Add "OFF" text to the left end
    var offText = new TextRuntime();
    offText.CustomFontFile = @"fonts/NotArial.fnt";
    offText.FontScale = 0.2f;
    offText.UseCustomFont = true;
    offText.Text = "OFF";
    offText.X = 5f;
    offText.Y = 2f;
    innerContainer.AddChild(offText);

    // Add "MAX" text to the right end
    var maxText = new TextRuntime
    {
      CustomFontFile = @"fonts/NotArial.fnt",
      FontScale = 0.2f,
      UseCustomFont = true,
      Text = "MAX"
    };
    maxText.Anchor(Gum.Wireframe.Anchor.TopRight);
    maxText.X = -5f;
    maxText.Y = 2f;
    innerContainer.AddChild(maxText);

    // Define colors for focused and unfocused states
    var focusedColor = Color.White;
    var unfocusedColor = Color.Gray;

    // Create slider state category - Slider.SliderCategoryName is the required name
    var sliderCategory = new StateSaveCategory
    {
      Name = SliderCategoryName
    };
    topLevelContainer.AddCategory(sliderCategory);

    // Create the enabled (default/unfocused) state
    var enabled = new StateSave
    {
      Name = EnabledStateName,
      Apply = () =>
        {
          // When enabled but not focused, use gray coloring for text
          _fillRectangle.Color = Color.LightBlue;
        }
    };
    sliderCategory.States.Add(enabled);

    // Create the focused state
    var focused = new StateSave
    {
      Name = FocusedStateName,
      Apply = () =>
        {
          // When focused, use white coloring for text
          _fillRectangle.Color = Color.Cyan;
        }
    };
    sliderCategory.States.Add(focused);

    // Create the highlighted+focused state by cloning the focused state
    var highlightedFocused = focused.Clone();
    highlightedFocused.Name = FrameworkElement.HighlightedFocusedStateName;
    sliderCategory.States.Add(highlightedFocused);

    // Create the highlighted state by cloning the enabled state
    var highlighted = enabled.Clone();
    highlighted.Name = FrameworkElement.HighlightedStateName;
    sliderCategory.States.Add(highlighted);

    // Assign the configured container as this slider's visual
    Visual = topLevelContainer;

    // Enable click-to-point functionality for the slider
    // This allows users to click anywhere on the track to jump to that value
    IsMoveToPointEnabled = true;
    IsEnabled = true;

    // Add event handlers
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
    Visual.RollOn += HandleRollOn;
    base.ValueChanged += (s, e) => ValueChangedEvent?.Invoke(this, e);
    ValueChangedEvent += HandleValueChanged;
    ValueChangedByUi += HandleValueChangedByUi;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
  }

  /// <summary>
  /// Overrides the Value property to ensure ValueChangedEvent is fired when set programmatically.
  /// </summary>
  public new double Value
  {
    get => base.Value;
    set
    {
      if (base.Value != value)
      {
        base.Value = value;
        ValueChangedEvent?.Invoke(this, EventArgs.Empty);
      }
    }
  }

  /// <summary>
  /// Automatically focuses the slider when the user interacts with it
  /// </summary>
  private void HandleValueChangedByUi(object sender, EventArgs e)
  {
    IsFocused = true;
  }

  /// <summary>
  /// Automatically focuses the slider when the mouse hovers over it
  /// </summary>
  private void HandleRollOn(object sender, EventArgs e)
  {
    IsFocused = true;
  }

  /// <summary>
  /// Updates the fill rectangle width to visually represent the current value
  /// </summary>
  private void HandleValueChanged(object sender, EventArgs e)
  {
    // Calculate the ratio of the current value within its range
    var ratio = (Value - Minimum) / (Maximum - Minimum);

    // Update the fill rectangle width as a percentage
    // _fillRectangle uses percentage width units, so we multiply by 100
    _fillRectangle.Width = 100 * (float)ratio;
  }
}