using System;
using System.Diagnostics.Tracing;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

[EventSource(Name = "JohnLudlow.TerrainGeneration2D.Performance")]
public sealed class TerrainPerformanceEventSource : EventSource
{
  public static TerrainPerformanceEventSource Log { get; } = new();

  private readonly EventCounter _activeChunkCounter;
  private readonly IncrementingEventCounter _chunksSavedCounter;
  private readonly EventCounter _wfcShortlistCounter;
  private bool _disposed;

  private TerrainPerformanceEventSource()
  {
    _activeChunkCounter = new EventCounter("active-chunk-count", this)
    {
      DisplayName = "Active Chunks",
      DisplayUnits = "chunks"
    };
    _chunksSavedCounter = new IncrementingEventCounter("chunks-saved-per-second", this)
    {
      DisplayName = "Chunks Saved/sec",
      DisplayUnits = "chunks/s",
      DisplayRateTimeScale = TimeSpan.FromSeconds(1)
    };
    _wfcShortlistCounter = new EventCounter("wfc-shortlist-size", this)
    {
      DisplayName = "WFC Shortlist Size",
      DisplayUnits = "cells"
    };
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && !_disposed)
    {
      _activeChunkCounter.Dispose();
      _chunksSavedCounter.Dispose();
      _wfcShortlistCounter.Dispose();
      _disposed = true;
    }

    base.Dispose(disposing);
  }

  // ID allocation: reserve 10–99 for this EventSource. IDs 1–9 are reserved by EventSource/Counters.
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

  public void ReportWfcShortlistSize(int count)
  {
    if (_disposed)
    {
      return;
    }
    _wfcShortlistCounter.WriteMetric(count);
  }

  [Event(16, Level = EventLevel.Verbose, Message = "WFC begin chunk {0},{1}")]
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
    if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
    {
      WriteEvent(17, chunkX, chunkY, success);
    }
  }

  // Backtracking & WFC detailed diagnostics (IDs >= 18)
  [Event(18, Level = EventLevel.Verbose, Message = "WFC decision push depth={0} at {1},{2} candidates={3}")]
  public void WfcDecisionPush(int depth, int x, int y, int candidateCount)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(18, depth, x, y, candidateCount);
    }
  }

  [Event(19, Level = EventLevel.Verbose, Message = "WFC apply choice depth={0} at {1},{2} tile={3}")]
  public void WfcApplyChoice(int depth, int x, int y, int tileId)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(19, depth, x, y, tileId);
    }
  }

  [Event(20, Level = EventLevel.Warning, Message = "WFC contradiction depth={0} at {1},{2}")]
  public void WfcContradiction(int depth, int x, int y)
  {
    if (IsEnabled(EventLevel.Warning, EventKeywords.None))
    {
      WriteEvent(20, depth, x, y);
    }
  }

  [Event(21, Level = EventLevel.Verbose, Message = "WFC rollback begin depth={0} mark={1}")]
  public void WfcRollbackBegin(int depth, int mark)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(21, depth, mark);
    }
  }

  [Event(22, Level = EventLevel.Verbose, Message = "WFC rollback end depth={0}")]
  public void WfcRollbackEnd(int depth)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(22, depth);
    }
  }

  [Event(23, Level = EventLevel.Verbose, Message = "WFC decision pop depth={0}")]
  public void WfcDecisionPop(int depth)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(23, depth);
    }
  }

  [Event(24, Level = EventLevel.Verbose, Message = "WFC stats decisions={0} backtracks={1} maxDepth={2}")]
  public void WfcStats(int decisions, int backtracks, int maxDepth)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(24, decisions, backtracks, maxDepth);
    }
  }

  [Event(25, Level = EventLevel.Verbose, Message = "WFC influence tie-break applied, remaining={0}")]
  public void WfcTieBreakInfluenceApplied(int remaining)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(25, remaining);
    }
  }

  [Event(26, Level = EventLevel.Verbose, Message = "WFC central tie-break applied, remaining={0}")]
  public void WfcTieBreakCentralApplied(int remaining)
  {
    if (IsEnabled(EventLevel.Informational, EventKeywords.None))
    {
      WriteEvent(26, remaining);
    }
  }
}