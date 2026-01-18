using System;
using System.Globalization;
using Gum.DataTypes;
using Gum.Forms.Controls;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGameGum;
using MonoGameGum.GueDeriving;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UI;

internal sealed class GameSceneUI : ContainerRuntime
{
  // The string format to use when updating the text for the score display.
#pragma warning disable CA1852
  private const string _scoreFormat = "SCORE: {0:D6}";
#pragma warning restore CA1852

  // The sound effect to play for auditory feedback of the user interface.
#pragma warning disable CS8618
  private SoundEffect _uiSoundEffect;

  // The pause panel
  private Panel _pausePanel;

  // The resume button on the pause panel. Field is used to track reference so
  // focus can be set when the pause panel is shown.
  private AnimatedButton _resumeButton;

  // The game over panel.
  private Panel _gameOverPanel;

  // The retry button on the game over panel. Field is used to track reference
  // so focus can be set when the game over panel is shown.
  private AnimatedButton _retryButton;

  // The text runtime used to display the players score on the game screen.
  private TextRuntime _scoreText;
  // The on-screen hint for controls.
  private TextRuntime _hintText;
#pragma warning restore CS8618

  /// <summary>
  /// Event invoked when the Resume button on the Pause panel is clicked.
  /// </summary>
  public event EventHandler ResumeButtonClick;

  /// <summary>
  /// Event invoked when the Quit button on either the Pause panel or the
  /// Game Over panel is clicked.
  /// </summary>
  public event EventHandler QuitButtonClick;

  /// <summary>
  /// Event invoked when the Retry button on the Game Over panel is clicked.
  /// </summary>
  public event EventHandler RetryButtonClick;



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public GameSceneUI()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  {
    // The game scene UI inherits from ContainerRuntime, so we set its
    // doc to fill so it fills the entire screen.
    Dock(Gum.Wireframe.Dock.Fill);

    // Add it to the root element.
    this.AddToRoot();

    // Get a reference to the content manager that was registered with the
    // GumService when it was original initialized.
#pragma warning disable CS8602
    var content = GumService.Default.ContentLoader.XnaContentManager;
#pragma warning restore CS8602

    // Use that content manager to load the sound effect and atlas for the
    // user interface elements
    _uiSoundEffect = content.Load<SoundEffect>("audio/ui");
    var atlas = TextureAtlas.FromFile(content, "images/atlas-definition.xml");

    // Create the text that will display the players score and add it as
    // a child to this container.
    _scoreText = CreateScoreText();
    AddChild(_scoreText);

    // Create the on-screen hint text (bottom-left) and add it.
    _hintText = CreateHintText();
    AddChild(_hintText);

    // Create the Pause panel that is displayed when the game is paused and
    // add it as a child to this container
    _pausePanel = CreatePausePanel(atlas);
    AddChild(_pausePanel.Visual);

    // Create the Game Over panel that is displayed when a game over occurs
    // and add it as a child to this container
    _gameOverPanel = CreateGameOverPanel(atlas);
    AddChild(_gameOverPanel.Visual);
  }

  /// <summary>
  /// Updates the text on the score display.
  /// </summary>
  /// <param name="score">The score to display.</param>
  public void UpdateScoreText(int score)
  {
#pragma warning disable CA1863
    _scoreText.Text = string.Format(CultureInfo.InvariantCulture, _scoreFormat, score);
#pragma warning restore CA1863
  }

  /// <summary>
  /// Tells the game scene ui to show the pause panel.
  /// </summary>
  public void ShowPausePanel()
  {
    _pausePanel.IsVisible = true;

    // Give the resume button focus for keyboard/gamepad input.
    _resumeButton.IsFocused = true;

    // Ensure the game over panel isn't visible.
    _gameOverPanel.IsVisible = false;
  }

  /// <summary>
  /// Tells the game scene ui to hide the pause panel.
  /// </summary>
  public void HidePausePanel()
  {
    _pausePanel.IsVisible = false;
  }

  /// <summary>
  /// Tells the game scene ui to show the game over panel.
  /// </summary>
  public void ShowGameOverPanel()
  {
    _gameOverPanel.IsVisible = true;

    // Give the retry button focus for keyboard/gamepad input.
    _retryButton.IsFocused = true;

    // Ensure the pause panel isn't visible.
    _pausePanel.IsVisible = false;
  }

