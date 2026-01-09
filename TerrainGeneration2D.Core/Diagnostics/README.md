# TerrainGeneration2D Performance Diagnostics

## Overview

The `TerrainPerformanceEventSource` provides performance telemetry for the terrain generation system using .NET EventSource infrastructure. This allows monitoring of chunk operations, WFC generation, and system metrics.

## Event Categories

### Trace Events

- **UpdateActiveChunksBegin/End**: Tracks chunk activation/deactivation
- **ChunkLoadBegin/End**: Monitors chunk loading from disk
- **ChunkSaveBegin/End**: Monitors chunk saving to disk  
- **WaveFunctionCollapseBegin/End**: Tracks WFC algorithm execution

### Performance Counters

- **active-chunk-count**: Current number of loaded chunks in memory
- **chunks-saved-per-second**: Rate of chunk persistence operations

## Viewing Events

### Option 1: Console Listener (Development)

Add the `ConsoleEventListener` to your game initialization:

```csharp
// In TerrainGenerationGame.cs or Core.cs
private ConsoleEventListener? _eventListener;

protected override void Initialize()
{
    _eventListener = new ConsoleEventListener();
    base.Initialize();
    // ... rest of initialization
}

protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        _eventListener?.Dispose();
    }
    base.Dispose(disposing);
}
```

This will write all events and counters to the console output.

### Option 2: dotnet-counters (Production Monitoring)

Install the tool:

```bash
dotnet tool install --global dotnet-counters
```

Monitor the running game:

```bash
dotnet-counters monitor --process-id <PID> JohnLudlow.TerrainGeneration2D.Performance
```

List available counters:

```bash
dotnet-counters list --process-id <PID>
```

### Option 3: dotnet-trace (Event Capture)

Install the tool:

```bash
dotnet tool install --global dotnet-trace
```

Capture events to a file:

```bash
dotnet-trace collect --process-id <PID> --providers JohnLudlow.TerrainGeneration2D.Performance
```

View the trace file:

```bash
dotnet-trace report trace.nettrace
```

### Option 4: PerfView (Windows Advanced)

1. Download [PerfView](https://github.com/microsoft/perfview)
2. Run `PerfView.exe /onlyProviders=*JohnLudlow.TerrainGeneration2D.Performance collect`
3. Start your game
4. Stop collection in PerfView
5. View events in the PerfView UI

## Example Output

With `ConsoleEventListener` enabled, you'll see output like:

```plain
[INFO] UpdateActiveChunksBegin: UpdateActiveChunks bounds 0,0 -> 2,2
[INFO] ChunkLoadBegin: Chunk load begin 0,0
[INFO] ChunkLoadEnd: Chunk load end 0,0 success=False
[INFO] WaveFunctionCollapseBegin: WFC begin chunk 0,0
[INFO] WaveFunctionCollapseEnd: WFC end chunk 0,0 success=True
[INFO] UpdateActiveChunksEnd: UpdateActiveChunks ended 0,0 -> 2,2 count 9
[COUNTER] active-chunk-count: 9
[INFO] ChunkSaveBegin: Chunk save begin 0,0
[INFO] ChunkSaveEnd: Chunk save end 0,0 success=True
[COUNTER] chunks-saved-per-second: 1.5
```

## Custom EventListener

Create a custom listener for specific needs:

```csharp
public class MyCustomListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "JohnLudlow.TerrainGeneration2D.Performance")
        {
            // Enable only informational events, no verbose
            EnableEvents(eventSource, EventLevel.Informational, EventKeywords.All);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        // Custom processing - log to file, send to telemetry service, etc.
    }
}
```

## Integration with Application Insights / OpenTelemetry

The EventSource can be integrated with modern observability platforms:

```csharp
// Example with OpenTelemetry
using OpenTelemetry.Trace;

services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddSource("JohnLudlow.TerrainGeneration2D.Performance")
        .AddConsoleExporter());
```

## Performance Impact

EventSource has minimal overhead when:

- Events are disabled (default state)
- No listeners are attached
- Events are enabled but filtered by level

The `IsEnabled()` checks in each event method ensure zero allocation when events are not being captured.

## Troubleshooting

### Events not appearing?

- Ensure an `EventListener` is created and enables the event source
- Check that `EventLevel` is set appropriately (Verbose, Informational, etc.)
- Verify the event source name matches exactly: `"JohnLudlow.TerrainGeneration2D.Performance"`

### Counters showing zeros?

- Counters update based on `EventCounterIntervalSec` setting (default 1 second)
- Ensure events that increment counters are actually being called
- Check that `EnableEvents` includes the `EventCounterIntervalSec` argument

### Too much output?

- Reduce `EventLevel` from `Verbose` to `Informational`
- Filter specific events in your listener's `OnEventWritten` method
- Disable the listener when not actively debugging

### ERROR: Event has ID X which is already in use

- This occurs when event IDs conflict with EventSource infrastructure (IDs 1-2) or EventCounter (IDs 1-9)
- Solution: Use event IDs starting from 10 or higher
- The TerrainPerformanceEventSource uses IDs 10-17 to avoid conflicts

## Technical Notes

### Event ID Ranges

EventSource reserves certain event IDs for internal use:

- **IDs 1-2**: Reserved for EventSource manifest and metadata
- **IDs 1-9**: May be used by EventCounter infrastructure
- **IDs 10+**: Safe for custom events

Our implementation uses IDs 10-17 for trace events to avoid conflicts.

### EventCounter vs WriteEvent

- `EventCounter.WriteMetric()` and `IncrementingEventCounter.Increment()` are **not** WriteEvent calls
- They generate internal events for counter aggregation
- Counters appear in `dotnet-counters` and monitoring tools, not in trace events
- Use `EventCounterIntervalSec` parameter when enabling events to control counter reporting frequency
