# Wave Function Collapse Implementation Completion Plan

## Overview

This plan outlines the implementation steps needed to complete the Wave Function Collapse
(WFC) algorithm in the MonoGame TerrainGeneration2D project. The current implementation
provides a foundation with basic observation, constraint propagation, and backtracking
capabilities, but lacks several critical components for a production-ready WFC system
suitable for strategy games.

The goal is to transform the existing partial implementation into a robust,
high-performance, and well-tested WFC library that can generate seamless terrain across
chunk boundaries while maintaining deterministic behavior and supporting future adaptation
into a standalone library for production strategy games.

**Primary Use Cases:**

- Real-time terrain generation for strategy games with infinite scrolling
- Seamless chunk boundaries ensuring visual and logical consistency across
  generated terrain
- Deterministic generation for multiplayer games requiring identical maps across
  clients
- Plugin architecture support enabling custom rule systems and tile types for
  different biomes
- Performance-constrained environments with strict frame-time budgets
  (≤100ms generation time)

**Target Constraints:**

- Runtime Performance: Chunk generation must complete within configurable time budgets
  (20-100ms)
- Memory Efficiency: Minimal heap allocations during generation to maintain stable
  frame rates
- Deterministic Behavior: Identical inputs must produce identical outputs across
  platforms and sessions
- Extensibility: Support for future tile types, rule systems, and generation
  algorithms without core changes
- Developer Onboarding: Clear interfaces and comprehensive documentation for teams
  unfamiliar with constraint satisfaction

## Feature requirements

- **AC-3 Constraint Propagation**: Replace current propagation with proper arc
  consistency algorithm for 40% reduction in contradictions
- **Precomputed Rule Tables**: Achieve 70% performance improvement in rule evaluation
  through lookup tables
- **Boundary Constraint System**: Ensure 100% consistency between adjacent chunk
  boundaries with no visual seams
- **Performance Optimization**: 60% reduction in memory allocations and 80% reduction
  in height-related computations
- **Library Abstraction**: Generic WFC solver interface supporting non-tile domains
  and plugin architecture
- **Comprehensive Testing**: ≥95% code coverage with unit, integration,
  property-based, and performance regression tests
- **Developer Documentation**: Complete onboarding materials enabling productivity
  within 2 weeks for WFC newcomers

## Feature status

- In development

The WFC implementation is currently partial with basic functionality in place, but
requires significant completion work to meet production requirements for strategy game
integration.

## Definition of terms

Detailed list of terms not considered 'common english'. Include references to
articles about the term

