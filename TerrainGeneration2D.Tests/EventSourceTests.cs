using System.Diagnostics.Tracing;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public class EventSourceTests : IDisposable
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

        Assert.True(_listener.Events.Count >= 2, $"Expected at least 2 events, got {_listener.Events.Count}");
        Assert.Contains(_listener.Events, e => e.EventName == "ChunkLoadBegin");
        Assert.Contains(_listener.Events, e => e.EventName == "ChunkLoadEnd");
    }

    [Fact]
    public void EventSource_ChunkSaveEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.ChunkSaveBegin(3, 4);
        TerrainPerformanceEventSource.Log.ChunkSaveEnd(3, 4, true);
        TerrainPerformanceEventSource.Log.ChunkSaved();

        Assert.True(_listener.Events.Count >= 2, $"Expected at least 2 events, got {_listener.Events.Count}");
        Assert.Contains(_listener.Events, e => e.EventName == "ChunkSaveBegin");
        Assert.Contains(_listener.Events, e => e.EventName == "ChunkSaveEnd");
    }

    [Fact]
    public void EventSource_UpdateActiveChunksEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.UpdateActiveChunksBegin(0, 0, 2, 2);
        TerrainPerformanceEventSource.Log.ReportActiveChunkCount(9);
        TerrainPerformanceEventSource.Log.UpdateActiveChunksEnd(0, 0, 2, 2, 9);

        Assert.True(_listener.Events.Count >= 2, $"Expected at least 2 events, got {_listener.Events.Count}");
        Assert.Contains(_listener.Events, e => e.EventName == "UpdateActiveChunksBegin");
        Assert.Contains(_listener.Events, e => e.EventName == "UpdateActiveChunksEnd");
    }

    [Fact]
    public void EventSource_WFCEvents_AreEmitted()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.WaveFunctionCollapseBegin(5, 6);
        TerrainPerformanceEventSource.Log.WaveFunctionCollapseEnd(5, 6, true);

        Assert.True(_listener.Events.Count >= 2, $"Expected at least 2 events, got {_listener.Events.Count}");
        Assert.Contains(_listener.Events, e => e.EventName == "WaveFunctionCollapseBegin");
        Assert.Contains(_listener.Events, e => e.EventName == "WaveFunctionCollapseEnd");
    }

    [Fact]
    public void EventSource_EventMessages_ContainCorrectData()
    {
        _listener.ClearEvents();

        TerrainPerformanceEventSource.Log.ChunkLoadBegin(10, 20);
        
        var evt = _listener.Events.FirstOrDefault(e => e.EventName == "ChunkLoadBegin");
        Assert.NotNull(evt);
        Assert.NotNull(evt.Payload);
        Assert.Equal(2, evt.Payload.Count);
        Assert.Equal(10, evt.Payload[0]);
        Assert.Equal(20, evt.Payload[1]);
    }

    private class TestEventListener : EventListener
    {
        public List<EventWrittenEventArgs> Events { get; } = new();
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
                Events.Add(eventData);
            }
        }

        public void ClearEvents()
        {
            Events.Clear();
        }
    }
}
