using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/BitSet.cs

/// <summary>
/// Efficient bit set implementation for tile ID collections using BitArray.
/// Provides O(1) set operations for rule table lookups.
/// </summary>
/// <remarks>
/// Wraps System.Collections.BitArray with set-like operations for WFC domains.
/// </remarks>
public class BitSet : IEnumerable<int>
{
  private readonly BitArray _bits;

  public BitSet(int capacity) => _bits = new BitArray(capacity);

  /// <summary>
  /// Checks if the specified tile ID is present in this set.
  /// </summary>
  public bool Contains(int tileId) => tileId < _bits.Length && _bits[tileId];

  /// <summary>
  /// Adds a tile ID to this set.
  /// </summary>
  public void Add(int tileId) { if (tileId < _bits.Length) _bits[tileId] = true; }

  /// <summary>
  /// Performs intersection with another BitSet, modifying this set.
  /// </summary>
  public void IntersectWith(BitSet other)
  {
    ArgumentNullException.ThrowIfNull(other);

    _ = _bits.And(other._bits);
  }

  /// <summary>
  /// Gets all tile IDs present in this set.
  /// </summary>
  public IEnumerable<int> TileIds
  {
    get
    {
      for (var i = 0; i < _bits.Length; i++)
        if (_bits[i]) yield return i;
    }
  }

  /// <summary>
  /// Returns an enumerator that iterates through the set of tile IDs.
  /// </summary>
  public IEnumerator<int> GetEnumerator() => TileIds.GetEnumerator();

  /// <summary>
  /// Returns a non-generic enumerator that iterates through the set of tile IDs.
  /// </summary>
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}