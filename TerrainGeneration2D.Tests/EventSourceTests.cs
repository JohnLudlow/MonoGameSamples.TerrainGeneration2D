using System.Diagnostics.Tracing;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

public sealed class EventSourceTests : IDisposable
{
    private readonly TestEventListener _listener;

    public EventSourceTests()
    {
        _listener = new TestEventListener();
        System.Threading.Thread.Sleep(50);
    }

    public void Dispose()
    {
        _listener.Dispose();
    }

    [Fact]
    public void EventSource_IsCreated()
    {
        Assert.NotNull(TerrainPerformanceEventSource.Log);
        Assert.Equal("JohnLudlow.TerrainGeneration2D.Performance", TerrainPerformanceEventSource.Log.Name);
    }

    [Fact]
    public void EventSource_ChunkLoadEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.ChunkLoadBegin(1, 2);
        TerrainPerformanceEventSource.Log.ChunkLoadEnd(1, 2, true);
        var events = _listener.Snapshot().Where(e => e != null).ToList();
        Assert.True(events.Count >= 2, $"Expected at least 2 events, got {events.Count}");
        Assert.Contains(events, e => e.EventName == "ChunkLoadBegin");
        Assert.Contains(events, e => e.EventName == "ChunkLoadEnd");
    }

    [Fact]
    public void EventSource_ChunkSaveEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.ChunkSaveBegin(3, 4);
        TerrainPerformanceEventSource.Log.ChunkSaveEnd(3, 4, true);
        TerrainPerformanceEventSource.Log.ChunkSaved();

        var events = _listener.Snapshot().Where(e => e != null).ToList();
        Assert.True(events.Count >= 2, $"Expected at least 2 events, got {events.Count}");
        Assert.Contains(events, e => e.EventName == "ChunkSaveBegin");
        Assert.Contains(events, e => e.EventName == "ChunkSaveEnd");
    }

    [Fact]
    public void EventSource_UpdateActiveChunksEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.UpdateActiveChunksBegin(0, 0, 2, 2);
        TerrainPerformanceEventSource.Log.ReportActiveChunkCount(9);
        TerrainPerformanceEventSource.Log.UpdateActiveChunksEnd(0, 0, 2, 2, 9);

        var events = _listener.Snapshot().Where(e => e != null).ToList();
        Assert.True(events.Count >= 2, $"Expected at least 2 events, got {events.Count}");
        Assert.Contains(events, e => e.EventName == "UpdateActiveChunksBegin");
        Assert.Contains(events, e => e.EventName == "UpdateActiveChunksEnd");
    }

    [Fact]
    public void EventSource_WFCEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(5, 6);
        TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(5, 6, true);

        var events = _listener.Snapshot().Where(e => e != null).ToList();
        Assert.True(events.Count >= 2, $"Expected at least 2 events, got {events.Count}");
        Assert.Contains(events, e => e.EventName == "WaveFunctionCollapseBegin");
        Assert.Contains(events, e => e.EventName == "WaveFunctionCollapseEnd");
    }

    [Fact(Skip = "Flaky test")]
    public void EventSource_EventMessages_ContainCorrectData()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.ChunkLoadBegin(10, 20);
        
        var eventsSnapshot = _listener.Snapshot();
        var evt = eventsSnapshot.FirstOrDefault(e => e != null && e.EventName == "ChunkLoadBegin");
        Assert.NotNull(evt);
        Assert.NotNull(evt.Payload);
        Assert.Equal(2, evt.Payload.Count);
        Assert.Equal(10, evt.Payload[0]);
        Assert.Equal(20, evt.Payload[1]);
    }

    private sealed class TestEventListener : EventListener
    {
        public List<EventWrittenEventArgs> Events { get; } = new();
        private readonly object _sync = new();
        private bool _enabled;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "JohnLudlow.TerrainGeneration2D.Performance")
            {
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
                _enabled = true;
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (_enabled && eventData.EventName != "EventCounters")
            {
                lock (_sync)
                {
                    Events.Add(eventData);
                }
            }
        }

        public void ClearEvents()
        {
            lock (_sync)
            {
                Events.Clear();
            }
        }

        public List<EventWrittenEventArgs> Snapshot()
        {
            lock (_sync)
            {
                return Events.ToList();
            }
        }
    }
}
