# Library Abstraction for Non-Tile Domains

## Overview

Abstract the WFC library to support non-tile domains, enabling procedural generation for a wide range of applications (e.g., building layouts, resource placement). Intended for teams needing WFC for domains beyond terrain tiles.

## Table of contents

- [Overview](#overview)
- [Feature requirements](#feature-requirements)
- [Feature status](#feature-status)
- [Definition of terms](#definition-of-terms)
- [Architectural considerations and constraints](#architectural-considerations-and-constraints)
- [Implementation guide](#implementation-guide)

## Feature requirements

- Generic WFC solver interfaces supporting arbitrary cell/value types
- Decouple terrain-specific logic from WFC core
- Support for custom rule and constraint systems
- Maintain performance and determinism

## Feature status

- Not started

## Definition of terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Domain | The set of possible values for a cell | |
| Adapter | A component that translates between generic and domain-specific logic | |

## Architectural considerations and constraints

- Backward compatibility for terrain generation
  - The generic WFC abstraction must preserve support for all existing terrain generation features and APIs.
  - Existing code using tile-based WFC (e.g., `WfcProvider`, `TileTypeRegistry`, `ChunkedTilemap`) should continue to work without modification.
  - Provide adapter classes or shims that map the legacy tile-based interfaces to the new generic interfaces.
  - Migration should be incremental: legacy and generic APIs can coexist, allowing gradual adoption.
  - All tests and benchmarks for terrain generation must pass after migration.
- Plugin system must support generic domains
- Test coverage for new abstractions

```mermaid
classDiagram
    class IWfcSolver_TCell_TValue_ {
        +Solve(WfcConfiguration_TCell_TValue_)
    }
    class Adapter {
        +Translate(...)
    }
    IWfcSolver_TCell_TValue_ <|.. Adapter
```

## Implementation guide

### Phase 1: Refactor WFC Core to Generics (Backward Compatibility Phase)

#### Objective

Refactor WFC core to use generic types for cells and values, while preserving backward compatibility for all existing terrain generation code and APIs.

#### Technical details

- Change class and method signatures to use type parameters. Move terrain-specific logic to adapters.
- Provide adapter classes or shims that allow legacy tile-based APIs (e.g., WfcProvider, TileTypeRegistry) to work with the new generic core without modification.
- Ensure all existing terrain generation tests and benchmarks pass after migration.

#### Examples

```csharp
/// <summary>
/// Generic WFC solver interface for any constraint satisfaction domain.
/// </summary>
/// <typeparam name="TCell">Cell coordinate type (e.g., Point, Vector3)</typeparam>
/// <typeparam name="TValue">Value type placed in cells (e.g., int, enum)</typeparam>
public interface IWfcSolver<TCell, TValue>
{
    /// <summary>
    /// Solves the constraint satisfaction problem using WFC algorithm.
    /// </summary>
    /// <param name="config">Solver configuration and constraints</param>
    /// <returns>Solution if found; null if unsatisfiable within constraints</returns>
    WfcSolution<TCell, TValue>? Solve(WfcConfiguration<TCell, TValue> config);
}

/// <summary>
/// Adapter for legacy tile-based WFC API, preserving backward compatibility.
/// </summary>
public class LegacyTileWfcAdapter : IWfcSolver<(int x, int y), int>
{
    private readonly WfcProvider _legacyProvider;
    public LegacyTileWfcAdapter(WfcProvider legacyProvider) => _legacyProvider = legacyProvider;
    public WfcSolution<(int x, int y), int>? Solve(WfcConfiguration<(int x, int y), int> config)
    {
        // Bridge call to legacy provider
        var success = _legacyProvider.Generate();
        if (!success) return null;
        // Convert legacy output to generic solution
        // ...implementation omitted...
        return new WfcSolution<(int x, int y), int>();
    }
}
```

The following test demonstrates that legacy terrain generation continues to work after migration to the generic WFC core. It verifies that the adapter correctly bridges the legacy and generic APIs and that the output is still valid.

```csharp
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
```

```csharp
/// <summary>
/// Adapter for translating between generic and domain-specific logic.
/// </summary>
public class BuildingLayoutAdapter : IWfcSolver<(int x, int y), string>
{
    /// <inheritdoc />
    public WfcSolution<(int x, int y), string>? Solve(WfcConfiguration<(int x, int y), string> config)
    {
        // Example: Use WFC to generate a building layout with room types as strings
        // ("Office", "Hallway", "Restroom", etc.)
        // ...implementation omitted...
        return null;
    }
}

/// <summary>
/// Example usage: Resource placement in a grid.
/// </summary>
public void ResourcePlacementExample()
{
    // Define possible resources
    var resources = new[] { "Gold", "Wood", "Stone" };

    // Create configuration for a 10x10 grid
    var config = new WfcConfiguration<(int x, int y), string>
    {
        // ... set up domains, constraints, etc. ...
    };

    // Use a generic solver (could be a custom or built-in implementation)
    IWfcSolver<(int x, int y), string> solver = new ResourcePlacementAdapter();
    var solution = solver.Solve(config);
    // ... use solution for game logic ...
}
```

### Phase 2: Rule Table and Propagator Abstraction

#### Objective

Update the rule table and propagator to support generic constraints, enabling the WFC core to operate on any cell/value type, not just tile IDs.

#### Technical details

- Refactor the rule table interface to use type parameters for cell and value types.
- Update the propagator (e.g., AC3Propagator) to work with generic domains and constraints.
- Provide adapters for terrain (legacy) and new non-tile domains.
- Ensure all constraint logic (adjacency, domain reduction) is type-agnostic and extensible.

#### Examples

```csharp
/// <summary>
/// Generic rule table interface for WFC constraints.
/// </summary>
public interface IRuleTable<TCell, TValue>
{
    /// <summary>
    /// Gets allowed neighboring values for a given value in a specific direction.
    /// </summary>
    IEnumerable<TValue> GetAllowedNeighbors(TValue value, Direction direction);
}

/// <summary>
/// Generic propagator for arc consistency.
/// </summary>
public class GenericAC3Propagator<TCell, TValue>
{
    public bool PropagateFrom(TCell cell, TValue observedValue, IRuleTable<TCell, TValue> ruleTable, /* ... */)
    {
        // ... generic propagation logic ...
        return true;
    }
}
```

### Phase 3: Sample Implementations and Documentation

#### Objective

Demonstrate the flexibility of the generic WFC library by providing sample implementations for non-tile domains and updating onboarding documentation.

#### Technical details

- Implement at least two sample adapters: one for building layouts (e.g., rooms as values), one for resource placement (e.g., resources as values).
- Document the process of creating a new domain adapter, including required interfaces and configuration.
- Update onboarding and API documentation to include generic usage patterns and migration guides.

#### Examples

```csharp
/// <summary>
/// Adapter for WFC-based building layout generation.
/// </summary>
public class BuildingLayoutAdapter : IWfcSolver<(int x, int y), string>
{
    public WfcSolution<(int x, int y), string>? Solve(WfcConfiguration<(int x, int y), string> config)
    {
        // Example: Use WFC to generate a building layout with room types as strings
        // ...implementation omitted...
        return null;
    }
}

/// <summary>
/// Adapter for WFC-based resource placement.
/// </summary>
public class ResourcePlacementAdapter : IWfcSolver<(int x, int y), string>
{
    public WfcSolution<(int x, int y), string>? Solve(WfcConfiguration<(int x, int y), string> config)
    {
        // Example: Use WFC to place resources on a grid
        // ...implementation omitted...
        return null;
    }
}
```

### Phase 4: Testing

#### Objective

Ensure correctness, robustness, and maintainability of the generic WFC abstractions through comprehensive testing.

#### Technical details

- Add unit tests for the generic solver interface and core algorithm logic.
- Create integration tests for each sample non-tile domain adapter (e.g., building layouts, resource placement).
- Implement property-based tests to verify constraint satisfaction, determinism, and completeness across arbitrary domains.
- Validate backward compatibility with terrain generation through regression tests.

#### Examples

```csharp
/// <summary>
/// Unit test for generic WFC solver.
/// </summary>
[Fact]
public void GenericSolver_SolvesSimpleDomain()
{
    // Arrange: create a simple domain and configuration
    var config = new WfcConfiguration<(int, int), string> { /* ... */ };
    var solver = new BuildingLayoutAdapter();
    // Act
    var solution = solver.Solve(config);
    // Assert
    Assert.NotNull(solution);
}

/// <summary>
/// Property-based test for constraint satisfaction.
/// </summary>
[Property]
public void GenericSolver_AlwaysSatisfiesConstraints(/* ... */)
{
    // ... property-based test logic ...
}
```
