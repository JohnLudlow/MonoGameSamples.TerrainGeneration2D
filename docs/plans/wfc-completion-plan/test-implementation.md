# Test Implementation for Library Abstraction

This document provides the concrete test code for verifying the generic WFC library abstractions.

## Unit Tests

Add the following test class to `TerrainGeneration2D.Tests/WfcGenericTests.cs`:

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;
using Xunit;

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
        // Arrange: set up legacy provider and config
        var legacyProvider = new WfcProvider(/* ... legacy args ... */);
        var adapter = new LegacyTileWfcAdapter(legacyProvider);
        var config = new WfcConfiguration<(int x, int y), int> { /* ... */ };
        // Act
        var solution = adapter.Solve(config);
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
        var ruleTable = new SimpleResourceRuleTable(); // See below for implementation
        var config = new WfcConfiguration<(int, int), string>(initialDomains, ruleTable);
        var solver = new ResourcePlacementAdapter();
        // Act
        var solution = solver.Solve(config);
        // Assert
        Assert.NotNull(solution);
        Assert.Contains((0, 0), solution.Assignments.Keys);
        Assert.Contains((0, 1), solution.Assignments.Keys);
    }

/// <summary>
/// Simple rule table for resource placement (all resources allowed as neighbors).
/// </summary>
public class SimpleResourceRuleTable : IRuleTable<string>
{
    public IEnumerable<string> GetAllowedNeighbors(string value, Direction direction)
    {
        // For this test, all resources are allowed as neighbors
        return new[] { "Gold", "Wood", "Stone" };
    }
}

    /// <summary>
    /// Property-based test for constraint satisfaction.
    /// </summary>
    [Property]
    public void GenericSolver_AlwaysSatisfiesConstraints(/* ... */)
    {
        // ... property-based test logic ...
    }
}
```

## Integration with Test Project

Ensure the test project includes:

- Reference to `TerrainGeneration2D.Core`
- xUnit packages (already present)
- Any additional packages for property-based testing (e.g., FsCheck if needed)

Run tests with: `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
