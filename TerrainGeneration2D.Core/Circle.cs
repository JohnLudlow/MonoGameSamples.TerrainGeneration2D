using System;
using Microsoft.Xna.Framework;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public readonly struct Circle : IEquatable<Circle>
{

  /// <summary>
  /// The x-coordinate of the center of this circle.
  /// </summary>
  public readonly int X { get; }

  /// <summary>
  /// The y-coordinate of the center of this circle.
  /// </summary>
  public readonly int Y { get; }

  /// <summary>
  /// The length, in pixels, from the center of this circle to the edge.
  /// </summary>
  public readonly int Radius { get; }

  public static Circle Empty { get; } = new Circle(0, 0, 0);
  public readonly bool IsEmpty => X == 0 && Y == 0 && Radius <= 0;
  public Point Location => new(X, Y);

  /// <summary>
  /// Gets the y-coordinate of the highest point on this circle.
  /// </summary>
  public readonly int Top => Y - Radius;

  /// <summary>
  /// Gets the y-coordinate of the lowest point on this circle.
  /// </summary>
  public readonly int Bottom => Y + Radius;

  /// <summary>
  /// Gets the x-coordinate of the leftmost point on this circle.
  /// </summary>
  public readonly int Left => X - Radius;

  /// <summary>
  /// Gets the x-coordinate of the rightmost point on this circle.
  /// </summary>
  public readonly int Right => X + Radius;


  /// <summary>
  /// Creates a new circle with the specified position and radius.
  /// </summary>
  /// <param name="x">The x-coordinate of the center of the circle.</param>
  /// <param name="y">The y-coordinate of the center of the circle..</param>
  /// <param name="radius">The length from the center of the circle to an edge.</param>
  public Circle(int x, int y, int radius)
  {
    X = x;
    Y = y;
    Radius = radius;
  }

  /// <summary>
  /// Creates a new circle with the specified position and radius.
  /// </summary>
  /// <param name="location">The center of the circle.</param>
  /// <param name="radius">The length from the center of the circle to an edge.</param>
  public Circle(Point location, int radius)
  {
    X = location.X;
    Y = location.Y;
    Radius = radius;
  }

  /// <summary>
  /// Returns a value that indicates whether this circle and the specified object are equal
  /// </summary>
  /// <param name="obj">The object to compare with this circle.</param>
  /// <returns>true if this circle and the specified object are equal; otherwise, false.</returns>
  public override readonly bool Equals(object? obj) => obj is Circle other && Equals(other);

  /// <summary>
  /// Returns a value that indicates whether this circle and the specified circle are equal.
  /// </summary>
  /// <param name="other">The circle to compare with this circle.</param>
  /// <returns>true if this circle and the specified circle are equal; otherwise, false.</returns>
  public readonly bool Equals(Circle other) => X == other.X &&
                                               Y == other.Y &&
                                               Radius == other.Radius;

  public bool Intersects(Circle other)
  {
    var radiiSquared = (Radius + other.Radius) * (Radius + other.Radius);
    var distanceSquared = Vector2.DistanceSquared(Location.ToVector2(), other.Location.ToVector2());
    return distanceSquared < radiiSquared;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(X, Y, Radius);
  }

  public static bool operator ==(Circle left, Circle right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(Circle left, Circle right)
  {
    return !left.Equals(right);
  }
}