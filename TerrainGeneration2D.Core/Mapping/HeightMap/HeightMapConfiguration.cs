namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

public sealed class HeightMapConfiguration
{
  public float ContinentScale { get; init; } = 0.0045f;
  public float MountainScale { get; init; } = 0.02f;
  public float DetailScale { get; init; } = 0.1f;
  public float ContinentWeight { get; init; } = 0.75f;
  public float MountainWeight { get; init; } = 0.35f;
  public float DetailWeight { get; init; } = 0.25f;
}
