using System.Diagnostics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

/// <summary>
/// Represents a tile coordinate pair within a chunk or grid.
/// </summary>
[DebuggerDisplay("({X},{Y})")]
public readonly record struct TilePoint(int X, int Y)
{
	public override string ToString() => $"({X},{Y})";
}