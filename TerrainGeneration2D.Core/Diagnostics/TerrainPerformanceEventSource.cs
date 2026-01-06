using System.Diagnostics.Tracing;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

[EventSource(Name = "JohnLudlow.TerrainGeneration2D.Performance")]
public sealed class TerrainPerformanceEventSource : EventSource
{
    public static TerrainPerformanceEventSource Log { get; } = new();

    private readonly EventCounter _activeChunkCounter;
    private readonly IncrementingEventCounter _chunksSavedCounter;
    private bool _disposed;

    private TerrainPerformanceEventSource()
    {
        _activeChunkCounter = new EventCounter("active-chunk-count", this);
        _chunksSavedCounter = new IncrementingEventCounter("chunks-saved-per-second", this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _activeChunkCounter.Dispose();
            _chunksSavedCounter.Dispose();
            _disposed = true;
        }

        base.Dispose(disposing);
    }

    [Event(10, Level = EventLevel.Informational, Message = "UpdateActiveChunks bounds {0},{1} -> {2},{3}")]
    public void UpdateActiveChunksBegin(int minChunkX, int minChunkY, int maxChunkX, int maxChunkY)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(10, minChunkX, minChunkY, maxChunkX, maxChunkY);
        }
    }

    [Event(11, Level = EventLevel.Verbose, Message = "UpdateActiveChunks ended {0},{1} -> {2},{3} count {4}")]
    public void UpdateActiveChunksEnd(int minChunkX, int minChunkY, int maxChunkX, int maxChunkY, int activeChunkCount)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            WriteEvent(11, minChunkX, minChunkY, maxChunkX, maxChunkY, activeChunkCount);
        }
    }

    public void ReportActiveChunkCount(int count)
    {
        if (_disposed)
        {
            return;
        }

        _activeChunkCounter.WriteMetric(count);
    }

    [Event(12, Level = EventLevel.Informational, Message = "Chunk load begin {0},{1}")]
    public void ChunkLoadBegin(int chunkX, int chunkY)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(12, chunkX, chunkY);
        }
    }

    [Event(13, Level = EventLevel.Informational, Message = "Chunk load end {0},{1} success={2}")]
    public void ChunkLoadEnd(int chunkX, int chunkY, bool success)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(13, chunkX, chunkY, success);
        }
    }

    [Event(14, Level = EventLevel.Informational, Message = "Chunk save begin {0},{1}")]
    public void ChunkSaveBegin(int chunkX, int chunkY)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(14, chunkX, chunkY);
        }
    }

    [Event(15, Level = EventLevel.Informational, Message = "Chunk save end {0},{1} success={2}")]
    public void ChunkSaveEnd(int chunkX, int chunkY, bool success)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(15, chunkX, chunkY, success);
        }
    }

    public void ChunkSaved()
    {
        if (_disposed)
        {
            return;
        }

        _chunksSavedCounter.Increment();
    }

    [Event(16, Level = EventLevel.Informational, Message = "WFC begin chunk {0},{1}")]
    public void WaveFunctionCollapseBegin(int chunkX, int chunkY)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(16, chunkX, chunkY);
        }
    }

    [Event(17, Level = EventLevel.Informational, Message = "WFC end chunk {0},{1} success={2}")]
    public void WaveFunctionCollapseEnd(int chunkX, int chunkY, bool success)
    {
        if (IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            WriteEvent(17, chunkX, chunkY, success);
        }
    }
}
