namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

/// <summary>
/// Central documentation for logging/event ID ranges to avoid conflicts.
///
/// Reserved ranges:
/// - EventSource (ETW) events: 10–99 for TerrainPerformanceEventSource (counters + perf events)
///   - 10–24 currently in use (active chunks, chunk IO, WFC)
/// - LoggerMessage (Microsoft.Extensions.Logging) events:
///   - 1000–1599: MonoGame pipeline lifecycle
///   - 2200–2299: Tile render system
///   - 3000–3099: Mapping/WFC high-level messages
///
/// Guidelines:
/// - Never use EventSource IDs 1–9 (reserved by EventSource infrastructure and counters)
/// - Keep groups contiguous for easier filtering in sinks/dashboards
/// - Update this file when allocating new ranges
/// </summary>
public static class EventIdRanges { }