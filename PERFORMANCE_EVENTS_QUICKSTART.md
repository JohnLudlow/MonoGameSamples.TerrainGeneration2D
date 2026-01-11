# Quick Start: Viewing Performance Events

## Immediate Verification (Console Output)

1. **Run the game in DEBUG mode**:

   ```bash
   dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj --configuration Debug
   ```

2. **You should immediately see event output** in the console:

   ```plain
   [INFO] UpdateActiveChunksBegin: UpdateActiveChunks bounds 0,0 -> 2,2
   [INFO] ChunkLoadBegin: Chunk load begin 0,0
   [INFO] WaveFunctionCollapseBegin: WFC begin chunk 0,0
   [INFO] WaveFunctionCollapseEnd: WFC end chunk 0,0 success=True
   [INFO] UpdateActiveChunksEnd: UpdateActiveChunks ended 0,0 -> 2,2 count 9
   [COUNTER] active-chunk-count: 9
   ```

## No Events Appearing?

If you don't see events, check:

1. **Verify DEBUG mode**:

   ```bash
   dotnet build --configuration Debug
   dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj --configuration Debug
   ```

2. **Check for event ID errors in PerfView**:
   - Look for messages like `"Event X has ID Y which is already in use"`
   - This indicates event ID conflicts with EventSource infrastructure
   - Solution: Event IDs should be 10 or higher (already fixed in this project)

3. **Manually enable in RELEASE mode** (edit `TerrainGenerationGame.cs`):

   ```csharp
   public TerrainGenerationGame() : base("Dungeon Slime", 1280, 720, false)
   {
       EnablePerformanceDiagnostics = true;  // Force enable
   }
   ```

4. **Verify the listener is created** - Add breakpoint in `Core.Initialize()`:

   ```csharp
   if (EnablePerformanceDiagnostics)
   {
       _eventListener = new ConsoleEventListener();  // <-- Breakpoint here
   }
   ```

5. **Check console output is visible**:
   - In Visual Studio: View → Output → Show output from: Debug
   - In VS Code: Terminal should show stdout
   - Command line: Events write directly to console

## Production Monitoring (No Code Changes)

Use `dotnet-counters` to monitor a running process:

```bash
# Install the tool (once)
dotnet tool install --global dotnet-counters

# Find your process ID
dotnet-counters ps

# Monitor performance counters
dotnet-counters monitor --process-id <PID> JohnLudlow.TerrainGeneration2D.Performance
```

Output:

```plain
[JohnLudlow.TerrainGeneration2D.Performance]
    active-chunk-count                        9
    chunks-saved-per-second (Count / 1 sec)   1.5
```

## Event Capture for Analysis

Capture all events to a trace file:

```bash
# Install the tool (once)
dotnet tool install --global dotnet-trace

# Capture events
dotnet-trace collect --process-id <PID> --providers JohnLudlow.TerrainGeneration2D.Performance

# View the trace
dotnet-trace report trace.nettrace
```

## Testing

Verify events are working with unit tests:

```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj --filter "EventSourceTests"
```

All tests should pass, confirming the EventSource is properly emitting events.

## Understanding the Events

| Event | When It Fires | What It Tells You |
|-------|--------------|-------------------|
| `UpdateActiveChunksBegin/End` | Every frame when camera moves | Chunk loading/unloading activity |
| `ChunkLoadBegin/End` | When loading from disk | Cache hit rate (success=true means found on disk) |
| `ChunkSaveBegin/End` | When chunks scroll out of view | Disk write activity |
| `WaveFunctionCollapseBegin/End` | When generating new chunks | WFC success rate and performance |
| `active-chunk-count` (counter) | Every second | Memory usage (chunks * 64KB) |
| `chunks-saved-per-second` (counter) | Every second | Disk I/O rate |

## Performance Tips

- **High chunk load failures?** → Increase chunk buffer or pre-generate terrain
- **High WFC failures?** → Relax tile rules or increase max iterations
- **High active-chunk-count?** → Reduce camera buffer or implement aggressive chunk unloading
- **High chunks-saved rate?** → Camera moving too fast, consider batch saves

## Disabling in Production

Remove or comment out in `TerrainGenerationGame.cs`:

```csharp
public TerrainGenerationGame() : base("Dungeon Slime", 1280, 720, false)
{
    // EnablePerformanceDiagnostics = true;  // Disabled
}
```

Or build in RELEASE mode (auto-disabled):

```bash
dotnet build --configuration Release
```

## Integration with Monitoring Services

The EventSource works with:

- **Application Insights** (Azure)
- **OpenTelemetry**
- **PerfView** (Windows)
- **Custom EventListener** implementations

See `TerrainGeneration2D.Core/Diagnostics/README.md` for integration examples.
