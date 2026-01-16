namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping;

/// <summary>
/// Represents a tile coordinate pair within a chunk or grid.
/// </summary>
public readonly record struct TilePoint(int X, int Y);