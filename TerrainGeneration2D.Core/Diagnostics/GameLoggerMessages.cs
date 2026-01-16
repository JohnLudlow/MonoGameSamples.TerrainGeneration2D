using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Diagnostics;

/// <summary>
/// Source-generated logging message templates for high-throughput paths.
/// ID ranges:
/// - 1000–1599: MonoGame pipeline lifecycle
/// - 1600–1699: Scene lifecycle (per-scene update/draw)
/// - 2200–2299: Tile render system
/// - 3000–3099: Mapping system
/// </summary>
public static partial class GameLoggerMessages
{
  #region MonoGame pipeline messages (1000–1599)
  [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "[{contextMemberName}] Init started ({pid})", EventName = "MonoGame Initialize Begin")]
  public static partial void MonoGameInitBegin(ILogger logger, int pid, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "[{contextMemberName}] Initialize window {width}x{height}")]
  public static partial void MonoGameInitWindow(ILogger logger, int width, int height, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1099, Level = LogLevel.Information, Message = "[{contextMemberName}] Init ended")]
  public static partial void MonoGameInitEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1200, Level = LogLevel.Information, Message = "[{contextMemberName}] LoadContent started")]
  public static partial void MonoGameLoadContentBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1299, Level = LogLevel.Information, Message = "[{contextMemberName}] LoadContent ended")]
  public static partial void MonoGameLoadContentEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1300, Level = LogLevel.Information, Message = "[{contextMemberName}] Update started")]
  public static partial void MonoGameUpdateBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1399, Level = LogLevel.Debug, Message = "[{contextMemberName}] Update ended")]
  public static partial void MonoGameUpdateEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1400, Level = LogLevel.Debug, Message = "[{contextMemberName}] Draw started")]
  public static partial void MonoGameDrawBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1499, Level = LogLevel.Debug, Message = "[{contextMemberName}] Draw ended")]
  public static partial void MonoGameDrawEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1500, Level = LogLevel.Debug, Message = "[{contextMemberName}] Exit started")]
  public static partial void MonoGameExitBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1599, Level = LogLevel.Debug, Message = "[{contextMemberName}] Exit ended")]
  public static partial void MonoGameExitEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);
  #endregion

  #region Scene messages (1600–1699)
  [LoggerMessage(EventId = 1600, Level = LogLevel.Debug, Message = "[{contextMemberName}] Scene Update started")]
  public static partial void SceneUpdateBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1609, Level = LogLevel.Debug, Message = "[{contextMemberName}] Scene Update ended")]
  public static partial void SceneUpdateEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1610, Level = LogLevel.Debug, Message = "[{contextMemberName}] Scene Draw started")]
  public static partial void SceneDrawBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 1619, Level = LogLevel.Debug, Message = "[{contextMemberName}] Scene Draw ended")]
  public static partial void SceneDrawEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);
  #endregion

  #region Tile render system messages (2200–2299)
  [LoggerMessage(EventId = 2201, Level = LogLevel.Information, Message = "[{contextMemberName}] Loading tileset started")]
  public static partial void LoadTilesetBegin(ILogger logger, [CallerMemberName] string? contextMemberName = null);

  [LoggerMessage(EventId = 2209, Level = LogLevel.Information, Message = "[{contextMemberName}] Loading tileset ended")]
  public static partial void LoadTilesetEnd(ILogger logger, [CallerMemberName] string? contextMemberName = null);
  #endregion

  #region Mapping system messages (3000–3099)
  [LoggerMessage(EventId = 3000, Level = LogLevel.Information, Message = "Begin generate {width}x{height}")]
  public static partial void MapGenerateBegin(ILogger logger, int width, int height);

  [LoggerMessage(EventId = 3090, Level = LogLevel.Information, Message = "End generate success={success}")]
  public static partial void MapGenerateEnd(ILogger logger, bool success);
  #endregion
}