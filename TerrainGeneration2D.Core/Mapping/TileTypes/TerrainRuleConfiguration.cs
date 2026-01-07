namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class TerrainRuleConfiguration
{
  public int MountainRangeMin { get; set; } = 8;
  public int MountainRangeMax { get; set; } = 48;
  public int MountainWidthMax { get; set; } = 12;
  public int MountainWidthMin { get; set; } = 3;
  public int BeachOceanSizeMin { get; set; } = 12;
  public int BeachOceanSizeMax { get; set; } = 180;
  public int BeachPlainsSizeMin { get; set; } = 20;
  public int BeachPlainsSizeMax { get; set; } = 400;
  public float OceanHeightMax { get; set; } = 0.34f;
  public float BeachHeightMin { get; set; } = 0.33f;
  public float BeachHeightMax { get; set; } = 0.48f;
  public float PlainsHeightMin { get; set; } = 0.35f;
  public float PlainsHeightMax { get; set; } = 0.78f;
  public float ForestHeightMin { get; set; } = 0.42f;
  public float ForestHeightMax { get; set; } = 0.88f;
  public float SnowHeightMin { get; set; } = 0.82f;
  public float MountainHeightMin { get; set; } = 0.76f;
  public float MountainNoiseThreshold { get; set; } = 0.55f;
}
