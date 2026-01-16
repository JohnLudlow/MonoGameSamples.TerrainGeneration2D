using System;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class AnimatedSprite : Sprite
{
  private int _currentFrame;
  private TimeSpan _elapsed;

  public AnimatedSprite(TextureRegion textureRegion, GameAnimation animation) : base(textureRegion)
  {
    Animation = animation ?? throw new ArgumentNullException(nameof(animation));

    _currentFrame = 0;
    _elapsed = TimeSpan.Zero;
  }

  public GameAnimation Animation
  {
    get;
    set
    {
      field = value ?? throw new ArgumentNullException(nameof(value));
      _currentFrame = 0;
      _elapsed = TimeSpan.Zero;
    }
  }

  public void Update(GameTime gameTime)
  {
    ArgumentNullException.ThrowIfNull(gameTime);

    _elapsed += gameTime.ElapsedGameTime;
    if (_elapsed >= Animation.Delay)
    {
      _elapsed -= Animation.Delay;
      _currentFrame++;

      if (_currentFrame >= Animation.Frames.Count)
      {
        _currentFrame = 0;
      }

      Region = Animation.Frames[_currentFrame];
    }
  }
}