# WFC Backtracking Robustness Plan

## Overview

This plan describes enhancements to the Wave Function Collapse (WFC) backtracking mechanism to ensure robust handling of singleton domains, contradictions, and decision stack unwinding. The goal is to guarantee that the solver can recover from propagation-induced contradictions, even when domains are restricted to a single candidate, and always explores all possible solutions without getting stuck in unrecoverable states.

**Background:** The current WFC implementation includes basic backtracking support through the ChangeLog system, which records domain changes for rollback. However, there are edge cases where singleton domains (domains with only one possible value) cause contradictions during propagation that may not be properly handled by the current decision stack logic.

**Primary Improvements:**

- **Singleton Domain Handling**: Ensure all cell assignments, including forced assignments from singleton domains, are tracked on the decision stack
- **Propagation Contradiction Recovery**: Detect and recover from contradictions that occur during constraint propagation, not just during cell observation
- **Complete Solution Space Exploration**: Guarantee exhaustive search through all candidate values when backtracking occurs
- **Deterministic Behavior**: Maintain reproducible solving with proper stack unwinding and state restoration

**Target Scenarios:**

- Complex constraint networks where propagation creates singleton domains that later contradict
- Multi-level backtracking across several decision points
- Seam consistency scenarios where boundary constraints force singleton domains
- Deterministic generation requiring identical decision sequences

## Table of contents

