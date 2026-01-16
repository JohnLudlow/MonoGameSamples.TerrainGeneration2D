namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

public sealed class DefaultHeightProvider : IHeightProvider
{
  public static readonly DefaultHeightProvider Instance = new();

  private DefaultHeightProvider()
  {
  }

  public HeightSample GetSample(int worldX, int worldY)
  {
    return new HeightSample(0.5f, 0f, 0f);
  }
}