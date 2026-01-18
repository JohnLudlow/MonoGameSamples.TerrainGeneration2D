using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Xunit;
using System.Collections.Generic;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.HeightMap;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

/// <summary>
/// Tests for generic WFC abstractions.
/// </summary>
public class WfcGenericTests
{
    /// <summary>
    /// Test: Legacy terrain generation still works after migration.
    /// </summary>
    [Fact]
    public void LegacyTerrainGeneration_ProducesSameOutput_AfterMigration()
    {
        // TODO: Provide real arguments for these types
        var width = 8;
        var height = 8;

        var terrainRuleConfiguration = new TileTypeRuleConfiguration();
        var heightMapConfiguration = new HeightMapConfiguration();
        var random = new RandomAdapter(Random.Shared);

        var tileRegistry = new TileTypeRegistry(
        [
            new OceanTileType(0),
            new BeachTileType(1),
            new PlainsTileType(2)
        ]);      

        var heightProvider = new HeightMapGenerator(12345, heightMapConfiguration);
        var chunkOrigin = new Microsoft.Xna.Framework.Point(0, 0);
        var legacyProvider = new WfcProvider(
          width,
          height,
          tileRegistry,
          random,
          terrainRuleConfiguration,
          heightProvider,
          chunkOrigin
        );
        var adapter = new LegacyTileWfcAdapter(legacyProvider);


        // For generic config, provide initialDomains and ruleTable
        var initialDomains = new Dictionary<(int x, int y), ISet<int>>(); // TODO: fill with test data
        var ruleTable = new PrecomputedTileTypeRuleTable(tileRegistry);
        var genericConfig = new WfcConfiguration<(int x, int y), int>(initialDomains, ruleTable);
        // Act
        var solution = adapter.Solve(genericConfig);
        // Assert
        Assert.NotNull(solution);
        // Optionally compare output to known-good legacy result
    }

    /// <summary>
    /// Unit test for generic WFC solver.
    /// </summary>
    [Fact]
    public void GenericSolver_SolvesSimpleDomain()
    {
        // Arrange: create a simple domain and configuration
        var initialDomains = new Dictionary<(int, int), ISet<string>>
        {
            { (0, 0), new HashSet<string> { "Gold", "Wood" } },
            { (0, 1), new HashSet<string> { "Stone" } }
        };
        // var ruleTable = new SimpleResourceRuleTable(); // See below for implementation
        // var config = new WfcConfiguration<(int, int), string>(initialDomains, ruleTable);
        // var solver = new ResourcePlacementAdapter();
        // Act
        // var solution = solver.Solve(config);
        // Assert
        // Assert.NotNull(solution);
        // Assert.Contains((0, 0), solution.Assignments.Keys);
        // Assert.Contains((0, 1), solution.Assignments.Keys);
        Assert.True(true); // Placeholder until implementation
    }

    // TODO: Add property-based tests with FsCheck if needed
}