namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

public readonly record struct HeightSample
{
  public float Altitude { get; init; }
  public float MountainNoise { get; init; }
  public float DetailNoise { get; init; }

  public HeightSample(float altitude, float mountainNoise, float detailNoise)
  {
    Altitude = altitude;
    MountainNoise = mountainNoise;
    DetailNoise = detailNoise;
  }
}
