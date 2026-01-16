using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class GameAnimation
{
  public Collection<TextureRegion> Frames { get; } = [];
  public TimeSpan Delay { get; set; }
  public GameAnimation()
  {
    Frames = [];
    Delay = TimeSpan.FromMilliseconds(100);
  }

  public GameAnimation(IEnumerable<TextureRegion> frames, TimeSpan delay)
  {
    Frames = new Collection<TextureRegion>(new List<TextureRegion>(frames));
    Delay = delay;
  }
}