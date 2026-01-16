using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.WFC
{
  public class WfcProviderUnitTests
  {
    [Fact]
    public void DomainLifecycle_InitializesAndCollapsesCorrectly()
    {
      // Arrange: 2x2 grid, 2 tile types
      var registry = TileTypeRegistry.CreateDefault(2, new TerrainRuleConfiguration());
      var random = new TestRandomProvider();
      var config = new WfcWeightConfiguration();
      var heuristics = new HeuristicsConfiguration();
      var provider = new WfcProvider(
        2, 2,
        registry,
        random,
        new TerrainRuleConfiguration(),
        Core.Mapping.HeightMap.DefaultHeightProvider.Instance,
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
      var registry = TileTypeRegistry.CreateDefault(2, new TerrainRuleConfiguration());
      var random = new TestRandomProvider();
      var config = new WfcWeightConfiguration();
      var heuristics = new HeuristicsConfiguration { UseDomainEntropy = true, UseShannonEntropy = false };
      var provider = new WfcProvider(
        2, 2,
        registry,
        random,
        new TerrainRuleConfiguration(),
        Core.Mapping.HeightMap.DefaultHeightProvider.Instance,
        Microsoft.Xna.Framework.Point.Zero,
        config,
        heuristics
      );


      // Collapse (0,0) to tile 1 by setting domain
      provider.GetPossibilities()[0][0] = [1];
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
      var registry = TileTypeRegistry.CreateDefault(2, new TerrainRuleConfiguration());
      var ruleTable = new PrecomputedRuleTable(registry);
      foreach (var tileId in registry.TileIds)
      {
        var tile = registry.GetTileType(tileId);

        foreach (var dir in Enum.GetValues<Direction>())
        {
          var allowed = ruleTable.GetAllowedNeighbors(tile.TileId, dir);
          // Runtime: check by evaluating rules directly
          var runtimeAllowed = new HashSet<int>();

          foreach (var neighborId in registry.TileIds)
          {
            var neighbor = registry.GetTileType(neighborId);

            // Provide dummy/default values for context
            var ctx = new TileRuleContext(
              default,
              tile.TileId,
              default,
              neighbor.TileId,
              dir,
              new TerrainRuleConfiguration(),
              default,
              default,
              null
            );

            if (tile.EvaluateRules(ctx))
              runtimeAllowed.Add(neighbor.TileId);
          }

          Assert.Equal(runtimeAllowed, [.. allowed.TileIds]);
        }
      }
    }

    [Fact]
    public void AC3Propagation_EliminatesInvalidDomains()
    {
      // Arrange: 2x2 grid, custom domains
      var registry = TileTypeRegistry.CreateDefault(2, new TerrainRuleConfiguration());
      var ruleTable = new PrecomputedRuleTable(registry);
      var domains = new HashSet<int>?[2][];

      for (var x = 0; x < 2; x++)
      {
        domains[x] = new HashSet<int>?[2];

        for (var y = 0; y < 2; y++)
          domains[x][y] = [0, 1];
      }

      // Collapse (0,0) to 0
      domains[0][0] = null;
      var propagator = new AC3Propagator(ruleTable, domains);
      // Act: propagate from (0,0)
      var result = propagator.PropagateFrom(0, 0, 0);
      // Assert: No contradiction, domains reduced
      Assert.True(result);
      Assert.NotNull(domains[1][0]);
      Assert.True(domains[1][0]!.Count > 0);
    }

    // Minimal stub for deterministic random
    private class TestRandomProvider : IRandomProvider
    {
      public int NextInt() => 0;
      public int NextInt(int maxValue) => 0;
      public int NextInt(int minValue, int maxValue) => minValue;
      public double NextDouble() => 0.0;
    }
  }
}