using System;
using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

/// <summary>
/// Provides spatial information about collapsed tiles, such as contiguous region metrics.
/// </summary>
public sealed class MappingInformationService
{
  private readonly int[][] _output;
  private readonly int _width;
  private readonly int _height;

  public MappingInformationService(int[][] output)
  {
    _output = output ?? throw new ArgumentNullException(nameof(output));
    _width = _output.Length;
    _height = _output.Length > 0 ? output[0].Length : 0;
  }

  /// <summary>
  /// Retrieves metrics for the contiguous region containing the specified coordinate.
  /// </summary>
  public GroupMetrics GetGroupMetrics(TilePoint point, int? assumeTileId = null)
  {
    if (!IsInBounds(point))
    {
      return GroupMetrics.Empty;
    }

    var tileId = assumeTileId ?? _output[point.X][point.Y];
    if (tileId == -1)
    {
      return GroupMetrics.Empty;
    }

    var visited = new bool[_width][];
    // Initialize inner arrays
    for (var x = 0; x < _width; x++)
    {
      visited[x] = new bool[_height];
    }

    var queue = new Queue<TilePoint>();
    queue.Enqueue(point);
    visited[point.X][point.Y] = true;

    var minX = point.X;
    var maxX = point.X;
    var minY = point.Y;
    var maxY = point.Y;
    var count = 0;

    while (queue.Count > 0)
    {
      var current = queue.Dequeue();
      var currentTileId = GetTileId(current, point, assumeTileId);
      if (currentTileId != tileId)
      {
        continue;
      }

      count++;
      minX = Math.Min(minX, current.X);
      maxX = Math.Max(maxX, current.X);
      minY = Math.Min(minY, current.Y);
      maxY = Math.Max(maxY, current.Y);

      foreach (var neighbor in GetNeighbors(current))
      {
        if (visited[neighbor.X][neighbor.Y])
        {
          continue;
        }

        var neighborTileId = GetTileId(neighbor, point, assumeTileId);
        if (neighborTileId == tileId)
        {
          visited[neighbor.X][neighbor.Y] = true;
          queue.Enqueue(neighbor);
        }
      }
    }

    if (count == 0)
    {
      return GroupMetrics.Empty;
    }

    return new GroupMetrics(count, maxX - minX + 1, maxY - minY + 1);
  }

  /// <summary>
  /// Checks whether the coordinate is within the map bounds.
  /// </summary>
  public bool IsInBounds(TilePoint point)
  {
    return point.X >= 0 && point.X < _width && point.Y >= 0 && point.Y < _height;
  }

  private int GetTileId(TilePoint current, TilePoint candidate, int? assumeTileId)
  {
    if (assumeTileId.HasValue && current == candidate)
    {
      return assumeTileId.Value;
    }

    return _output[current.X][current.Y];
  }

  private IEnumerable<TilePoint> GetNeighbors(TilePoint point)
  {
    if (point.Y > 0) yield return new TilePoint(point.X, point.Y - 1);
    if (point.Y < _height - 1) yield return new TilePoint(point.X, point.Y + 1);
    if (point.X > 0) yield return new TilePoint(point.X - 1, point.Y);
    if (point.X < _width - 1) yield return new TilePoint(point.X + 1, point.Y);
  }
}

/// <summary>
/// Metrics describing a contiguous tile region.
/// </summary>
public readonly record struct GroupMetrics(int Count, int Width, int Height)
{
  public static GroupMetrics Empty => new(0, 0, 0);
  public int MaxDimension => Math.Max(Width, Height);
  public bool IsValid => Count > 0;
}