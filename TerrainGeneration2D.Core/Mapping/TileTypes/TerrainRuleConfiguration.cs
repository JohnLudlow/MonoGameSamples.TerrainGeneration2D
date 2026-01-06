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
  public float OceanHeightMax { get; init; } = 0.34f;
  public float BeachHeightMin { get; init; } = 0.33f;
  public float BeachHeightMax { get; init; } = 0.48f;
  public float PlainsHeightMin { get; init; } = 0.35f;
  public float PlainsHeightMax { get; init; } = 0.78f;
  public float ForestHeightMin { get; init; } = 0.42f;
  public float ForestHeightMax { get; init; } = 0.88f;
  public float SnowHeightMin { get; init; } = 0.82f;
  public float MountainHeightMin { get; init; } = 0.76f;
  public float MountainNoiseThreshold { get; init; } = 0.55f;
}
