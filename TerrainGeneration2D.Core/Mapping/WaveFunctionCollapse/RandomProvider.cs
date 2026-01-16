namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Abstraction over randomness to enable deterministic testing and mocking.
/// </summary>
public interface IRandomProvider
{
  int NextInt();
  int NextInt(int maxValue);
  int NextInt(int minValue, int maxValue);
  double NextDouble();
}

/// <summary>
/// Adapter that wraps <see cref="System.Random"/> to provide <see cref="IRandomProvider"/>.
/// </summary>
public sealed class RandomAdapter : IRandomProvider
{
  private readonly System.Random _random;

  public RandomAdapter(System.Random random)
  {
    _random = random ?? throw new System.ArgumentNullException(nameof(random));
  }

  public int NextInt() => _random.Next();
  public int NextInt(int maxValue) => _random.Next(maxValue);
  public int NextInt(int minValue, int maxValue) => _random.Next(minValue, maxValue);
  public double NextDouble() => _random.NextDouble();
}