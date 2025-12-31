namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class TerrainRuleConfiguration
{
  public int MountainRangeMin { get; init; } = 8;
  public int MountainRangeMax { get; init; } = 48;
  public int MountainWidthMax { get; init; } = 12;
  public int MountainWidthMin { get; init; } = 3;
  public int BeachOceanSizeMin { get; init; } = 12;
  public int BeachOceanSizeMax { get; init; } = 180;
  public int BeachPlainsSizeMin { get; init; } = 20;
  public int BeachPlainsSizeMax { get; init; } = 400;
}
