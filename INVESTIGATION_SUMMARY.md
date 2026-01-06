# Investigation Summary: TerrainPerformanceEventSource

## Problem
The `TerrainPerformanceEventSource` was implemented but events were not appearing during runtime.

## Root Cause
1. **.NET EventSource events only emit when actively listened to.** By default, EventSource operates in a "no-op" mode unless:
   - An `EventListener` is attached and enabled for that source
   - ETW (Event Tracing for Windows) is actively collecting events
   - A monitoring tool (dotnet-trace, dotnet-counters, PerfView) is capturing events

2. **Event ID conflicts with EventSource infrastructure.** The original implementation used event IDs 1-8, which conflicted with:
   - IDs 1-2: Reserved by EventSource for manifest and metadata
   - IDs 1-9: May be used by EventCounter/IncrementingEventCounter infrastructure
   
   This caused the error: `"Event ChunkLoadBegin has ID 3 which is already in use"`

The event source was correctly implemented and being called, but **no listener was attached AND event IDs were conflicting**, so events were failing silently.

## Solution Implemented

### 1. Created ConsoleEventListener
**File**: `TerrainGeneration2D.Core/Diagnostics/ConsoleEventListener.cs`

A simple EventListener that:
- Auto-discovers and enables the `TerrainPerformanceEventSource`
- Writes all events to the console with formatted output
- Displays event counters every second
- Filters out unnecessary EventCounter metadata events

### 2. Fixed Event ID Conflicts
**File**: `TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs`

Changed event IDs from 1-8 to 10-17 to avoid conflicts:
- Event IDs 1-2 are reserved by EventSource infrastructure
- Event IDs 1-9 may be used by EventCounter infrastructure
- Event IDs 10+ are safe for custom events

### 3. Integrated into Core Game
**File**: `TerrainGeneration2D.Core/Core.cs`

Added optional performance diagnostics:
```csharp
public bool EnablePerformanceDiagnostics { get; set; }
private ConsoleEventListener? _eventListener;
```

The listener is created during `Initialize()` when enabled and disposed properly.

### 4. Enabled in Debug Builds
**File**: `TerrainGeneration2D/TerrainGenerationGame.cs`

Automatically enables diagnostics in DEBUG builds:
```csharp
#if DEBUG
    EnablePerformanceDiagnostics = true;
#endif
```

### 5. Created Documentation
**File**: `TerrainGeneration2D.Core/Diagnostics/README.md`

Comprehensive guide covering:
- Event categories and counters
- Multiple ways to view events (console, dotnet-counters, dotnet-trace, PerfView)
- Integration examples
- Troubleshooting guide including event ID conflict resolution

### 6. Added Unit Tests
**File**: `TerrainGeneration2D.Tests/EventSourceTests.cs`

Tests verify:
- EventSource is properly created
- All event methods emit correctly
- Event payloads contain expected data

## Verification

The event source is already instrumented in:
- `ChunkedTilemap.cs`: Chunk load/save operations, UpdateActiveChunks
- `WaveFunctionCollapse.cs`: WFC generation begin/end tracking

With the `ConsoleEventListener` enabled, you'll now see output like:

```
[INFO] UpdateActiveChunksBegin: UpdateActiveChunks bounds 0,0 -> 2,2
[INFO] ChunkLoadBegin: Chunk load begin 0,0
[INFO] ChunkLoadEnd: Chunk load end 0,0 success=False
[INFO] WaveFunctionCollapseBegin: WFC begin chunk 0,0
[INFO] WaveFunctionCollapseEnd: WFC end chunk 0,0 success=True
[COUNTER] active-chunk-count: 9
[INFO] ChunkSaveBegin: Chunk save begin 1,1
[INFO] ChunkSaveEnd: Chunk save end 1,1 success=True
[COUNTER] chunks-saved-per-second: 1.5
```

## Usage

### Development (Console Output)
Events automatically appear in console output when running DEBUG builds:
```bash
dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj
```

### Production Monitoring
Use dotnet-counters for live performance metrics:
```bash
dotnet-counters monitor --process-id <PID> JohnLudlow.TerrainGeneration2D.Performance
```

### Event Capture
Use dotnet-trace to capture events to a file for later analysis:
```bash
dotnet-trace collect --process-id <PID> --providers JohnLudlow.TerrainGeneration2D.Performance
```

## Key Takeaways

1. **EventSource requires active listeners** - They don't automatically log anywhere
2. **Zero overhead when disabled** - The `IsEnabled()` checks ensure no performance impact
3. **Multiple consumption options** - Console, dotnet-counters, dotnet-trace, PerfView, OpenTelemetry
4. **Production-ready** - Can be enabled/disabled at runtime without code changes

## Files Modified/Created

### Created:
- `TerrainGeneration2D.Core/Diagnostics/ConsoleEventListener.cs`
- `TerrainGeneration2D.Core/Diagnostics/README.md`
- `TerrainGeneration2D.Tests/EventSourceTests.cs`
- `INVESTIGATION_SUMMARY.md` (this file)

### Modified:
- `TerrainGeneration2D.Core/Core.cs` - Added EventListener integration
- `TerrainGeneration2D/TerrainGenerationGame.cs` - Enabled diagnostics in DEBUG mode

### Already Instrumented (No Changes):
- `TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs`
- `TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse.cs`
- `TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs`