  /// <summary>
  /// Tells the game scene ui to hide the game over panel.
  /// </summary>
#pragma warning disable CA1822
  public void HideGameOverPanel()
  {
    _gameOverPanel.IsVisible = false;
  }
#pragma warning restore CA1822

  /// <summary>
  /// Updates the game scene ui.
  /// </summary>
  /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
#pragma warning disable CA1822
  public void Update(GameTime gameTime)
  {
    GumService.Default.Update(gameTime);
  }
#pragma warning restore CA1822

  /// <summary>
  /// Draws the game scene ui.
  /// </summary>
#pragma warning disable CA1822
  public void Draw()
  {
    GumService.Default.Draw();
  }
#pragma warning restore CA1822


  private void OnResumeButtonClicked(object sender, EventArgs args)
  {
    // Button was clicked, play the ui sound effect for auditory feedback.
    JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.GameCore.Audio.PlaySoundEffect(_uiSoundEffect);

    // Since the resume button was clicked, we need to hide the pause panel.
    HidePausePanel();

    // Invoke the ResumeButtonClick event
    ResumeButtonClick?.Invoke(sender, args);
  }

  private void OnRetryButtonClicked(object sender, EventArgs args)
  {
    // Button was clicked, play the ui sound effect for auditory feedback.
    JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.GameCore.Audio.PlaySoundEffect(_uiSoundEffect);

    // Since the retry button was clicked, we need to hide the game over panel.
    HideGameOverPanel();

    // Invoke the RetryButtonClick event.
    RetryButtonClick?.Invoke(sender, args);
  }

  private void OnQuitButtonClicked(object sender, EventArgs args)
  {
    // Button was clicked, play the ui sound effect for auditory feedback.
    JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.GameCore.Audio.PlaySoundEffect(_uiSoundEffect);

    // Both panels have a quit button, so hide both panels
    HidePausePanel();
    HideGameOverPanel();

    // Invoke the QuitButtonClick event.
#pragma warning disable CA1822
    if (QuitButtonClick != null)
    {
      QuitButtonClick(sender, args);
    }
#pragma warning restore CA1822
  }

  private void OnElementGotFocus(object sender, EventArgs args)
  {
    // A ui element that can receive focus has received focus, play the
    // ui sound effect for auditory feedback.
    JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.GameCore.Audio.PlaySoundEffect(_uiSoundEffect);
  }


#pragma warning disable CA1822 // Mark members as static
  private TextRuntime CreateScoreText()
#pragma warning restore CA1822 // Mark members as static
  {
    var text = new TextRuntime();
    text.Anchor(Gum.Wireframe.Anchor.TopLeft);
    text.WidthUnits = DimensionUnitType.RelativeToChildren;
    text.X = 20.0f;
    text.Y = 5.0f;
    text.FontScale = 0.25f;
#pragma warning disable CA1863
    text.Text = string.Format(CultureInfo.InvariantCulture, _scoreFormat, 0);
#pragma warning restore CA1863

    return text;
  }

