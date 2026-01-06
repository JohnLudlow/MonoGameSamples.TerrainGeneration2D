using System;


namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

public sealed class HeightMapGenerator : IHeightProvider
{
  private readonly FastNoiseLite _continentNoise;
  private readonly FastNoiseLite _mountainNoise;
  private readonly FastNoiseLite _detailNoise;
  private readonly HeightMapConfiguration _config;

  public HeightMapGenerator(int seed, HeightMapConfiguration? config = null)
  {
    _config = config ?? new HeightMapConfiguration();

    _continentNoise = new FastNoiseLite(seed);
    _continentNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    _continentNoise.SetFrequency(_config.ContinentScale);

    _mountainNoise = new FastNoiseLite(seed + 1);
    _mountainNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    _mountainNoise.SetFrequency(_config.MountainScale);

    _detailNoise = new FastNoiseLite(seed + 2);
    _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.Value);
    _detailNoise.SetFrequency(_config.DetailScale);
  }

  public HeightSample GetSample(int worldX, int worldY)
  {
    var continent = (_continentNoise.GetNoise(worldX, worldY) + 1f) / 2f;
    var mountain = (_mountainNoise.GetNoise(worldX, worldY) + 1f) / 2f;
    var detail = (_detailNoise.GetNoise(worldX, worldY) + 1f) / 2f;

    var altitude = continent * _config.ContinentWeight + mountain * _config.MountainWeight + detail * _config.DetailWeight;
    altitude = Math.Clamp(altitude, 0f, 1f);

    return new HeightSample(altitude, mountain, detail);
  }
}
