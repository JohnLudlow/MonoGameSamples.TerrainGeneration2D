# Event ID Conflict Resolution

## Issue
When running the game, PerfView showed this error:
```
ERROR: Exception in Command Processing for EventSource JohnLudlow.TerrainGeneration2D.Performance: 
Event ChunkLoadBegin has ID 3 which is already in use.
```

## Root Cause
EventSource reserves certain event IDs for internal use:

| ID Range | Purpose | Reserved By |
|----------|---------|-------------|
| 1-2 | Manifest and metadata events | EventSource infrastructure |
| 1-9 | Counter aggregation and reporting | EventCounter/IncrementingEventCounter |
| 10+ | **Safe for custom events** | Your code |

Our original implementation used IDs 1-8, which conflicted with the infrastructure.

## Solution
Changed all event IDs to start from 10:

```csharp
// BEFORE (BROKEN)
[Event(1, Level = EventLevel.Informational, Message = "...")]
public void UpdateActiveChunksBegin(...) 
{
    WriteEvent(1, ...);
}

[Event(3, Level = EventLevel.Informational, Message = "...")]
public void ChunkLoadBegin(...)
{
    WriteEvent(3, ...);  // ERROR: ID 3 conflicts!
}

// AFTER (FIXED)
[Event(10, Level = EventLevel.Informational, Message = "...")]
public void UpdateActiveChunksBegin(...)
{
    WriteEvent(10, ...);
}

[Event(12, Level = EventLevel.Informational, Message = "...")]
public void ChunkLoadBegin(...)
{
    WriteEvent(12, ...);  // No conflict
}
```

## Event ID Mapping

| Method | Old ID | New ID | Status |
|--------|--------|--------|--------|
| UpdateActiveChunksBegin | 1 | 10 | ✅ Fixed |
| UpdateActiveChunksEnd | 2 | 11 | ✅ Fixed |
| ChunkLoadBegin | 3 | 12 | ✅ Fixed |
| ChunkLoadEnd | 4 | 13 | ✅ Fixed |
| ChunkSaveBegin | 5 | 14 | ✅ Fixed |
| ChunkSaveEnd | 6 | 15 | ✅ Fixed |
| WaveFunctionCollapseBegin | 7 | 16 | ✅ Fixed |
| WaveFunctionCollapseEnd | 8 | 17 | ✅ Fixed |

## Verification

### Before Fix
```
# PerfView shows:
ERROR: Event ChunkLoadBegin has ID 3 which is already in use.

# No events appear in console
# No counters appear in dotnet-counters
```

### After Fix
```
# PerfView shows no errors
# Console shows:
[INFO] UpdateActiveChunksBegin: UpdateActiveChunks bounds 0,0 -> 2,2
[INFO] ChunkLoadBegin: Chunk load begin 0,0
[COUNTER] active-chunk-count: 9

# dotnet-counters shows:
[JohnLudlow.TerrainGeneration2D.Performance]
    active-chunk-count                        9
    chunks-saved-per-second (Count / 1 sec)   1.5
```

## Best Practices

1. **Always start custom event IDs at 10 or higher**
2. **Never use IDs 1-9** (reserved by infrastructure)
3. **Use sequential IDs** for maintainability (10, 11, 12, ...)
4. **Document your event ID range** in comments
5. **Test with PerfView** to catch conflicts early

## Testing

Run this to verify no event ID conflicts:
```bash
# Start the game
dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj

# In another terminal, capture with dotnet-trace
dotnet-trace collect --process-id <PID> --providers JohnLudlow.TerrainGeneration2D.Performance

# Check for errors in trace
dotnet-trace report trace.nettrace | grep ERROR
```

Should show **no errors**.

## References
- [EventSource Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource)
- [EventCounter Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventcounter)
- Event ID reservation is mentioned in EventSource best practices but not always obvious

## Impact
- **Before**: Zero events emitted, counters non-functional, silent failures
- **After**: All events emit correctly, counters work in dotnet-counters and Visual Studio
