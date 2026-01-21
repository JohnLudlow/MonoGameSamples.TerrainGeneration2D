using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon;

public sealed class TestRandomProvider : IRandomProvider
{
  public int NextInt() => 0;
  public int NextInt(int maxValue) => 0;
  public int NextInt(int minValue, int maxValue) => minValue;
  public double NextDouble() => 0.0;
}
