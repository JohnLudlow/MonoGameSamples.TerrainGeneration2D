namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

public interface IHeightProvider
{
  HeightSample GetSample(int worldX, int worldY);
}