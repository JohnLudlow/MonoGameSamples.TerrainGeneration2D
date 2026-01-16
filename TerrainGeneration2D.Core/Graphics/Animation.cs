using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class Animation
{
  public Collection<TextureRegion> Frames { get; } = [];
  public TimeSpan Delay { get; set; }
  public Animation()
  {
    Frames = [];
    Delay = TimeSpan.FromMilliseconds(100);
  }

  public Animation(IEnumerable<TextureRegion> frames, TimeSpan delay)
  {
    Frames = new Collection<TextureRegion>(new List<TextureRegion>(frames));
    Delay = delay;
  }
}