| Term | Meaning | Reference |
| ---- | ------- | --------- |
| Wave Function Collapse | A constraint-solving algorithm that generates content by iteratively collapsing superposition states based on local adjacency rules | [WFC Original Paper](https://github.com/mxgmn/WaveFunctionCollapse) |
| Domain | The set of possible tile types that can be placed at a given cell position | - |
| Entropy | A measure of uncertainty in a cell's domain; lower entropy cells have fewer possible tiles | - |
| Observation | The act of selecting and placing a specific tile from a cell's domain | - |
| Propagation | The process of updating neighboring cell domains based on adjacency constraints after an observation | - |
| Backtracking | Rolling back decisions when contradictions occur and trying alternative choices | - |
| Arc Consistency | A constraint propagation algorithm that ensures all constraints between connected variables are satisfied | [AC-3 Algorithm](https://en.wikipedia.org/wiki/AC-3_algorithm) |
| Seam Consistency | Ensuring that adjacent chunks in the world have compatible tile placements at their boundaries | - |
| Shannon Entropy | An information-theoretic measure of entropy calculated as H = -Σ(pi × log(pi)) | [Information Theory](https://en.wikipedia.org/wiki/Entropy_(information_theory)) |
| Most Constraining Variable | A heuristic that preferentially selects cells that will constrain the most neighboring cells | [CSP Heuristics](https://en.wikipedia.org/wiki/Constraint_satisfaction_problem) |
| Rule Table | Precomputed adjacency constraints that define which tiles can be placed next to each other | - |
| Change Log | A data structure that records reversible changes to support backtracking | - |
| BitSet | A custom data structure wrapping .NET's BitArray to provide efficient set operations for tile ID collections | [System.Collections.BitArray](https://docs.microsoft.com/en-us/dotnet/api/system.collections.bitarray) |

## Architectural considerations and constraints

The WFC completion requires careful consideration of the existing MonoGame architecture and performance constraints:

- **Performance**: Target generation time ≤100ms per chunk for real-time gameplay
- **Memory**: Optimize for constrained environments with large tile sets and chunk sizes
- **Threading**: Design for future multi-threaded chunk generation
- **Determinism**: Ensure reproducible results across platforms for multiplayer consistency
- **Extensibility**: Support plugin architecture for custom rule systems and tile types
- **Configuration**: WFC behavior controlled through [TerrainGeneration2D/appsettings.json](../../TerrainGeneration2D/appsettings.json) with F10 runtime panel

**Core WFC Algorithm Flow:**

The Wave Function Collapse algorithm operates through iterative observation and constraint propagation:

1. Initialization: All cells start with full domain (all possible tiles)
2. Cell Selection: Choose cell with minimum entropy using heuristics
3. Observation: Collapse cell to single tile value
4. Propagation: Update neighboring cell domains using AC-3 algorithm
5. Repeat: Continue until all cells observed or contradiction encountered
6. Backtracking: If contradiction, revert to previous state and try alternatives

**Mathematical Formulations:**

Shannon Entropy for information-theoretic cell selection:
$$H = -\sum_{i} p_i \log_2 p_i$$

Where $p_i$ is the weighted probability of tile type $i$ in the current domain:
$$p_i = \frac{w_i}{\sum_{j} w_j}$$

Domain Entropy (simpler alternative): $E = |D|$ where $|D|$ is the domain size.

AC-3 Arc Consistency time complexity: $O(ed^3)$ where $e$ = number of arcs, $d$ = maximum domain size.

**Data Flow Architecture:**

```plain
ChunkedTilemap -> WfcProvider -> RuleTable -> AC3Propagator
      ↓              ↓              ↓              ↓
BoundaryConstraints <- EntropyProvider <- ChangeLog <- TimeBudgetManager
```

The system flows from chunk-level orchestration through WFC solving with precomputed rules, constraint propagation, and backtracking support, all within managed time budgets.

**Performance Data Structure Choices:**

Jagged arrays (`HashSet<int>[][]`) are used instead of multidimensional arrays (`HashSet<int>[,]`) for WFC domain grids based on performance analysis:

- **Access speed**: Jagged arrays provide 10-30% faster access through simple pointer dereferencing vs. complex offset calculations
- **Cache performance**: Better memory locality for row-wise iteration patterns common in constraint propagation
- **Microsoft guidance**: Aligns with CA1814 analyzer rule recommending jagged arrays for performance-critical scenarios
- **Memory cost**: Minimal overhead (~256 bytes for 64×64 grid) compared to performance benefits in hot WFC loops

**Migration Strategy**: The current codebase requires systematic conversion from multidimensional to jagged arrays across:

- `WfcProvider._possibilities` and `._output` fields
- `ICellEntropyProvider` interface signatures  
- `ChangeLog.RollbackTo()` parameters
- `MappingInformationService` arrays
- Test fixtures and benchmark data structures

The performance improvement in domain access directly supports the target of ≤100ms chunk generation times.

**Null Domain Representation Design Pattern:**

In the current WfcProvider implementation, `_possibilities` uses `HashSet<int>?[][]` where null entries represent collapsed (observed) cells. This design pattern serves several important purposes:

- **Memory efficiency**: Once a cell is observed (assigned a specific tile), its domain HashSet is no longer needed. Setting it to null allows garbage collection to reclaim the memory rather than maintaining single-element HashSets.
- **Clear state indication**: Null provides an unambiguous signal that a cell has been collapsed, distinct from an empty domain (which indicates a contradiction) or a single-element domain (which indicates high constraint but not yet observed).
- **Algorithm optimization**: Constraint propagation can quickly skip null entries without examining their contents, improving iteration performance over large grids.
- **Backtracking support**: The ChangeLog system can distinguish between "restore previous domain" (non-null) and "restore collapsed state" (null) when rolling back decisions.

**Comprehensive Null Entry Cases:**

The `_possibilities` array entries can be null in several distinct scenarios, each with different implications:

1. **Array initialization state**: When `_possibilities = new HashSet<int>?[width][]` is first created, inner arrays are null until explicitly initialized with `_possibilities[x] = new HashSet<int>?[height]`.

2. **Domain initialization failure**: If domain setup fails due to memory constraints or invalid configuration, entries may remain null rather than containing HashSet instances.

3. **Cell observation (primary case)**: When a cell is collapsed during WFC solving, its domain is set to null since the specific tile value is stored in `_output[x][y]`. This is the primary and intentional use of null.

4. **Backtracking restoration**: During state rollback, entries may be temporarily null while the ChangeLog system restores previous domain states, particularly if the original state was collapsed.

5. **Memory pressure handling**: Under extreme memory constraints, the system might proactively null out domains for cells that can be inferred from neighbors, though this optimization is not currently implemented.

6. **Error recovery**: If constraint propagation fails catastrophically (e.g., infinite loops), the system might null out affected cell domains as part of error isolation.

**Important distinction from empty domains**: A null entry means "collapsed cell with value in `_output`" while an empty HashSet means "contradiction detected - no valid tiles possible". The algorithm treats these states very differently:

- **Null check**: `if (_possibilities[x][y] == null)` → cell is solved, read from `_output[x][y]`
- **Empty check**: `if (_possibilities[x][y]?.Count == 0)` → contradiction, trigger backtracking

**Example domain lifecycle:**
1. **Initialization**: `_possibilities[x][y] = new HashSet<int> {0, 1, 2, 3}` (full domain)
2. **Constraint propagation**: `_possibilities[x][y] = new HashSet<int> {1, 2}` (reduced domain)
3. **Observation**: `_possibilities[x][y] = null` (collapsed to specific tile in `_output[x][y]`)
4. **Backtracking**: `_possibilities[x][y] = new HashSet<int> {1, 2}` (restored previous state)

This null-based design requires careful consideration when interfacing with components that expect non-nullable arrays, as documented in the Known Implementation Issues section.

**Implementation Priority**:

1. Core WFC algorithms (highest performance impact)
2. Interface signatures (enables plugin compatibility) 
3. Supporting services (diagnostic and mapping utilities)
4. Test infrastructure (lowest priority, warnings only)

## Implementation guide

Detailed step-by-step implementation guide following Test Driven Development principles where applicable, leading with minimal breaking tests, followed by minimal changes to fix tests, followed by refactor, repeating until the feature is complete.

### Phase 0: Array Migration (Prerequisite)

#### Objective

Migrate all WFC-related data structures from multidimensional arrays to jagged arrays to achieve 10-30% performance improvement in domain access operations and eliminate CA1814 analyzer warnings.

#### Technical details

This foundational phase converts array declarations and access patterns throughout the WFC system. The migration follows a specific order to minimize compilation errors:

1. **Interface updates**: Modify `ICellEntropyProvider` and related interfaces
2. **Core WFC classes**: Update `WfcProvider._possibilities` and `._output` fields  
3. **Algorithm implementations**: Convert `AC3Propagator`, `ChangeLog`, entropy providers
4. **Supporting services**: Update `MappingInformationService` and diagnostic utilities
5. **Test infrastructure**: Convert test fixtures and benchmark data

**Breaking Changes**: This phase introduces breaking changes to public interfaces. All entropy providers, rule tables, and diagnostic utilities must be updated simultaneously.

#### Examples

**Array Migration Comparison:** This example demonstrates the conversion from multidimensional to jagged arrays for WFC domains, showing the syntax changes required and performance benefits achieved.

```csharp
// Before: Multidimensional array declaration and access
private readonly HashSet<int>?[,] _possibilities;
private readonly int[,] _output;

// Initialize
_possibilities = new HashSet<int>?[width, height];
_output = new int[width, height];

// Access
var domain = _possibilities[x, y];
_output[x, y] = selectedTile;
```

```csharp
// After: Jagged array declaration and access  
private readonly HashSet<int>?[][] _possibilities;
private readonly int[][] _output;

// Initialize
_possibilities = new HashSet<int>?[width][];
_output = new int[width][];
for (int x = 0; x < width; x++)
{
    _possibilities[x] = new HashSet<int>?[height];
    _output[x] = new int[height];
}

// Access
var domain = _possibilities[x][y];
_output[x][y] = selectedTile;
```

**Interface Signature Updates:** This example shows how entropy provider interfaces need to be updated to use jagged arrays instead of multidimensional arrays for domain parameters.

```csharp
// Updated interface signatures
public interface ICellEntropyProvider
{
    /// <summary>
    /// Gets entropy score for cell selection heuristics.
    /// </summary>
    /// <param name="possibilities">Domain grid using jagged arrays for performance</param>
    /// <param name="output">Output grid using jagged arrays for performance</param>
    double GetScore(int x, int y, HashSet<int>?[][] possibilities, int[][] output, 
        WfcWeightConfiguration weightConfig);
}
```

### Phase 1: Core Algorithm Enhancement

#### Objective

Replace runtime rule evaluation with precomputed tables and implement proper AC-3 constraint propagation to achieve 70% performance improvement in rule evaluation and proper arc consistency with reduced contradiction rates.

**Prerequisites: Array Migration** - Before implementing AC-3, convert all WFC data structures from multidimensional arrays (`[,]`) to jagged arrays (`[][]`) for optimal performance in hot-path domain operations.

#### Technical details

The current WFC implementation evaluates adjacency rules at runtime for each constraint check, creating performance bottlenecks during the hot path of constraint propagation. This phase introduces precomputed rule tables using BitSet data structures for O(1) lookup operations instead of O(n) rule evaluation.

AC-3 (Arc Consistency 3) algorithm maintains consistency between neighboring cell domains by ensuring every value in a domain has at least one supporting value in adjacent domains. The implementation uses an arc queue to process constraint propagation systematically, detecting contradictions early and reducing backtracking frequency.

Key architectural changes:

- **Rule preprocessing**: Convert TileType adjacency rules into BitSet lookup tables during initialization
- **Domain representation**: Use HashSet<int> for small domains (≤32 tiles), custom BitSet wrapper for larger tile sets
- **Arc queue management**: Efficient queue processing with neighbor enumeration and direction mapping
- **Contradiction detection**: Early termination when domains become empty, triggering backtracking
- **Performance optimization**: Use jagged arrays (`HashSet<int>[][]`) instead of multidimensional arrays (`HashSet<int>[,]`) for 10-30% faster domain access in tight WFC loops

#### Examples

**BitSet Data Structure:** This implementation shows a custom BitSet wrapper around .NET's BitArray, providing efficient set operations for tile ID collections in rule table lookups.

```csharp
// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/BitSet.cs

/// <summary>
/// Efficient bit set implementation for tile ID collections using BitArray.
/// Provides O(1) set operations for rule table lookups.
/// </summary>
/// <remarks>
/// Wraps System.Collections.BitArray with set-like operations for WFC domains.
/// </remarks>
public class BitSet
{
    private readonly BitArray _bits;
    
    public BitSet(int capacity) => _bits = new BitArray(capacity);
    
    /// <summary>
    /// Checks if the specified tile ID is present in this set.
    /// </summary>
    public bool Contains(int tileId) => tileId < _bits.Length && _bits[tileId];
    
    /// <summary>
    /// Adds a tile ID to this set.
    /// </summary>
    public void Add(int tileId) { if (tileId < _bits.Length) _bits[tileId] = true; }
    
    /// <summary>
    /// Performs intersection with another BitSet, modifying this set.
    /// </summary>
    public void IntersectWith(BitSet other) => _bits.And(other._bits);
    
    /// <summary>
    /// Gets all tile IDs present in this set.
    /// </summary>
    public IEnumerable<int> GetTileIds()
    {
        for (int i = 0; i < _bits.Length; i++)
            if (_bits[i]) yield return i;
    }
}
```

**Rule Table Interface:** This interface defines the contract for precomputed rule tables, enabling O(1) adjacency lookups instead of runtime rule evaluation during WFC solving.

```csharp
// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IRuleTable.cs

/// <summary>
/// Precomputed rule table for efficient adjacency lookups during WFC solving.
/// </summary>
/// <remarks>
/// Built once at initialization; avoid allocations during hot path solving.
/// </remarks>
public interface IRuleTable
{
    /// <summary>
    /// Gets allowed neighboring tile IDs for a given tile in a specific direction.
    /// </summary>
    /// <param name="tileId">Source tile ID</param>
    /// <param name="direction">Direction to check (North, South, East, West)</param>
    /// <returns>Custom BitSet of allowed neighbor tile IDs for O(1) intersection operations</returns>
    BitSet GetAllowedNeighbors(int tileId, Direction direction);
}
```

**AC-3 Algorithm Implementation:** This class implements the AC-3 (Arc Consistency 3) constraint propagation algorithm, which systematically maintains consistency between neighboring cell domains to reduce contradictions and improve WFC solution quality.

```csharp
// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/AC3Propagator.cs

/// <summary>
/// AC-3 constraint propagation implementation for WFC domains.
/// </summary>
/// <remarks>
/// Maintains arc consistency between all neighboring cell domains.
/// </remarks>
public class AC3Propagator
{
    private readonly IRuleTable _ruleTable;
    private readonly Queue<(int x, int y, Direction dir)> _arcQueue;
    private readonly HashSet<int>[][] _domains;
    
    /// <summary>
    /// Initializes a new AC-3 propagator with the specified rule table and domain grid.
    /// </summary>
    /// <param name="ruleTable">Precomputed rule table for adjacency lookups</param>
    /// <param name="domains">Reference to the WFC domain grid using jagged arrays for optimal performance</param>
    public AC3Propagator(IRuleTable ruleTable, HashSet<int>[][] domains)
    {
        _ruleTable = ruleTable ?? throw new ArgumentNullException(nameof(ruleTable));
        _domains = domains ?? throw new ArgumentNullException(nameof(domains));
        _arcQueue = new Queue<(int x, int y, Direction dir)>();
    }
    
    /// <summary>
    /// Propagates constraints from a newly collapsed cell to all neighbors.
    /// </summary>
    /// <param name="sourceX">X coordinate of collapsed cell</param>
    /// <param name="sourceY">Y coordinate of collapsed cell</param>
    /// <param name="placedTileId">Tile ID that was placed</param>
    /// <returns>True if propagation succeeded; false if contradiction detected</returns>
    public bool PropagateFrom(int sourceX, int sourceY, int placedTileId)
    {
        // Example: Ocean (ID=0) can only be adjacent to Beach (ID=1) on North/South/East/West
        // Beach (ID=1) can be adjacent to Ocean (ID=0) or Plains (ID=2)
        // Plains (ID=2) can be adjacent to Beach (ID=1) or Forest (ID=3)
        
        // Enqueue arcs from all neighbors back to the collapsed cell
        var neighbors = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // N, E, S, W
        var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
        
        for (int i = 0; i < neighbors.Length; i++)
        {
            var (dx, dy) = neighbors[i];
            var neighborX = sourceX + dx;
            var neighborY = sourceY + dy;
            
            if (IsValidCoordinate(neighborX, neighborY))
            {
                _arcQueue.Enqueue((neighborX, neighborY, directions[i]));
            }
        }
        
        // Process arc consistency
        while (_arcQueue.Count > 0)
        {
            var (x, y, direction) = _arcQueue.Dequeue();
            
            if (RemoveInconsistentValues(x, y, direction))
            {
                if (_domains[x][y]?.Count == 0)
                    return false; // Contradiction detected
                    
                // Re-enqueue arcs from neighbors of (x,y)
                EnqueueNeighborArcs(x, y);
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if the given coordinates are within the domain grid bounds.
    /// </summary>
    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && y >= 0 && x < _domains.Length && y < _domains[x].Length;
    }
    
    /// <summary>
    /// Gets the neighbor position in the specified direction.
    /// </summary>
    private (int x, int y) GetNeighborPosition(int x, int y, Direction direction)
    {
        return direction switch
        {
            Direction.North => (x, y - 1),
            Direction.East => (x + 1, y),
            Direction.South => (x, y + 1),
            Direction.West => (x - 1, y),
            _ => (x, y)
        };
    }
    
    /// <summary>
    /// Enqueues arcs from all neighbors of the given cell for consistency checking.
    /// </summary>
    private void EnqueueNeighborArcs(int x, int y)
    {
        var neighbors = new[] { (0, -1), (1, 0), (0, 1), (-1, 0) }; // N, E, S, W
        var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
        
        for (int i = 0; i < neighbors.Length; i++)
        {
            var (dx, dy) = neighbors[i];
            var neighborX = x + dx;
            var neighborY = y + dy;
            
            if (IsValidCoordinate(neighborX, neighborY))
            {
                // Enqueue arc from neighbor back to current cell
                var oppositeDirection = GetOppositeDirection(directions[i]);
                _arcQueue.Enqueue((neighborX, neighborY, oppositeDirection));
            }
        }
    }
    
    /// <summary>
    /// Gets the opposite direction for constraint checking.
    /// </summary>
    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => direction
        };
    }
    
    private bool RemoveInconsistentValues(int x, int y, Direction direction)
    {
        // Example rule check: if neighbor cell contains Ocean (ID=0),
        // current cell can only contain Beach (ID=1)
        var neighborPos = GetNeighborPosition(x, y, direction);
        if (!IsValidCoordinate(neighborPos.x, neighborPos.y))
            return false;
            
        var currentDomain = _domains[x][y];
        var neighborDomain = _domains[neighborPos.x][neighborPos.y];
        
        // Handle null domains: null means collapsed cell, skip processing
        if (currentDomain == null || neighborDomain == null)
            return false;
            
        var removed = false;
        var tilesToRemove = new List<int>();
        
        foreach (var tileId in currentDomain)
        {
            var allowedNeighbors = _ruleTable.GetAllowedNeighbors(tileId, direction);
            
            // Check if any tile in neighbor domain is allowed
            bool hasSupport = neighborDomain.Any(neighborTile => 
                allowedNeighbors.Contains(neighborTile));
                
            if (!hasSupport)
            {
                tilesToRemove.Add(tileId);
            }
        }
        
        foreach (var tile in tilesToRemove)
        {
            currentDomain.Remove(tile);
            removed = true;
        }
        
        return removed;
    }
}
```

#### Integration with Existing WFC System

The AC3Propagator integrates into the existing WFC architecture by replacing the current constraint propagation logic in `WfcProvider`. This requires coordinated changes across multiple components:

**Architecture Integration Points:**

1. **WfcProvider modification**: Replace ad-hoc constraint checking with AC3Propagator instance
2. **Rule table preprocessing**: Convert TileTypeRegistry rules into IRuleTable format during initialization
3. **Constraint propagation**: Use AC3 algorithm instead of simple neighbor checks
4. **Backtracking integration**: Ensure AC3Propagator state resets properly during backtrack operations

#### Examples

**WfcProvider Integration:** This example demonstrates how the AC3Propagator would integrate into the existing WFC solver. **Note: The actual WfcProvider.cs already contains a comprehensive implementation - this shows the key integration points for AC-3 rather than the complete existing codebase.**

```csharp
// Key integration points for AC3Propagator in existing WfcProvider
public class WfcProvider
{
    protected readonly HashSet<int>?[][] _possibilities;
    protected readonly int[][] _output;
    protected readonly AC3Propagator _propagator;  // ← NEW: Add AC3 propagator
    protected readonly IRuleTable _ruleTable;     // ← NEW: Add rule table
    public int Width { get; }
    public int Height { get; }
    
    public WfcProvider(int width, int height, TileTypeRegistry tileRegistry, 
        IRandomProvider randomProvider, WfcConfiguration config)
    {
        Width = width;
        Height = height;
        _possibilities = new HashSet<int>?[width][];
        _output = new int[width][];
        for (int x = 0; x < width; x++)
        {
            _possibilities[x] = new HashSet<int>?[height];
            _output[x] = new int[height];
        }
        _ruleTable = new PrecomputedRuleTable(tileRegistry);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _possibilities[x][y] = new HashSet<int>();
                for (int tileId = 0; tileId < tileRegistry.TileCount; tileId++)
                {
                    _possibilities[x][y].Add(tileId);
                }
            }
        }
        _propagator = new AC3Propagator(_ruleTable, _possibilities);
    }
    
    // ← MODIFY: Update existing Generate() method to use AC-3
    public virtual bool Generate(int maxIterations = 10000, TimeSpan? timeBudget = null)
    {
        // ... existing iteration and timing logic ...
        while (!_collapsed && iterations < maxIterations)
        {
            // 1. Select cell with minimum entropy (existing logic preserved)
            var (x, y) = FindLowestEntropy();
            // 2. Observe (collapse to single tile) - existing logic
            var selectedTile = ObserveCell(x, y);
            _output[x][y] = selectedTile;
            _possibilities[x][y] = null; // Mark as collapsed
            // 3. ← CHANGE: Replace existing Propagate() with AC-3
            if (!_propagator.PropagateFrom(x, y, selectedTile))
            {
                // Contradiction detected - existing backtracking logic
                if (_enableBacktracking && CanBacktrack())
                {
                    Backtrack();
                    continue;
                }
                return false; // Generation failed
            }
        }
        return true;
    }
    
    // ← CHANGE: Update existing propagation methods or replace with AC-3
    protected bool Propagate(int startX, int startY)
    {
        return _propagator.PropagateFrom(startX, startY, _output[startX][startY]);
    }
}
```

**Current Implementation Status:** The actual [WfcProvider.cs](../../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs) already contains:

- ✅ Complete jagged array structure (`HashSet<int>?[][]`)
- ✅ Comprehensive Generate() methods (with and without backtracking)
- ✅ Advanced entropy-based cell selection with multiple heuristics
- ✅ Weighted tile selection with neighbor matching
- ✅ Change logging for backtracking support
- ❌ **Missing: AC3Propagator integration** (currently uses ad-hoc constraint checking)
- ❌ **Missing: Precomputed rule tables** (currently evaluates rules at runtime)

**Code Quality Opportunities in Current Implementation:**

**Generate Method Duplication Analysis:**
The current WfcProvider has significant code duplication between the two Generate() overloads:

1. **Duplicate initialization logic**: Both methods repeat identical setup for performance logging, timing, and success tracking
2. **Duplicate iteration control**: Similar while loop structure with iteration counting and time budget checking  
3. **Duplicate entropy and propagation calls**: Both use identical `FindLowestEntropy()`, `CollapseCell()`, and `Propagate()` call patterns
4. **Duplicate cleanup logic**: Both have identical finally blocks for performance event logging

**Refactoring Opportunities:**

1. **Extract common generation loop**: Create `GenerateCore(GenerationContext context)` helper method containing the shared iteration logic
2. **Consolidate initialization**: Create `InitializeGeneration(bool enableBacktracking, TimeSpan? timeBudget)` helper
3. **Unify decision handling**: Extract decision frame creation and candidate ordering into helper methods
4. **Simplify method signatures**: Make the simpler Generate() method delegate to the full-featured version:

   ```csharp
   public bool Generate(int maxIterations = 10000, TimeSpan? timeBudget = null)
   {
       return Generate(false, maxIterations, null, null, timeBudget);
   }
   ```

**Estimated Impact:** Refactoring could reduce the Generate methods from ~300 lines to ~150 lines while improving maintainability and reducing the risk of behavior divergence between the two approaches.

**Priority:** Medium - This is a code quality improvement that should be addressed after the core AC-3 functionality is implemented.

**Rule Table Implementation:** This class shows how to convert TileTypeRegistry adjacency rules into efficient BitSet lookup tables during initialization, eliminating runtime rule evaluation costs.

```csharp
/// <summary>
/// Precomputed rule table implementation that converts TileTypeRegistry adjacency rules 
/// into efficient BitSet lookup tables for O(1) constraint checking during WFC solving.
/// </summary>
/// <remarks>
/// Built once during initialization to eliminate runtime rule evaluation costs.
/// Uses BitSet data structures for efficient set operations on tile ID collections.
/// </remarks>
public class PrecomputedRuleTable : IRuleTable
{
    private readonly Dictionary<(int tileId, Direction dir), BitSet> _allowedNeighbors;
    
    /// <summary>
    /// Initializes a new precomputed rule table from the specified tile registry.
    /// </summary>
    /// <param name="registry">Tile registry containing adjacency rules to precompute</param>
    /// <exception cref="ArgumentNullException">Thrown when registry is null</exception>
    public PrecomputedRuleTable(TileTypeRegistry registry)
    {
        _allowedNeighbors = new Dictionary<(int, Direction), BitSet>();
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        PrecomputeAllRules(registry);
    }
    
    /// <summary>
    /// Gets the set of allowed neighboring tile IDs for a given tile in a specific direction.
    /// </summary>
    /// <param name="tileId">Source tile ID to check neighbors for</param>
    /// <param name="direction">Direction to check (North, South, East, West)</param>
    /// <returns>BitSet containing allowed neighbor tile IDs; empty set if no constraints</returns>
    public BitSet GetAllowedNeighbors(int tileId, Direction direction)
    {
        return _allowedNeighbors.GetValueOrDefault((tileId, direction), new BitSet(0));
    }
    
    /// <summary>
    /// Precomputes all adjacency rules by testing every tile-direction-neighbor combination
    /// and storing results in efficient BitSet lookup tables.
    /// </summary>
    /// <param name="registry">Tile registry containing rules to evaluate</param>
    /// <remarks>
    /// Creates TileRuleContext objects with default values for testing basic adjacency rules
    /// without runtime-specific data like height samples or mapping information.
    /// </remarks>
    private void PrecomputeAllRules(TileTypeRegistry registry)
    {
        // Convert TileType adjacency rules into efficient BitSet lookups
        var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
        
        for (int tileId = 0; tileId < registry.TileCount; tileId++)
        {
            var tileType = registry.GetTileType(tileId);
            
            foreach (var direction in directions)
            {
                var allowedSet = new BitSet(registry.TileCount);
                
                // Test each potential neighbor tile
                for (int neighborId = 0; neighborId < registry.TileCount; neighborId++)
                {
                    // Create full context with default values for precomputation
                    // This tests basic adjacency rules without runtime-specific data
                    var defaultHeight = new HeightSample
                    {
                        Altitude = 0.5f,
                        MountainNoise = 0.0f,
                        DetailNoise = 0.0f
                    };
                    
                    var context = new TileRuleContext(
                        CandidatePosition: new TilePoint(0, 0),
                        CandidateTileId: tileId,
                        NeighborPosition: GetNeighborPosition(direction),
                        NeighborTileId: neighborId,
                        DirectionToNeighbor: direction,
                        Config: new TerrainRuleConfiguration(),
                        CandidateHeight: defaultHeight,
                        NeighborHeight: defaultHeight,
                        MappingService: new MappingInformationService(new int[1][])
                    );
                    
                    if (tileType.EvaluateRules(context))
                    {
                        allowedSet.Add(neighborId);
                    }
                }
                
                _allowedNeighbors[(tileId, direction)] = allowedSet;
            }
        }
    }
    
    /// <summary>
    /// Gets the neighbor position coordinates in the specified direction from origin (0,0).
    /// </summary>
    /// <param name="direction">Direction to get neighbor position for</param>
    /// <returns>TilePoint representing neighbor position relative to origin</returns>
    /// <remarks>
    /// Used during rule precomputation to create consistent TileRuleContext objects.
    /// </remarks>
    private static TilePoint GetNeighborPosition(Direction direction)
    {
        return direction switch
        {
            Direction.North => new TilePoint(0, -1),
            Direction.South => new TilePoint(0, 1),
            Direction.East => new TilePoint(1, 0),
            Direction.West => new TilePoint(-1, 0),
            _ => new TilePoint(0, 0)
        };
    }
}
```

**Boundary Constraints Integration:** This enhanced WFC provider demonstrates how to integrate boundary constraints for seamless chunk generation, applying neighbor constraints before running AC-3 propagation.

### Integration Issues and Considerations

### Suggested Code Fixes for EnhancedWfcProvider Integration

To enable EnhancedWfcProvider to extend WfcProvider cleanly, apply the following changes to the WfcProvider base class:

- **Constructor Signature:**
  - Add a constructor to WfcProvider that takes (int width, int height, TileTypeRegistry tileRegistry, IRandomProvider randomProvider, WfcConfiguration config) if not already present, or update EnhancedWfcProvider to match the actual constructor signature.

- **Member Visibility:**
  - Change the visibility of _propagator and _possibilities from private to protected (or provide protected/internal properties or methods to access them) so that subclasses can use them for advanced behaviors.

- **Expose Dimensions:**
  - Add protected or public properties for Width and Height in WfcProvider if they are needed by subclasses (e.g., for boundary propagation or diagnostics).

- **Extensibility Pattern:**
  - For any member or method that is intended to be used or overridden by subclasses, use protected visibility and provide XML documentation describing its intended use.

These changes will make the WFC system more extensible and allow for advanced features such as boundary-aware chunk generation and diagnostics in derived classes.

```csharp
// Integration with boundary constraints for chunk seaming
/// <summary>
/// Sample configuration class aggregating all WFC-related settings for chunk generation.
/// </summary>
public class WfcConfiguration
{
    /// <summary>
    /// Tile selection weights.
    /// </summary>
    public WfcWeightConfiguration Weights { get; set; } = new WfcWeightConfiguration();

    /// <summary>
    /// Heuristic and entropy settings.
    /// </summary>
    public HeuristicsConfiguration Heuristics { get; set; } = new HeuristicsConfiguration();

    /// <summary>
    /// Time budget for WFC generation (in milliseconds).
    /// </summary>
    public int TimeBudgetMs { get; set; } = 50;

    /// <summary>
    /// Additional configuration fields as needed.
    /// </summary>
    // public ...
}
/// <summary>
/// Enhanced WFC provider integrating advanced boundary constraint extraction and application for seamless chunk generation.
/// </summary>
/// <remarks>
/// Ensures boundaries between adjacent chunks are consistent by applying neighbor constraints before AC-3 propagation.
/// Supports partial neighbor constraints and deterministic domain restriction for robust terrain seaming.
/// </remarks>
public class EnhancedWfcProvider : WfcProvider
{
    private readonly IBoundaryConstraintProvider _boundaryProvider;
    private readonly bool _enableValidation;

    /// <summary>
    /// Initializes a new instance of EnhancedWfcProvider.
    /// </summary>
    /// <param name="width">Chunk width in tiles.</param>
    /// <param name="height">Chunk height in tiles.</param>
    /// <param name="tileRegistry">Tile type registry.</param>
    /// <param name="randomProvider">Random provider for deterministic generation.</param>
    /// <param name="config">WFC configuration.</param>
    /// <param name="boundaryProvider">Boundary constraint provider.</param>
    /// <param name="enableValidation">If true, validates chunk seams after generation.</param>
    public EnhancedWfcProvider(
        int width,
        int height,
        TileTypeRegistry tileRegistry,
        IRandomProvider randomProvider,
        WfcConfiguration config,
        IBoundaryConstraintProvider boundaryProvider,
        bool enableValidation = false)
        : base(width, height, tileRegistry, randomProvider, config)
    {
        _boundaryProvider = boundaryProvider ?? throw new ArgumentNullException(nameof(boundaryProvider));
        _enableValidation = enableValidation;
    }
    
    public bool GenerateWithBoundaries(Dictionary<Point, Chunk> neighborChunks, 
        Point currentChunkCoords)
    {
        // Apply boundary constraints before generation
        ApplyBoundaryConstraints(neighborChunks, currentChunkCoords);
        // Run standard AC-3 generation
        var success = Generate();
        // Validate seam consistency (optional verification step)
        if (success && _enableValidation)
        {
            ValidateChunkSeams(neighborChunks, currentChunkCoords);
        }
        return success;
    }

    /// <summary>
    /// Verifies that tiles along shared chunk boundaries match adjacency rules and logs any mismatches.
    /// </summary>
    /// <param name="neighborChunks">Dictionary of neighboring chunks keyed by their coordinates.</param>
    /// <param name="currentChunkCoords">Coordinates of the current chunk.</param>
    private void ValidateChunkSeams(Dictionary<Point, Chunk> neighborChunks, Point currentChunkCoords)
    {
        foreach (var kvp in neighborChunks)
        {
            var neighborCoords = kvp.Key;
            var neighborChunk = kvp.Value;
            var sharedEdge = GetSharedEdge(currentChunkCoords, neighborCoords);
            if (sharedEdge == null) continue;

            var currentBoundary = ExtractBoundaryTiles(this, sharedEdge.Value, true);
            var neighborBoundary = ExtractBoundaryTiles(neighborChunk, GetOppositeDirection(sharedEdge.Value), false);

            for (int i = 0; i < currentBoundary.Length; i++)
            {
                if (!AdjacencyRulesMatch(currentBoundary[i], neighborBoundary[i], sharedEdge.Value))
                {
                    LogBoundaryMismatch(currentChunkCoords, neighborCoords, sharedEdge.Value, i, currentBoundary[i], neighborBoundary[i]);
                }
            }
        }
    }

    /// <summary>
    /// Determines which edge is shared between two chunk coordinates.
    /// </summary>
    /// <param name="a">Coordinates of the first chunk.</param>
    /// <param name="b">Coordinates of the second chunk.</param>
    /// <returns>The shared edge direction, or null if not adjacent.</returns>
    private Direction? GetSharedEdge(Point a, Point b)
    {
        if (a.X == b.X && a.Y == b.Y + 1) return Direction.North;
        if (a.X == b.X && a.Y == b.Y - 1) return Direction.South;
        if (a.X == b.X + 1 && a.Y == b.Y) return Direction.West;
        if (a.X == b.X - 1 && a.Y == b.Y) return Direction.East;
        return null;
    }

    /// <summary>
    /// Extracts tile IDs along the specified edge from a chunk or provider.
    /// </summary>
    /// <param name="chunkOrProvider">Chunk or provider to extract from.</param>
    /// <param name="edge">Edge to extract.</param>
    /// <param name="isCurrent">True if extracting from the current chunk, false for neighbor.</param>
    /// <returns>Array of tile IDs along the edge.</returns>
    private int[] ExtractBoundaryTiles(object chunkOrProvider, Direction edge, bool isCurrent)
    {
        // Example: extract tile IDs along the specified edge
        // Replace with actual chunk access in real code
        return new int[Chunk.ChunkSize];
    }

    /// <summary>
    /// Checks if two tiles match adjacency rules for a given edge.
    /// </summary>
    /// <param name="tileA">Tile ID from the current chunk.</param>
    /// <param name="tileB">Tile ID from the neighbor chunk.</param>
    /// <param name="edge">Edge direction being checked.</param>
    /// <returns>True if adjacency rules are satisfied; otherwise, false.</returns>
    private bool AdjacencyRulesMatch(int tileA, int tileB, Direction edge)
    {
        // Replace with actual rule table lookup
        return true;
    }

    /// <summary>
    /// Gets the opposite direction for a given edge.
    /// </summary>
    /// <param name="dir">Direction to invert.</param>
    /// <returns>Opposite direction.</returns>
    private Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => dir
        };
    }

    /// <summary>
    /// Logs a boundary mismatch between two chunks at a specific edge and position.
    /// </summary>
    /// <param name="chunkA">Coordinates of the first chunk.</param>
    /// <param name="chunkB">Coordinates of the second chunk.</param>
    /// <param name="edge">Edge direction where mismatch occurred.</param>
    /// <param name="position">Position along the edge.</param>
    /// <param name="tileA">Tile ID from the first chunk.</param>
    /// <param name="tileB">Tile ID from the second chunk.</param>
    private void LogBoundaryMismatch(Point chunkA, Point chunkB, Direction edge, int position, int tileA, int tileB)
    {
        // Replace with actual logging
        Console.WriteLine($"Boundary mismatch at edge {edge} position {position}: chunk {chunkA} tile {tileA} vs chunk {chunkB} tile {tileB}");
    }
    
    private void ApplyBoundaryConstraints(Dictionary<Point, Chunk> neighbors, Point coords)
    {
        var neighborOffsets = new[]
        {
            (new Point(0, -1), Direction.North),
            (new Point(1, 0), Direction.East),
            (new Point(0, 1), Direction.South),
            (new Point(-1, 0), Direction.West)
        };
        
        foreach (var (offset, direction) in neighborOffsets)
        {
            var neighborPos = coords + offset;
            if (neighbors.TryGetValue(neighborPos, out var chunk))
            {
                var constraints = _boundaryProvider.ExtractConstraints(chunk, direction);
                _boundaryProvider.ApplyConstraints(_possibilities, constraints);
            }
        }
        
        // Important: Run initial propagation after applying boundary constraints
        // This ensures constraint consistency before starting main generation
        PropagateInitialConstraints();
    }
    
    private void PropagateInitialConstraints()
    {
        // Propagate from all boundary cells that have been constrained
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_possibilities[x][y]?.Count == 1)
                {
                    // Single-domain cell acts as initial constraint
                    var constrainedTile = _possibilities[x][y].First();
                    if (!_propagator.PropagateFrom(x, y, constrainedTile))
                    {
                        throw new InvalidOperationException(
                            $"Boundary constraints created contradiction at ({x},{y})");
                    }
                }
            }
        }
    }
}
```

**Key Integration Benefits:**

- **Systematic propagation**: AC-3 ensures thorough constraint checking vs. ad-hoc neighbor validation
- **Early contradiction detection**: Reduces wasted computation in unsolvable states  
- **Boundary compatibility**: Works seamlessly with chunk seam constraints
- **Performance improvement**: Precomputed rule tables eliminate runtime rule evaluation
- **Backtracking support**: Domain-based design integrates naturally with state restoration

**Migration Path:**

1. Implement `IRuleTable` and `PrecomputedRuleTable` alongside existing `TileTypeRegistry`
2. Create `AC3Propagator` and integrate into `WfcProvider.Generate()` method
3. Replace existing constraint propagation calls with `_propagator.PropagateFrom()`
4. Test boundary constraint integration with chunk generation workflow
5. Benchmark performance improvements and validate correctness against existing implementation

**Known Implementation Issues:**

**Nullability Compatibility Issue**: The current WfcProvider uses `HashSet<int>?[][]` for the `_possibilities` field (nullable arrays) since elements are set to `null` when cells collapse. However, the planned AC3Propagator constructor expects `HashSet<int>[][]` (non-nullable arrays), creating a compilation error.

**Possible Solutions:**
1. **Modify AC3Propagator**: Change constructor parameter to `HashSet<int>?[][]` and handle null checks internally
2. **Alternative collapsed representation**: Use empty HashSet instead of null for collapsed cells in WfcProvider
3. **Wrapper approach**: Create a non-nullable view of the possibilities array for AC3Propagator

**Recommended Solution:** Option 2 (empty HashSet) provides the cleanest interface while maintaining performance, as the AC3Propagator can treat empty sets as collapsed cells without special null handling.

### Phase 2: Chunk Seam Consistency

#### Objective

Implement boundary constraint system to ensure 100% consistency between adjacent chunk boundaries with no visual seams or logical contradictions.

#### Technical details

Chunk boundary consistency is achieved by extracting tile constraints from already-generated neighboring chunks and applying them as domain restrictions before WFC solving begins. This prevents the WFC algorithm from selecting tiles that would create visual discontinuities or logical inconsistencies across chunk boundaries.

The system operates in two phases:

1. **Constraint extraction**: Read boundary tiles from existing neighbor chunks and create constraint objects
2. **Domain restriction**: Apply constraints by limiting WFC domains to only compatible tiles along boundaries

Boundary mapping logic handles coordinate transformation between chunk-local coordinates and world coordinates, ensuring proper alignment across different chunk generation orders. The system supports partial neighbor constraints when not all neighboring chunks exist yet.

Key technical considerations:

- **Deterministic generation**: Boundary constraints must not affect generation determinism
- **Memory efficiency**: Constraint objects are lightweight structs to minimize allocation overhead
- **Flexible boundaries**: Support for incomplete neighbor sets during initial world generation
- **Rule compatibility**: Boundary constraints work seamlessly with existing adjacency rule system

#### Examples

**Boundary Constraint Interface:** This interface and supporting classes demonstrate how to extract tile constraints from neighboring chunks and apply them to ensure seamless boundaries between generated chunks.

```csharp
// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/IBoundaryConstraintProvider.cs

/// <summary>
/// Manages boundary constraints between adjacent chunks for seamless terrain.
/// </summary>
/// <remarks>
/// Extracts constraints from neighboring chunks before WFC initialization.
/// </remarks>
public interface IBoundaryConstraintProvider
{
    /// <summary>
    /// Extracts boundary constraints from an already-generated neighboring chunk.
    /// </summary>
    /// <param name="neighborChunk">Source chunk to extract constraints from</param>
    /// <param name="sharedEdge">Which edge is shared (North, South, East, West)</param>
    /// <returns>Array of tile constraints for the shared boundary</returns>
    BoundaryConstraint[] ExtractConstraints(Chunk neighborChunk, Direction sharedEdge);
    
    /// <summary>
    /// Applies boundary constraints to WFC domains before solving begins.
    /// </summary>
    /// <param name="domains">WFC domain grid to constrain (nullable jagged array matching WfcProvider pattern)</param>
    /// <param name="constraints">Boundary constraints to apply</param>
    void ApplyConstraints(HashSet<int>?[][] domains, BoundaryConstraint[] constraints);
}

/// <summary>
/// Represents a constraint for a single position along a chunk boundary.
/// </summary>
public struct BoundaryConstraint
{
    public int Position { get; init; }        // 0-63 position along boundary
    public int RequiredTileId { get; init; }  // Tile that must be placed
    public Direction Side { get; init; }      // Which boundary edge this applies to
}

/// <summary>
/// Default implementation of boundary constraint management.
/// </summary>
/// <remarks>
/// Ensures seamless transitions between chunks by constraining WFC domains.
/// </remarks>
public class BoundaryConstraintProvider : IBoundaryConstraintProvider
{
    public BoundaryConstraint[] ExtractConstraints(Chunk neighborChunk, Direction sharedEdge)
    {
        var constraints = new BoundaryConstraint[Chunk.ChunkSize];
        
        // Example: Extract from North neighbor's South edge
        if (sharedEdge == Direction.North)
        {
            // Get tiles along neighbor's bottom row (y = ChunkSize-1)
            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                var tileId = neighborChunk[x, Chunk.ChunkSize - 1];
                constraints[x] = new BoundaryConstraint
                {
                    Position = x,
                    RequiredTileId = tileId,
                    Side = Direction.North
                };
            }
        }
        else if (sharedEdge == Direction.East)
        {
            // Get tiles along neighbor's left column (x = 0)
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                var tileId = neighborChunk[0, y];
                constraints[y] = new BoundaryConstraint
                {
                    Position = y,
                    RequiredTileId = tileId,
                    Side = Direction.East
                };
            }
        }
        // Similar logic for South and West...
        
        return constraints;
    }
    
    public void ApplyConstraints(HashSet<int>?[][] domains, BoundaryConstraint[] constraints)
    {
        foreach (var constraint in constraints)
        {
            int x, y;
            
            // Convert constraint to domain grid coordinates
            switch (constraint.Side)
            {
                case Direction.North:
                    x = constraint.Position;
                    y = 0; // Top row of current chunk
                    break;
                case Direction.East:
                    x = Chunk.ChunkSize - 1; // Right column
                    y = constraint.Position;
                    break;
                case Direction.South:
                    x = constraint.Position;
                    y = Chunk.ChunkSize - 1; // Bottom row
                    break;
                case Direction.West:
                    x = 0; // Left column
                    y = constraint.Position;
                    break;
                default:
                    continue;
            }
            
            // Constrain domain to only the required tile (handle nullable arrays)
            if (domains[x][y] != null)
            {
                domains[x][y]!.Clear();
                domains[x][y]!.Add(constraint.RequiredTileId);
            }
        }
    }
}
```

**Chunk Generation Workflow:** This example shows how to integrate boundary constraint extraction and application into the complete chunk generation process, ensuring seamless terrain across chunk boundaries.

```csharp
/// <summary>
/// Example usage in chunk generation workflow.
/// </summary>
/// <remarks>
/// Demonstrates integration with ChunkedTilemap generation process.
/// </remarks>
public class SeamlessChunkGenerator
{
    private readonly IBoundaryConstraintProvider _boundaryProvider;
    private readonly IRuleTable _ruleTable;
    
    public bool GenerateChunk(Point chunkCoords, Dictionary<Point, Chunk> existingChunks)
    {
        var domains = InitializeDomains(); // All tiles possible initially
        var appliedConstraints = new List<BoundaryConstraint>();
        
        // Apply constraints from existing neighbors
        var neighborOffsets = new[]
        {
            (new Point(0, -1), Direction.North),  // North neighbor
            (new Point(1, 0), Direction.East),   // East neighbor  
            (new Point(0, 1), Direction.South),  // South neighbor
            (new Point(-1, 0), Direction.West)   // West neighbor
        };
        
        foreach (var (offset, direction) in neighborOffsets)
        {
            var neighborPos = chunkCoords + offset;
            if (existingChunks.TryGetValue(neighborPos, out var neighborChunk))
            {
                var constraints = _boundaryProvider.ExtractConstraints(neighborChunk, direction);
                _boundaryProvider.ApplyConstraints(domains, constraints);
                appliedConstraints.AddRange(constraints);
                
                // Example: if North neighbor has Ocean at position (32, 63),
                // current chunk position (32, 0) can only be Beach or Ocean
                // based on adjacency rules
            }
        }
        
        // Run WFC with pre-constrained domains
        var solver = new WfcProvider(Chunk.ChunkSize, Chunk.ChunkSize, _ruleTable,
            new Random(), domains);
            
        return solver.Generate();
    }
}
```

### Phase 3: Performance Optimization

#### Objective

Achieve 60% reduction in memory allocations and 80% reduction in height-related computations through caching and optimization strategies while maintaining 95% time budget compliance.

#### Technical details

Performance optimization focuses on eliminating repeated computations and memory allocations in WFC hot paths. Height sampling, which drives terrain rule evaluation, represents a significant computational cost that can be amortized through intelligent caching strategies.

Caching architecture implements multi-level caching:

- **Chunk-level caching**: Precompute all height samples for a 64×64 chunk area when first accessed
- **Spatial locality**: Cache neighboring chunk samples to support boundary constraint calculations
- **LRU eviction**: Remove least recently used chunk caches when memory pressure increases
- **Dirty tracking**: Invalidate caches only when underlying height parameters change

Time budget management adapts to runtime performance characteristics:

- **Historical analysis**: Track generation times and adjust budget allocation across WFC phases
- **Progressive quality**: Implement fallback algorithms when WFC exceeds time budget
- **Early termination**: Detect partial solutions that meet minimum quality thresholds
- **Adaptive heuristics**: Switch to faster heuristics under time pressure

Memory optimization strategies:

- **Object pooling**: Reuse domain HashSet instances across multiple WFC runs
- **Struct optimization**: Use value types for frequently allocated objects like coordinates
- **Lazy initialization**: Defer expensive computations until actually needed

#### Examples

**Performance Optimization Classes:** These classes demonstrate caching strategies and time budget management for optimizing WFC performance, including chunk-scoped height sample caching and adaptive time allocation.

```csharp
// TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/CachedHeightProvider.cs

/// <summary>
/// Cached height provider with chunk-scoped caching for WFC performance.
/// </summary>
/// <remarks>
/// Caches height samples per chunk; invalidated when chunk unloads.
/// </remarks>
public class CachedHeightProvider : IHeightProvider
{
    private readonly Dictionary<Point, HeightSample[,]> _chunkCache;
    
    /// <summary>
    /// Gets height sample with automatic caching for performance.
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>Cached height sample; computed on first access</returns>
    public HeightSample GetSample(int worldX, int worldY)
    {
        var chunkCoords = new Point(worldX / 64, worldY / 64);
        if (!_chunkCache.ContainsKey(chunkCoords))
        {
            PrecomputeChunkSamples(chunkCoords);
        }
        // Return from cache
        return _chunkCache[chunkCoords][worldX % 64, worldY % 64];
    }
}

/// <summary>
/// Adaptive time budget manager for real-time WFC generation.
/// </summary>
/// <remarks>
/// Learns from historical performance to optimize budget allocation.
/// </remarks>
public class TimeBudgetManager
{
    /// <summary>
    /// Allocates time budget across WFC phases based on historical data.
    /// </summary>
    /// <param name="totalBudget">Total time budget for chunk generation</param>
    /// <returns>Budget allocation for initialization, solving, cleanup phases</returns>
    public BudgetAllocation AllocateBudget(TimeSpan totalBudget)
    {
        // Use historical performance data to optimize allocation
        return new BudgetAllocation(totalBudget);
    }
}
```

### Phase 4: Library Abstraction

#### Objective

Create reusable WFC library that can solve non-tile problems and supports plugin architecture for custom rule types, enabling use in production strategy games.

#### Technical details

Library abstraction separates the core WFC algorithm from terrain-specific implementation details, creating a generic constraint satisfaction framework. This enables reuse for other procedural generation problems like building layouts, quest generation, or resource placement.

Generic design principles:

- **Type parameterization**: Generic solver interfaces supporting arbitrary cell and value types
- **Constraint abstraction**: Rule systems independent of specific domain knowledge (tiles, building blocks, etc.)
- **Plugin architecture**: Extensible entropy providers, constraint validators, and heuristic strategies
- **Configuration injection**: Dependency injection for all algorithm components

The plugin system enables runtime customization:

- **Entropy providers**: Custom cell selection strategies (Shannon entropy, domain size, spatial preferences)
- **Constraint providers**: Domain-specific rule systems (adjacency, distance, resource constraints)
- **Heuristic providers**: Tie-breaking strategies and optimization preferences
- **Diagnostic providers**: Custom performance monitoring and debugging hooks

Library structure supports multiple consumption patterns:

- **Embedded usage**: Direct integration into game engines with minimal overhead
- **Service oriented**: REST API wrapper for multi-language integration
- **Batch processing**: Command-line tools for offline content generation
- **Editor integration**: Real-time preview and editing capabilities

#### Examples

**Generic Library Interfaces:** These interfaces demonstrate the design for a reusable WFC library that can solve non-tile problems, with generic type parameters and plugin architecture for custom entropy providers.

```csharp
// TerrainGeneration2D.WFC/IWfcSolver.cs

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
    /// <param name="configuration">Solver configuration and constraints</param>
    /// <returns>Solution if found; null if unsatisfiable within constraints</returns>
    WfcSolution<TCell, TValue>? Solve(WfcConfiguration<TCell, TValue> configuration);
}

/// <summary>
/// Plugin interface for custom entropy calculation strategies.
/// </summary>
public interface IEntropyProviderPlugin
{
    string Name { get; }
    string Description { get; }
    
    /// <summary>
    /// Calculate entropy score for cell selection heuristics.
    /// </summary>
    /// <param name="x">Cell X coordinate</param>
    /// <param name="y">Cell Y coordinate</param>
    /// <param name="domains">Current domain state (jagged array for performance)</param>
    /// <param name="context">Additional context for calculation</param>
    /// <returns>Entropy score; lower scores selected first</returns>
    double CalculateEntropy(int x, int y, HashSet<int>[][] domains, EntropyContext context);
}
```

### Phase 5: Comprehensive Testing

#### Objective

Achieve ≥95% code coverage with comprehensive unit, integration, property-based, and performance regression testing to ensure production readiness.

#### Technical details

Comprehensive testing strategy validates both algorithmic correctness and performance characteristics across the entire WFC implementation. Testing architecture covers multiple validation levels from unit-level algorithm verification to end-to-end integration scenarios.

Property-based testing validates fundamental WFC guarantees:

- **Constraint satisfaction**: All generated outputs must satisfy adjacency rules regardless of input complexity
- **Completeness**: Algorithm terminates with valid solution or explicit failure indication
- **Determinism**: Identical inputs produce identical outputs across platforms and runs
- **Performance bounds**: Generation time scales predictably within documented limits

Performance regression testing prevents algorithmic degradation:

- **Baseline establishment**: Record performance metrics for core algorithm components
- **Automated benchmarking**: Continuous integration runs benchmark suites on every change
- **Statistical analysis**: Use percentile-based performance validation (95th percentile under budget)
- **Memory profiling**: Track allocation patterns and garbage collection behavior

Integration testing validates real-world scenarios:

- **Multi-chunk generation**: Test seam consistency across various chunk generation orders
- **Save/load persistence**: Verify deterministic regeneration from saved state
- **Configuration variations**: Test across different rule sets, tile counts, and constraint densities
- **Error recovery**: Validate graceful handling of invalid inputs and resource exhaustion

#### Examples

**Property-Based Testing:** This example shows property-based testing for WFC, validating that constraint satisfaction properties hold across all generated solutions regardless of input complexity.

```csharp
// TerrainGeneration2D.Tests/WFC/PropertyBasedWfcTests.cs
// Class: PropertyBasedWfcTests

/// <summary>
/// Property-based test fixture for WFC constraint satisfaction properties.
/// </summary>
[Fact]
public void WfcSolver_AlwaysSatisfiesConstraints_ForValidInputs()
{
    // Property: All generated solutions must satisfy adjacency rules
    Prop.ForAll(GenerateValidRuleSet(), GenerateValidConfiguration(),
        (rules, config) =>
        {
            var solver = new WfcProvider(config.Width, config.Height, rules,
                new DeterministicRandom(42), config);
            if (solver.Generate())
            {
                var output = solver.GetOutput();
                return VerifyAllConstraintsSatisfied(output, rules);
            }
            return true; // Failure is acceptable; constraint violation is not
        });
}

**Performance Regression Testing:** This test demonstrates how to validate that WFC performance remains within acceptable bounds, using statistical analysis to ensure 95% of generation attempts complete within the time budget.

```csharp
// TerrainGeneration2D.Tests/WFC/PerformanceRegressionTests.cs
// Class: PerformanceRegressionTests

/// <summary>
/// Performance regression test for chunk generation timing.
/// </summary>
[Fact]
public void ChunkGeneration_CompletesWithinTimeBudget_Under95PercentOfCases()
{
    var timings = new List<TimeSpan>();
    
    for (int i = 0; i < 1000; i++)
    {
        var stopwatch = Stopwatch.StartNew();
        var success = GenerateChunk(seed: i);
        stopwatch.Stop();
        
        if (success) timings.Add(stopwatch.Elapsed);
    }
    
    var percentile95 = timings.OrderBy(t => t).Skip((int)(timings.Count * 0.95)).First();
    Assert.True(percentile95 < TimeSpan.FromMilliseconds(100),
        $"95th percentile timing {percentile95.TotalMilliseconds}ms exceeds 100ms budget");
}
```

### Phase 6: Documentation and Onboarding

#### Objective

Provide comprehensive documentation enabling developers unfamiliar with WFC to become productive within 2 weeks, with systematic performance tuning guidance.

#### Technical details

Documentation strategy targets multiple developer skill levels and use cases, from newcomers learning constraint satisfaction concepts to experienced developers optimizing performance for production deployments.

Layered documentation approach:

- **Conceptual guides**: WFC algorithm explanation with visual examples and step-by-step walkthroughs
- **API reference**: Complete interface documentation with parameter explanations and usage examples
- **Implementation guides**: End-to-end tutorials for common integration scenarios
- **Performance guides**: Systematic optimization strategies with before/after benchmarks
- **Troubleshooting guides**: Common issues, diagnostic techniques, and resolution strategies

Interactive learning materials:

- **Minimal examples**: Simple, compilable demonstrations of core concepts
- **Progressive complexity**: Tutorial series building from basic to advanced usage
- **Visual debugging**: Tools to visualize WFC state progression and constraint propagation
- **Performance profiling**: Guided exercises in optimization and bottleneck identification

Production deployment guidance:

- **Integration patterns**: Best practices for game engine integration and lifecycle management
- **Configuration tuning**: Systematic approach to parameter optimization for specific use cases
- **Monitoring setup**: Diagnostic configuration and performance metric interpretation
- **Scaling strategies**: Multi-threading, caching, and memory management for large-scale deployment

#### Examples

**Basic Usage Example:** This example provides a simple, step-by-step demonstration of how to set up and use the WFC system for terrain generation, serving as an entry point for developers new to the library.

```csharp
// Example implementation
// Class: WfcExampleSetup

/// <summary>
/// Example: Basic WFC setup for terrain generation
/// </summary>
public void BasicWfcSetup_Example()
{
    // Step 1: Define tile types and adjacency rules
    var tileRegistry = new TileTypeRegistry(new[]
    {
        new TileType(0, "Ocean") { /* rules */ },
        new TileType(1, "Beach") { /* rules */ },
        new TileType(2, "Plains") { /* rules */ }
    });
    
    // Step 2: Configure WFC solver
    var config = new WfcConfiguration
    {
        Width = 64,
        Height = 64,
        TimeBudget = TimeSpan.FromMilliseconds(50),
        EnableBacktracking = true
    };
    
    // Step 3: Create and run solver
    var solver = new WfcProvider(config.Width, config.Height, tileRegistry,
        new Random(12345), config);
    if (solver.Generate())
    {
        var result = solver.GetOutput();
        // Use result for chunk data
    }
}
