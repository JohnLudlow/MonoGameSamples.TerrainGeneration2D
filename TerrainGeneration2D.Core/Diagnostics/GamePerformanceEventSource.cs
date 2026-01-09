using System.Diagnostics.Tracing;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

[EventSource(Name = "MyGame.Performance")]
public sealed class GamePerformanceEventSource : EventSource
{
  public static readonly GamePerformanceEventSource Log = new();

  private readonly EventCounter _fps;

  private GamePerformanceEventSource()
  {
    _fps = new EventCounter("fps", this)
    {
      DisplayName = "Frames Per Second",
      DisplayUnits = "fps"
    };
  }

  public void ReportFps(float framesPerSecond)
  {
    _fps.WriteMetric(framesPerSecond);
  }
}
