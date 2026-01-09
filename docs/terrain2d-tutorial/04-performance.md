# Phase 04 - Performance investigations and counters

In this phase you will:

- Add a minimal .NET `EventSource` for telemetry (standalone)
- Emit a live FPS counter from the game’s `Draw`
- View counters with `dotnet-counters`

## 1. EventSource (Core)

Create a minimal performance event source with an FPS counter.

`TerrainGeneration2D.Core/Diagnostics/GamePerformanceEventSource.cs`:

```csharp
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
```

## 2. Enable a console listener (Game)

Add a console listener during development to see events in the console:

```csharp
using System.Diagnostics.Tracing;

class ConsoleEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "MyGame.Performance")
        {
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, new Dictionary<string,string?>
            {
                ["EventCounterIntervalSec"] = "1"
            });
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName == "EventCounters") return;
        Console.WriteLine($"[{eventData.Level}] {eventData.EventName} {eventData.Message}");
    }
}
```

Create and keep an instance during debug builds:

```csharp
#if DEBUG
private ConsoleEventListener _listener = new();
#endif
```

## 3. Emit FPS from Draw

From your game class, report FPS each frame. A simple instantaneous value works well with `EventCounter` since tooling shows averages over the interval.

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

protected override void Draw(GameTime gameTime)
{
    // ... your drawing code ...

    float fps = 1f / (float)gameTime.ElapsedGameTime.TotalSeconds;
    GamePerformanceEventSource.Log.ReportFps(fps);

    base.Draw(gameTime);
}
```

## 4. View counters

- Run the game
- In another shell, monitor the provider:

```bash
dotnet-counters monitor --process-id <PID> --counters MyGame.Performance
```

You will see `fps` reported once per second.

## Notes

- Keep custom event IDs >= 10 to avoid EventSource conflicts.
- Counters are emitted when listeners pass `EventCounterIntervalSec` in `EnableEvents`.

## 5. Try It

- Create the file shown in [TerrainGeneration2D.Core/Diagnostics/GamePerformanceEventSource.cs](../../TerrainGeneration2D.Core/Diagnostics/GamePerformanceEventSource.cs) and add the `Draw()` snippet in your game class to report FPS.
- Build and run the game:

```pwsh
dotnet build src/Terrain2D.sln
dotnet run --project src/TerrainGeneration2D/TerrainGeneration2D.csproj
```

- In a second terminal, monitor the provider by name:

```pwsh
dotnet-counters monitor --name TerrainGeneration2D --counters MyGame.Performance
```

You should see the `fps` counter update every second.

## Tests (TDD)

Create `TerrainGeneration2D.Tests/PerformanceTests.cs`:

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

namespace TerrainGeneration2D.Tests;

public class PerformanceTests
{
    [Fact]
    public void GamePerformanceEventSource_ReportFps_DoesNotThrow()
    {
        GamePerformanceEventSource.Log.ReportFps(60f);
        Assert.True(true);
    }
}
```

Notes:

- Full listener tests require an `EventListener` and are integration-level; here we only assert that reporting a counter does not throw.

## See also

- Previous phase: [03 — Logging](03-logging.md)
- Next phase: [05 — Random tiles](05-random-tiles.md)
- Tutorial index: [README.md](README.md)
