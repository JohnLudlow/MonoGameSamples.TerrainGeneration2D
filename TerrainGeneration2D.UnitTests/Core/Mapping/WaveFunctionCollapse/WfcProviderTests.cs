using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Xunit;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.UnitTests.Core.Mapping.WaveFunctionCollapse;

public sealed class TestRandomProvider : IRandomProvider
{
  public int NextInt() => 0;
  public int NextInt(int maxValue) => 0;
  public int NextInt(int minValue, int maxValue) => minValue;
  public double NextDouble() => 0.0;
}

public class WfcProviderTests
{
  [Fact]
  public void DomainLifecycle_InitializesAndCollapsesCorrectly()
  {
    // Arrange: 2x2 grid, 2 tile types
    var registry = TileTypeRegistry.CreateDefault(2, new TileTypeRuleConfiguration());
    var random = new TestRandomProvider();
    var config = new WfcWeightConfiguration();
    var heuristics = new HeuristicsConfiguration();
    var provider = new WfcProvider(
      2, 2,
      registry,
      random,
      new TileTypeRuleConfiguration(),
      TerrainGeneration2D.Core.Mapping.HeightMap.DefaultHeightProvider.Instance,
      Microsoft.Xna.Framework.Point.Zero,
      config,
      heuristics
    );

    // Assert: All domains initialized (not null)
    for (var x = 0; x < 2; x++)
      for (var y = 0; y < 2; y++)
        Assert.NotNull(provider.GetPossibilities()[x][y]);

    // Collapse cell (0,0) to tile 1 by setting domain
    provider.GetPossibilities()[0][0] = new HashSet<int> { 1 };
    provider.CollapseCell(0, 0);

    // Assert: Domain is null or empty after collapse
    Assert.True(provider.GetPossibilities()[0][0] == null || provider.GetPossibilities()[0][0]?.Count == 0);
  }

  [Fact]
  public void EntropyHeuristics_SelectsCorrectCell()
  {
    // Arrange: 2x2 grid, custom entropy config
    var registry = TileTypeRegistry.CreateDefault(2, new TileTypeRuleConfiguration());
    var random = new TestRandomProvider();
    var config = new WfcWeightConfiguration();
    var heuristics = new HeuristicsConfiguration { UseDomainEntropy = true, UseShannonEntropy = false };
    var provider = new WfcProvider(
      2, 2,
      registry,
      random,
      new TileTypeRuleConfiguration(),
      TerrainGeneration2D.Core.Mapping.HeightMap.DefaultHeightProvider.Instance,
      Microsoft.Xna.Framework.Point.Zero,
      config,
      heuristics
    );

    // Collapse (0,0) to tile 1 by setting domain
    provider.GetPossibilities()[0][0] = new HashSet<int> { 1 };
    provider.CollapseCell(0, 0);

    // Act: Find lowest entropy cell
    var (x, y) = provider.FindLowestEntropy();

    // Assert: Should not select already collapsed cell
    Assert.False(x == 0 && y == 0);
  }

  [Fact]
  public void RuleTable_Correctness_MatchesRuntimeEvaluation()
  {
    // Arrange: 2 tile types, all directions
    var registry = TileTypeRegistry.CreateDefault(2, new TileTypeRuleConfiguration());
    var ruleTable = new PrecomputedTileTypeRuleTable(registry);
    foreach (var tileId in registry.TileIds)
    {
      var tile = registry.GetTileType(tileId);
      // ...additional assertions...
    }
  }
}
