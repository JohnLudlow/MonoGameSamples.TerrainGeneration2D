using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

/// <summary>
/// EventListener that writes TerrainPerformanceEventSource events to the console
/// </summary>
public sealed class ConsoleEventListener : EventListener
{
  protected override void OnEventSourceCreated(EventSource eventSource)
  {
    if (eventSource.Name == "JohnLudlow.TerrainGeneration2D.Performance")
    {
      EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string?>
      {
        ["EventCounterIntervalSec"] = "1"
      });
    }
  }

  protected override void OnEventWritten(EventWrittenEventArgs eventData)
  {
    if (eventData.EventName == "EventCounters")
    {
      for (var i = 0; i < eventData.Payload?.Count; i++)
      {
        if (eventData.Payload[i] is IDictionary<string, object> eventPayload)
        {
          var counterName = eventPayload.TryGetValue("Name", out var nameObj) ? nameObj?.ToString() : "unknown";
          object? counterValue = null;

          if (eventPayload.TryGetValue("Mean", out var meanObj))
          {
            counterValue = meanObj;
          }
          else if (eventPayload.TryGetValue("Increment", out var incObj))
          {
            counterValue = incObj;
          }

          if (counterValue != null)
          {
            Console.WriteLine($"[COUNTER] {counterName}: {counterValue}");
          }
        }
      }
    }
    else
    {
      var message = eventData.Message;
      if (message != null && eventData.Payload != null && eventData.Payload.Count > 0)
      {
        message = string.Format(message, eventData.Payload.ToArray());
      }

      var level = eventData.Level switch
      {
        EventLevel.Verbose => "VERBOSE",
        EventLevel.Informational => "INFO",
        EventLevel.Warning => "WARN",
        EventLevel.Error => "ERROR",
        EventLevel.Critical => "CRITICAL",
        _ => "LOG"
      };

      Console.WriteLine($"[{level}] {eventData.EventName}: {message}");
    }
  }
}