- [WFC Backtracking Robustness Plan](#wfc-backtracking-robustness-plan)
  - [Overview](#overview)
  - [Table of contents](#table-of-contents)
  - [Plan issue](#plan-issue)
  - [Plan status](#plan-status)
  - [Definition of terms](#definition-of-terms)
  - [Architectural considerations and constraints](#architectural-considerations-and-constraints)
  - [Implementation guide](#implementation-guide)
    - [Plan requirements](#plan-requirements)
    - [Phase 1: Decision Stack Refactoring](#phase-1-decision-stack-refactoring)
    - [Phase 2: Propagation Contradiction Handling](#phase-2-propagation-contradiction-handling)
    - [Phase 3: Test Coverage](#phase-3-test-coverage)
  - [See also](#see-also)
    - [Related Plans](#related-plans)
    - [Implementation Files](#implementation-files)
  - [References](#references)

## Plan issue

This plan currently has no associated GitHub issue. Consider creating an issue to track backtracking robustness improvements and link it here.

## Plan status

- **Status**: In discovery
- **Rationale**: Investigating whether current backtracking implementation has edge cases with singleton domains and propagation-induced contradictions

**Investigation Needed:**

1. Review current ChangeLog and decision stack implementation in WfcProvider
2. Identify specific scenarios where singleton domain contradictions occur
3. Determine if current backtracking properly handles all contradiction sources
4. Assess impact on seam consistency and multi-chunk generation

**Known Potential Issues:**

- Singleton domains created by propagation may not be pushed onto decision stack
- Contradiction detection may only occur during observation, not propagation
- Stack unwinding logic may not fully restore all propagation side effects

## Definition of terms

| Term            | Meaning                                                                                                              | Reference                                                                   |
| --------------- | -------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| Contradiction   | A state where a cell's domain becomes empty during propagation, indicating no valid tile values satisfy constraints |                                                                             |
| Decision Stack  | A stack data structure recording cell assignments and domain states to enable rollback during backtracking          |                                                                             |
| Domain          | The set of possible tile type IDs that can be validly assigned to a cell position                                   |                                                                             |
| Propagation     | The process of updating neighboring cell domains based on constraint rules after a cell observation                 |                                                                             |
| Singleton       | A domain containing exactly one possible value, representing a forced assignment                                     |                                                                             |
| Stack Frame     | A record on the decision stack containing cell coordinates, chosen value, and previous domain/output state          |                                                                             |
| Stack Unwinding | The process of popping decision stack frames and restoring previous states when a contradiction is detected         |                                                                             |
| WFC             | Wave Function Collapse, a constraint-based procedural generation algorithm                                          | [WFC Wiki](https://en.wikipedia.org/wiki/Wave_function_collapse_algorithm) |

## Architectural considerations and constraints

**Current Backtracking Architecture:**

The existing WFC implementation uses a ChangeLog-based backtracking system:

```text
WfcProvider
  ├── _possibilities: HashSet<int>?[][] (nullable domains)
  ├── _output: int[][] (assigned tile IDs)
  ├── _changeLog: Stack<List<Change>> (domain change records)
  └── _propagator: AC3Propagator (constraint propagation)
```

**Decision Flow:**

```text
1. Select uncollapsed cell (lowest entropy)
2. Choose candidate tile from domain
3. Record changes to ChangeLog
4. Collapse cell: set _output[x][y] and _possibilities[x][y] = null
5. Propagate constraints via AC3Propagator
6. If contradiction detected → Backtrack() and restore from ChangeLog
7. Repeat until all cells collapsed or unsatisfiable
```

**Identified Robustness Concerns:**

1. **Singleton Domain Tracking**: When propagation reduces a domain to one value, that cell may be auto-collapsed without being added to the decision stack, preventing backtracking through that choice

2. **Propagation Contradiction Detection**: Current implementation may only detect contradictions during explicit cell observation, missing cases where propagation creates empty domains in neighboring cells

3. **Stack Frame Completeness**: Decision stack may not capture all state necessary for full rollback, particularly:
   - Propagator internal state (arc queue, processed arcs)
   - Multiple domain reductions from a single propagation step
   - Order of candidate exploration for determinism

4. **Performance vs Robustness Tradeoff**: Pushing every singleton domain change onto the stack may increase memory usage and stack depth, requiring careful performance analysis

**Constraints:**

- **Performance**: Backtracking must not add significant overhead to the hot path (target: <5% performance impact)
- **Memory**: Decision stack depth bounded by grid size × domain size (64×64 grid → maximum ~4096 decisions)
- **Determinism**: Identical inputs must produce identical decision sequences and outputs
- **Compatibility**: Changes must not break existing chunk boundary constraints or seam consistency logic

## Implementation guide

### Plan requirements

- (Incomplete) Decision stack tracks all assignments including singletons
  - GIVEN a WFC solver with backtracking enabled
  - WHEN propagation reduces a domain to a singleton and that cell is auto-collapsed
  - THEN the decision stack contains an entry for that assignment enabling rollback

> Implementation not started. Requires modification to WfcProvider collapse logic and ChangeLog structure.

- (Incomplete) Propagation contradiction detection triggers backtracking
  - GIVEN a WFC solver with backtracking enabled
  - WHEN constraint propagation creates an empty domain in any cell
  - THEN the solver detects the contradiction and initiates stack unwinding

> Implementation not started. Requires AC3Propagator to return contradiction status and WfcProvider to handle it.

- (Incomplete) Stack unwinding explores all candidate values
  - GIVEN a decision stack with multiple candidate options at various levels
  - WHEN a contradiction occurs
  - THEN the solver unwinds to the most recent decision point with unexplored candidates and tries the next option

> Implementation not started. Requires decision stack to track candidate iteration state.

- (Incomplete) Deterministic backtracking with reproducible results
  - GIVEN identical grid dimensions, tile types, rules, and random seed
  - WHEN WFC generation runs with backtracking
  - THEN the output is identical across multiple runs including the same decision sequence

> Implementation not started. Requires deterministic candidate ordering and stack unwinding logic.

### Phase 1: Decision Stack Refactoring

#### Objective

Modify the ChangeLog-based system to explicitly track decision points, candidate iterations, and singleton assignments, enabling complete rollback of all solver state.

#### Technical details

**Current ChangeLog Structure:**

```csharp
// Each "change" records domain modifications
Stack<List<Change>> _changeLog;

struct Change
{
    int X, Y;
    HashSet<int>? PreviousDomain;
}
```

**Proposed Decision Stack Structure:**

```csharp
Stack<DecisionFrame> _decisionStack;

struct DecisionFrame
{
    // Decision metadata
    int X, Y;                      // Cell being assigned
    int ChosenTile;                // Tile ID assigned
    List<int> RemainingCandidates; // Unexplored options from original domain
    
    // Rollback state
    HashSet<int>?[][] PreviousDomains;  // Full domain snapshot (or delta)
    int[][] PreviousOutput;             // Full output snapshot (or delta)
    
    // Diagnostics
    bool IsSingleton;              // True if this was a forced assignment
    int StackDepth;                // Depth when created
}
```

**Alternative Lightweight Approach (Delta Storage):**

To reduce memory overhead, store only changed cells rather than full grid snapshots:

```csharp
struct DecisionFrame
{
    int X, Y;
    int ChosenTile;
    List<int> RemainingCandidates;
    
    // Delta-based rollback
    List<DomainChange> DomainChanges;  // Only cells that changed
}

struct DomainChange
{
    int X, Y;
    HashSet<int>? PreviousDomain;
}
```

**Implementation Steps:**

1. Define DecisionFrame structure with full vs delta storage options
2. Add `_decisionStack` field to WfcProvider
3. Modify cell collapse logic to push frame before assignment
4. Update Backtrack() to pop frame and restore state
5. Add candidate iteration tracking to explore all options
6. Benchmark memory usage (full snapshot vs delta approach)

#### Examples

**Before (Current ChangeLog):**

```csharp
// WfcProvider.cs - Current approach
bool CollapseCell(int x, int y)
{
    var domain = _possibilities[x][y];
    if (domain == null || domain.Count == 0) return false;
    
    _changeLog.Push(new List<Change>());  // Start new change group
    
    int chosen = SelectWeightedTile(domain);
    _output[x][y] = chosen;
    _possibilities[x][y] = null;  // Mark as collapsed
    
    return _propagator.PropagateFrom(x, y, chosen, _changeLog.Peek());
}
```

**After (Decision Stack):**

```csharp
bool CollapseCell(int x, int y)
{
    var domain = _possibilities[x][y];
    if (domain == null || domain.Count == 0) return false;
    
    // Create decision frame BEFORE collapsing
    var frame = new DecisionFrame
    {
        X = x,
        Y = y,
        ChosenTile = -1,  // Set after selection
        RemainingCandidates = new List<int>(domain),
        DomainChanges = new List<DomainChange>(),
        IsSingleton = domain.Count == 1
    };
    
    int chosen = SelectWeightedTile(frame.RemainingCandidates);
    frame.ChosenTile = chosen;
    frame.RemainingCandidates.Remove(chosen);  // Mark as explored
    
    _decisionStack.Push(frame);
    
    _output[x][y] = chosen;
    _possibilities[x][y] = null;
    
    return _propagator.PropagateFrom(x, y, chosen, frame.DomainChanges);
}
```

### Phase 2: Propagation Contradiction Handling

#### Objective

Ensure all contradiction sources (cell observation failures AND propagation-induced empty domains) trigger proper backtracking with stack unwinding.

#### Technical details

**Current Contradiction Handling:**

```csharp
// Only detects contradictions during cell selection
var cell = SelectLowestEntropyCell();
if (cell == null) return false;  // No uncollapsed cells

if (!CollapseCell(cell.X, cell.Y))
{
    // Contradiction during collapse
    if (CanBacktrack()) Backtrack();
    else return false;
}
```

**Enhanced Contradiction Handling:**

```csharp
bool PropagateFrom(int x, int y, int tileId, List<DomainChange> changes)
{
    // ... AC-3 propagation logic ...
    
    // Check for empty domains after propagation
    foreach (var (nx, ny) in affectedCells)
    {
        if (_possibilities[nx][ny]?.Count == 0)
        {
            return false;  // Contradiction detected
        }
    }
    
    return true;
}

// Main generate loop
while (HasUncollapedCells())
{
    var cell = SelectLowestEntropyCell();
    if (cell == null) break;
    
    if (!CollapseCell(cell.X, cell.Y))
    {
        // Contradiction - try backtracking
        while (CanBacktrack())
        {
            Backtrack();  // Restore previous state
            
            var frame = _decisionStack.Peek();
            if (frame.RemainingCandidates.Count > 0)
            {
                // Try next candidate from this decision point
                RetryWithNextCandidate(frame);
                break;
            }
            else
            {
                // No more candidates at this level, pop and continue
                _decisionStack.Pop();
            }
        }
        
        if (!CanBacktrack()) return false;  // Unsatisfiable
    }
}
```

**Implementation Steps:**

1. Modify AC3Propagator.PropagateFrom() to return contradiction status
2. Add empty domain detection after each propagation step
3. Update WfcProvider main loop to handle propagation contradictions
4. Implement RetryWithNextCandidate() method to explore remaining options
5. Add proper termination when all candidates exhausted (unsatisfiable problem)

#### Examples

**Enhanced Backtrack with Candidate Iteration:**

```csharp
void Backtrack()
{
    if (_decisionStack.Count == 0) return;
    
    var frame = _decisionStack.Peek();
    
    // Restore previous state
    foreach (var change in frame.DomainChanges)
    {
        _possibilities[change.X][change.Y] = change.PreviousDomain;
    }
    _output[frame.X][frame.Y] = -1;  // Uncollapse
    _possibilities[frame.X][frame.Y] = new HashSet<int>(frame.RemainingCandidates);
}

void RetryWithNextCandidate(DecisionFrame frame)
{
    // Don't pop the frame - reuse it with next candidate
    int nextTile = SelectWeightedTile(frame.RemainingCandidates);
    frame.ChosenTile = nextTile;
    frame.RemainingCandidates.Remove(nextTile);
    frame.DomainChanges.Clear();
    
    _output[frame.X][frame.Y] = nextTile;
    _possibilities[frame.X][frame.Y] = null;
    
    _propagator.PropagateFrom(frame.X, frame.Y, nextTile, frame.DomainChanges);
}
```

### Phase 3: Test Coverage

#### Objective

Create comprehensive unit, integration, and property-based tests to verify backtracking robustness across all contradiction scenarios.

#### Technical details

**Test Categories:**

1. **Unit Tests - Singleton Domain Handling**
   - Test that singleton domains are added to decision stack
   - Verify rollback properly restores singleton assignments
   - Ensure deterministic ordering of singleton explorations

2. **Unit Tests - Propagation Contradictions**
   - Create scenarios where propagation creates empty domains
   - Verify contradiction detection and backtracking initiation
   - Test multi-level unwinding (backtrack across multiple decisions)

3. **Integration Tests - Chunk Seam Consistency**
   - Generate adjacent chunks with boundary constraints
   - Verify seamless tile matching across chunk borders
   - Test that backtracking maintains seam consistency

4. **Property-Based Tests - Determinism**
   - Generate multiple runs with identical seeds
   - Assert identical decision sequences and outputs
   - Verify that backtracking paths are deterministic

5. **Performance Regression Tests**
   - Benchmark generation time with backtracking enabled
   - Measure memory usage of decision stack
   - Compare full snapshot vs delta storage approaches

**Implementation Steps:**

1. Create test fixtures with known contradiction scenarios
2. Implement unit tests for DecisionFrame stack operations
3. Add integration tests for chunk boundary backtracking
4. Develop property-based tests using FsCheck or similar
5. Set up performance benchmarks in BenchmarkDotNet
6. Document expected behavior and edge cases

#### Examples

**Unit Test - Singleton Domain Backtracking:**

```csharp
[Fact]
public void BacktrackingHandlesSingletonDomains()
{
    // Arrange: Create a scenario where propagation creates singleton
    var tileset = CreateSimpleTileset();  // Grass, Water, Sand
    var rules = new PrecomputedRuleTable(tileset);
    var provider = new WfcProvider(
        width: 3,
        height: 3,
        rules,
        seed: 12345,
        enableBacktracking: true
    );
    
    // Force a configuration that creates singleton then contradicts
    // [G] [?] [?]
    // [W] [S] [?]  <- S (sand) is singleton between G and W
    // [G] [?] [?]
    
    provider.SetOutput(0, 0, TileType.Grass);
    provider.SetOutput(0, 1, TileType.Water);
    provider.SetOutput(1, 1, TileType.Sand);  // Singleton forced by constraints
    provider.SetOutput(0, 2, TileType.Grass);
    
    // Act: Try to generate rest of grid (should contradict and backtrack)
    var result = provider.Generate();
    
    // Assert: Should successfully backtrack through singleton decision
    Assert.True(result, "Should find solution by backtracking through singleton");
    Assert.True(provider.IsFullyCollapsed(), "All cells should be assigned");
    
    // Verify decision stack included singleton assignment
    var diagnostics = provider.GetDiagnostics();
    Assert.Contains(diagnostics.DecisionHistory, d => d.IsSingleton);
}
```

**Integration Test - Multi-Chunk Seam Consistency:**

```csharp
[Fact]
public void BacktrackingMaintainsChunkSeamConsistency()
{
    // Arrange: Generate two adjacent chunks with backtracking
    var tileset = CreateTerrainTileset();
    var map = new ChunkedTilemap(tileset, worldWidth: 128, seed: 12345);
    
    // Act: Generate two adjacent chunks
    map.UpdateActiveChunks(new Rectangle(0, 0, 128, 64));
    
    // Assert: Verify seam tiles match perfectly
    for (int y = 0; y < 64; y++)
    {
        var chunk0Tile = map.GetTile(63, y);   // Right edge of chunk 0
        var chunk1Tile = map.GetTile(64, y);   // Left edge of chunk 1
        
        Assert.True(
            AreCompatibleNeighbors(chunk0Tile, chunk1Tile, Direction.East),
            $"Seam mismatch at y={y}: {chunk0Tile} <-> {chunk1Tile}"
        );
    }
}
```

## See also

### Related Plans

- [WFC Completion Plan][plan-wfc-completion] - Parent plan for all WFC enhancements
- [Performance Analysis][plan-performance] - Backtracking performance impact and optimization
- [Property-Based Tests][plan-property-tests] - Testing strategy including backtracking scenarios

### Implementation Files

- [WfcProvider.cs][impl-wfc-provider] - Main WFC solver with backtracking logic
- [AC3Propagator.cs][impl-ac3-propagator] - Constraint propagation and contradiction detection
- [ChangeLog.cs][impl-changelog] - Current change tracking system (to be enhanced)
- [PrecomputedRuleTable.cs][impl-rule-table] - Adjacency rules used in propagation

## References

### Constraint Satisfaction

- [Backtracking Algorithm][ref-backtracking] - General backtracking search strategies
- [Constraint Satisfaction Problem][ref-csp] - CSP theory and solving techniques
- [AC-3 Algorithm][ref-ac3] - Arc consistency and constraint propagation

### Implementation References

- [C# Stack\<T\>][ref-csharp-stack] - .NET stack data structure for decision tracking
- [HashSet\<T\>][ref-csharp-hashset] - Domain representation and set operations

<!-- Related Plans -->
[plan-wfc-completion]: ../wfc-completion-plan.md
[plan-performance]: performance-analysis.md
[plan-property-tests]: property-and-performance-tests.md

<!-- Implementation Files -->
[impl-wfc-provider]: ../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/WfcProvider.cs
[impl-ac3-propagator]: ../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/AC3Propagator.cs
[impl-changelog]: ../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/ChangeLog.cs
[impl-rule-table]: ../../TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse/PrecomputedRuleTable.cs

<!-- External References -->
[ref-backtracking]: https://en.wikipedia.org/wiki/Backtracking
[ref-csp]: https://en.wikipedia.org/wiki/Constraint_satisfaction_problem
[ref-ac3]: https://en.wikipedia.org/wiki/AC-3_algorithm
[ref-csharp-stack]: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.stack-1
[ref-csharp-hashset]: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1