  private Panel CreatePausePanel(TextureAtlas atlas)
  {
    var panel = new Panel();
    panel.Anchor(Gum.Wireframe.Anchor.Center);
    panel.WidthUnits = DimensionUnitType.Absolute;
    panel.HeightUnits = DimensionUnitType.Absolute;
    panel.Width = 264.0f;
    panel.Height = 70.0f;
    panel.IsVisible = false;

    var backgroundRegion = atlas.GetRegion("panel-background");

    var background = new NineSliceRuntime();
    background.Dock(Gum.Wireframe.Dock.Fill);
    background.Texture = backgroundRegion.Texture;
    background.TextureAddress = Gum.Managers.TextureAddress.Custom;
    background.TextureHeight = backgroundRegion.Height;
    background.TextureWidth = backgroundRegion.Width;
    background.TextureTop = backgroundRegion.SourceRectangle.Top;
    background.TextureLeft = backgroundRegion.SourceRectangle.Left;
    panel.AddChild(background);

    var text = new TextRuntime();
    text.Text = "PAUSED";
    text.UseCustomFont = true;
    text.CustomFontFile = "fonts/NotArial.fnt";
    text.FontScale = 0.5f;
    text.X = 10.0f;
    text.Y = 10.0f;
    panel.AddChild(text);

    _resumeButton = new AnimatedButton(atlas);
#pragma warning disable CS8622
    _resumeButton.Text = "RESUME";
    _resumeButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
    _resumeButton.X = 9.0f;
    _resumeButton.Y = -9.0f;

    _resumeButton.Click += OnResumeButtonClicked;
    _resumeButton.GotFocus += OnElementGotFocus;
#pragma warning restore CS8622

    panel.AddChild(_resumeButton);

    var quitButton = new AnimatedButton(atlas);
#pragma warning disable CS8622
    quitButton.Text = "QUIT";
    quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
    quitButton.X = -9.0f;
    quitButton.Y = -9.0f;

    quitButton.Click += OnQuitButtonClicked;
    quitButton.GotFocus += OnElementGotFocus;
#pragma warning restore CS8622

    panel.AddChild(quitButton);

    return panel;
  }

  private Panel CreateGameOverPanel(TextureAtlas atlas)
  {
    var panel = new Panel();
    panel.Anchor(Gum.Wireframe.Anchor.Center);
    panel.WidthUnits = DimensionUnitType.Absolute;
    panel.HeightUnits = DimensionUnitType.Absolute;
    panel.Width = 264.0f;
    panel.Height = 70.0f;
    panel.IsVisible = false;

    TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

    var background = new NineSliceRuntime();
    background.Dock(Gum.Wireframe.Dock.Fill);
    background.Texture = backgroundRegion.Texture;
    background.TextureAddress = Gum.Managers.TextureAddress.Custom;
    background.TextureHeight = backgroundRegion.Height;
    background.TextureWidth = backgroundRegion.Width;
    background.TextureTop = backgroundRegion.SourceRectangle.Top;
    background.TextureLeft = backgroundRegion.SourceRectangle.Left;
    panel.AddChild(background);

    var text = new TextRuntime();
    text.Text = "GAME OVER";
    text.WidthUnits = DimensionUnitType.RelativeToChildren;
    text.UseCustomFont = true;
    text.CustomFontFile = "fonts/NotArial.fnt";
    text.FontScale = 0.5f;
    text.X = 10.0f;
    text.Y = 10.0f;
    panel.AddChild(text);

    _retryButton = new AnimatedButton(atlas);
#pragma warning disable CS8622
    _retryButton.Text = "RETRY";
    _retryButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
    _retryButton.X = 9.0f;
    _retryButton.Y = -9.0f;

    _retryButton.Click += OnRetryButtonClicked;
    _retryButton.GotFocus += OnElementGotFocus;
#pragma warning restore CS8622

    panel.AddChild(_retryButton);

    var quitButton = new AnimatedButton(atlas);
#pragma warning disable CS8622
    quitButton.Text = "QUIT";
    quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
    quitButton.X = -9.0f;
    quitButton.Y = -9.0f;

    quitButton.Click += OnQuitButtonClicked;
    quitButton.GotFocus += OnElementGotFocus;
#pragma warning restore CS8622

    panel.AddChild(quitButton);

    return panel;
  }

#pragma warning disable CA1822 // Mark members as static
  private TextRuntime CreateHintText()
#pragma warning restore CA1822 // Mark members as static
  {
    var text = new TextRuntime();
    // Bottom-left with small padding
    text.Dock(Gum.Wireframe.Dock.Bottom);
    text.Anchor(Gum.Wireframe.Anchor.Bottom);
    text.X = 10.0f;
    text.Y = -10.0f;
    text.UseCustomFont = true;
    text.CustomFontFile = "fonts/NotArial.fnt";

    // Slightly dim color for subtle hint
    text.Red = 220;
    text.Green = 220;
    text.Blue = 220;
    text.Text = "F10: Settings • F11: Fullscreen • F12: Debug";
    text.WidthUnits = DimensionUnitType.RelativeToChildren;
    return text;
  }


}