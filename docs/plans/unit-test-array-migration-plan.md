# Unit Test Array Migration Plan

## Overview

This plan outlines the necessary updates to the unit test suite to support the migration from multidimensional arrays to jagged arrays in the WFC (Wave Function Collapse) system. The migration is part of Phase 0 of the WFC completion plan and addresses both compilation errors and performance analyzer warnings across the test suite.

## Definition of Terms

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Multidimensional Array | .NET array with fixed dimensions declared as `Type[,]` or `Type[,,]` | [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/multidimensional-arrays) |
| Jagged Array | Array of arrays declared as `Type[][]` with potentially different row lengths | [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/jagged-arrays) |
| CA1814 Warning | Code analysis rule recommending jagged arrays over multidimensional arrays for performance | [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1814) |
| CS1503 Error | Compiler error indicating argument type mismatch in method calls | - |
| Test Fixture | Class containing unit test methods, typically using xUnit framework | [xUnit Documentation](https://xunit.net/) |

## Requirements

- **Compilation Success**: All unit tests must compile without CS1503 type conversion errors
- **Performance Compliance**: Eliminate CA1814 warnings by using jagged arrays in test data
- **Test Coverage Maintenance**: Preserve existing test logic and coverage while updating data structures
- **Code Quality**: Address CA1861 warnings about constant array arguments where appropriate
- **Backward Compatibility**: Ensure tests continue to validate the same WFC behaviors after migration
- **Documentation**: Update test method comments to reflect jagged array usage patterns

## Implementation Steps

### Step 1: HeuristicsTests.cs Array Migration

**Objective**: Convert multidimensional array test data to jagged arrays for entropy provider testing.

**Current Issues**:

- CS1503 errors on lines 23, 24, 44 converting `HashSet<int>?[*,*]` to `HashSet<int>?[][]`
- CS1503 errors converting `int[*,*]` to `int[][]`
- CA1814 warnings for multidimensional array usage
- CA1861 warnings for constant array arguments

**Technical Implementation**:

```csharp
// Before: Multidimensional array initialization
var possibilities = new HashSet<int>?[3, 3];
var output = new int[3, 3];

// After: Jagged array initialization
var possibilities = new HashSet<int>?[3][];
var output = new int[3][];
for (int i = 0; i < 3; i++)
{
    possibilities[i] = new HashSet<int>?[3];
    output[i] = new int[3];
}
```

**Affected Methods**:

- `DomainEntropyProvider_ScoresByDomainSize()` - Lines 15-25
- `ShannonEntropyProvider_LowersEntropyWithPeakedPriors()` - Lines 33-45

### Step 2: MappingTests.cs Array Migration

**Objective**: Update mapping information service tests to use jagged arrays for tile data.

**Current Issues**:

- CS1503 errors on lines 44, 64 with `int[*,*]` to `int[][]` conversions
- CA2021 warnings about incompatible cast attempts
- CA1814 warnings for multidimensional array usage

**Technical Implementation**:

```csharp
// Before: Direct multidimensional array creation
var tiles = new int[4, 4] {
    {0, 0, 1, 1},
    {0, 0, 1, 1},
    {2, 2, 3, 3},
    {2, 2, 3, 3}
};

// After: Jagged array with helper method
var tiles = CreateJaggedArray(new int[,] {
    {0, 0, 1, 1},
    {0, 0, 1, 1},
    {2, 2, 3, 3},
    {2, 2, 3, 3}
});

private static int[][] CreateJaggedArray(int[,] source)
{
    int rows = source.GetLength(0);
    int cols = source.GetLength(1);
    var result = new int[rows][];
    
    for (int i = 0; i < rows; i++)
    {
        result[i] = new int[cols];
        for (int j = 0; j < cols; j++)
        {
            result[i][j] = source[i, j];
        }
    }
    return result;
}
```

**Affected Methods**:

- `MappingInformationService_ReturnsCorrectGroupMetrics()` - Lines 36-50
- `TileTypeRegistry_RespectsBeachRules()` - Lines 56-70

### Step 3: Test Helper Method Creation

**Objective**: Create reusable helper methods for common test data creation patterns.

**Technical Implementation**:
Create a new `TestArrayHelpers` class with utility methods:

```csharp
public static class TestArrayHelpers
{
    /// <summary>
    /// Creates a jagged array from multidimensional array literal for test readability.
    /// </summary>
    public static T[][] ToJagged<T>(T[,] source)
    {
        int rows = source.GetLength(0);
        int cols = source.GetLength(1);
        var result = new T[rows][];
        
        for (int i = 0; i < rows; i++)
        {
            result[i] = new T[cols];
            for (int j = 0; j < cols; j++)
            {
                result[i][j] = source[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Creates empty jagged domain grid for WFC testing.
    /// </summary>
    public static HashSet<int>?[][] CreateDomainGrid(int width, int height)
    {
        var grid = new HashSet<int>?[width][];
        for (int x = 0; x < width; x++)
        {
            grid[x] = new HashSet<int>?[height];
        }
        return grid;
    }

    /// <summary>
    /// Creates empty jagged output grid for WFC testing.
    /// </summary>
    public static int[][] CreateOutputGrid(int width, int height)
    {
        var grid = new int[width][];
        for (int x = 0; x < width; x++)
        {
            grid[x] = new int[height];
        }
        return grid;
    }
}
```

### Step 4: Constant Array Optimization

**Objective**: Address CA1861 warnings by converting constant array arguments to static readonly fields.

**Current Issues**:

- Multiple CA1861 warnings across test files for repeated constant array creation
- Performance impact from repeated array allocations in test methods

**Technical Implementation**:

```csharp
// Before: Constant arrays in method calls
Assert.Equal(expected, provider.GetScore(0, 0, possibilities, output, 
    new WfcWeightConfiguration { Base = 1, NeighborMatchBoost = 3 }));

// After: Static readonly fields
private static readonly int[] ExpectedScores = { 2.0, 1.0, 0.0 };
private static readonly WfcWeightConfiguration DefaultWeights = new() 
{ 
    Base = 1, 
    NeighborMatchBoost = 3 
};

// Usage in test methods
Assert.Equal(ExpectedScores[0], provider.GetScore(0, 0, possibilities, output, DefaultWeights));
```

### Step 5: Interface Signature Updates

**Objective**: Ensure all test mocks and stubs use updated interface signatures with jagged arrays.

**Technical Implementation**:
Update any test doubles (mocks, stubs) that implement WFC interfaces:

```csharp
// Update ICellEntropyProvider implementations in tests
public class TestEntropyProvider : ICellEntropyProvider
{
    public double GetScore(int x, int y, HashSet<int>?[][] possibilities, 
        int[][] output, WfcWeightConfiguration weightConfig)
    {
        // Updated signature with jagged arrays
        return possibilities[x][y]?.Count ?? 0;
    }
}
```

### Step 6: Test Data Validation

**Objective**: Ensure test data accuracy is preserved during array migration.

**Technical Implementation**:
Add validation tests to verify jagged array conversion preserves data:

```csharp
[Fact]
public void JaggedArrayConversion_PreservesData()
{
    var original = new int[,] { {1, 2}, {3, 4} };
    var jagged = TestArrayHelpers.ToJagged(original);
    
    Assert.Equal(original[0, 0], jagged[0][0]);
    Assert.Equal(original[0, 1], jagged[0][1]);
    Assert.Equal(original[1, 0], jagged[1][0]);
    Assert.Equal(original[1, 1], jagged[1][1]);
}
```

## Implementation Considerations

### Readability vs Performance

**Trade-off**: Jagged arrays require more verbose initialization code but provide better performance in WFC hot paths.

**Solution**: Use helper methods to maintain test readability while gaining performance benefits.

**Example**:

```csharp
// Maintains readability with helper method
var testGrid = TestArrayHelpers.ToJagged(new int[,] {
    {0, 1, 2},
    {1, 2, 0},
    {2, 0, 1}
});
```

### Test Coverage Preservation

**Consideration**: Array migration must not change test behavior or reduce coverage.

**Validation Strategy**:

- Run full test suite before and after migration
- Compare coverage reports to ensure no reduction
- Verify all existing test assertions continue to pass
- Add regression tests for array conversion utilities

### Memory Allocation Patterns

**Consideration**: Test performance should improve with jagged arrays, but initialization complexity increases.

**Mitigation**:

- Use shared test data where possible
- Implement lazy initialization for expensive test fixtures
- Profile test execution times before and after migration

### Maintenance Burden

**Consideration**: Jagged array syntax is more complex for test writers.

**Mitigation**:

- Document array creation patterns in test README
- Provide code snippets and examples
- Create IntelliSense templates for common patterns

## Testing Strategy

### Unit Test Validation

- **Compilation Tests**: Verify all tests compile without CS1503 errors
- **Behavior Tests**: Ensure migrated tests produce identical results
- **Performance Tests**: Validate that test execution improves or remains stable
- **Coverage Tests**: Confirm test coverage percentage is maintained

### Integration Test Validation

- **End-to-End WFC**: Verify WFC generation works with jagged arrays
- **Chunk Generation**: Test chunked tilemap with updated array types
- **Boundary Constraints**: Validate seam consistency with new array structures

### Regression Test Creation

Create specific tests for array migration:

```csharp
[Theory]
[InlineData(1, 1)]
[InlineData(3, 3)]
[InlineData(64, 64)]
public void JaggedArrayMigration_PreservesAccessPatterns(int width, int height)
{
    // Test that x,y access works consistently between old and new patterns
    var multidimensional = new int[width, height];
    var jagged = new int[width][];
    
    for (int x = 0; x < width; x++)
    {
        jagged[x] = new int[height];
        for (int y = 0; y < height; y++)
        {
            int testValue = x * height + y;
            multidimensional[x, y] = testValue;
            jagged[x][y] = testValue;
            
            Assert.Equal(multidimensional[x, y], jagged[x][y]);
        }
    }
}
```

## Migration Timeline

1. **Phase 1** (1-2 days): Create helper methods and update HeuristicsTests.cs
2. **Phase 2** (1 day): Update MappingTests.cs and other core test files  
3. **Phase 3** (1 day): Address CA1861 warnings with static readonly fields
4. **Phase 4** (1 day): Add regression tests and validation
5. **Phase 5** (0.5 days): Documentation and cleanup

**Total Estimate**: 4.5-5.5 days

## Success Criteria

- **Zero compilation errors**: All CS1503 errors resolved
- **Clean analysis**: All CA1814 warnings eliminated  
- **Maintained coverage**: Test coverage percentage unchanged
- **Improved performance**: Test execution time improved or stable
- **Documentation complete**: Updated test patterns documented
- **Regression protection**: New tests prevent future array-related issues

## Follow-up Tasks

- Update test documentation with jagged array patterns
- Create developer guidelines for writing new WFC tests
- Consider automated code analysis rules for test array usage
- Monitor test performance metrics after migration
- Plan similar migrations for integration and benchmark test suites
