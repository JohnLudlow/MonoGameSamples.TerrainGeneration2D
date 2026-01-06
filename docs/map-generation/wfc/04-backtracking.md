# Backtracking

Purpose: introduce decision points and reversible state so contradictions can be resolved by trying alternate candidates.

## Data Structures

Backtracking introduces explicit, reversible state. We model two concepts:
1) a decision point (which cell we’re choosing and the order we’ll try its candidates), and 2) a change log (every mutation we make so we can roll it back). These appear in code as `DecisionFrame` and `ChangeLog`.

### DecisionFrame — what it represents and how it’s used

A `DecisionFrame` captures a single branching point in the search. When the solver selects the lowest-entropy cell `(x,y)`, it computes an ordered list of candidate tiles for that cell. The frame stores:
- where the decision applies (`X`, `Y`),
- which candidates are available in the exact order we plan to try,
- which candidate index we’ll attempt next (`NextIndex`), and
- a bookmark into the change log (`ChangesMark`) so we can undo all mutations caused by trying a candidate and its propagation.

The solver pushes a `DecisionFrame` onto a stack before attempting any candidate. If trying a candidate leads to a contradiction, we call `RollbackTo(frame.ChangesMark)` and advance `frame.NextIndex` to try the next candidate. If we exhaust all candidates, we pop the frame and backtrack to the previous decision.

### Change and ChangeLog — capturing reversible mutations

Every time we remove a value from a domain, set a cell’s output, or auto-collapse a domain of size 1, we record an entry in the `ChangeLog`. The log is append-only and supports checkpoint/rollback:
- `Mark()` returns the current length (a checkpoint),
- `RollbackTo(mark, domains, output)` walks entries in reverse and restores the domains/output to their exact pre-decision state.

About naming: earlier drafts used generic auxiliary fields `A` and `B`. More descriptive names reduce ambiguity. Below, each change kind records specific values with clear names (e.g., `RemovedTileId`, `PrevOutput`, `NextOutput`, `ChosenTileId`) and optional snapshots for undo.

## WfcProvider Additions

Add the following private fields:
- `Stack<DecisionFrame> _decisions`
- `ChangeLog _changes`
- `bool _enableBacktracking`
- Optional limits: `int _backtrackLimit`, `int _steps`

Overload method signatures to thread the recorder (no behavioral change unless backtracking is enabled):
- `public bool Generate(int maxIterations = 10000)` (existing)
- `public bool Generate(bool enableBacktracking, int maxIterations = 10000, int? maxBacktrackSteps = null, int? maxDepth = null)` (new)
- `private bool CollapseCell(int x, int y)` (existing)
- `private bool CollapseCell(int x, int y, int chosenTile, ChangeLog log)` (new)
- `private bool Propagate(int startX, int startY)` (existing)
- `private bool Propagate(int startX, int startY, ChangeLog log)` (new)
- `private bool ConstrainNeighbor(int x, int y, Direction dirToNeighbor, int neighborTileId, TilePoint neighborPosition)` (existing)
- `private bool ConstrainNeighbor(int x, int y, Direction dirToNeighbor, int neighborTileId, TilePoint neighborPosition, ChangeLog log)` (new)

Example: XML-documented shapes (abbreviated)

```csharp
// Under TerrainGeneration2D.Core/Mapping/WaveFunctionCollapse (separate types)
/// <summary>
/// Represents a single branching point for the solver.
/// Stores the target cell, an ordered candidate list, and the
/// change-log bookmark required to roll back mutations made while
/// attempting a candidate.
/// </summary>
private sealed class DecisionFrame
{
	/// <summary>The x-coordinate of the chosen cell.</summary>
	public int X { get; init; }
	/// <summary>The y-coordinate of the chosen cell.</summary>
	public int Y { get; init; }
	/// <summary>Ordered list of tile IDs to try at (X,Y).</summary>
	public int[] Candidates { get; init; } = Array.Empty<int>();
	/// <summary>Index of the next candidate to attempt.</summary>
	public int NextIndex { get; set; }
	/// <summary>Checkpoint into the change log taken before trying any candidate.</summary>
	public int ChangesMark { get; init; }
	/// <summary>Depth of this frame in the decision stack (for diagnostics).</summary>
	public int Depth { get; init; }
}

/// <summary>
/// Kinds of reversible mutations captured by the change log.
/// </summary>
private enum ChangeKind { DomainRemoved, CellCollapsed, OutputSet }

/// <summary>
/// A single reversible mutation. Depending on <see cref="ChangeKind"/>,
/// fields encode the minimum data needed to undo the change.
/// </summary>
private readonly struct Change
{
	/// <summary>The mutation category.</summary>
	public ChangeKind Kind { get; }
	/// <summary>Target cell x-coordinate.</summary>
	public int X { get; }
	/// <summary>Target cell y-coordinate.</summary>
	public int Y { get; }
	/// <summary>When <see cref="ChangeKind.DomainRemoved"/>, the tile ID removed from the domain.</summary>
	public int RemovedTileId { get; }
	/// <summary>When <see cref="ChangeKind.CellCollapsed"/>, the tile ID chosen for the collapse.</summary>
	public int ChosenTileId { get; }
	/// <summary>When <see cref="ChangeKind.OutputSet"/>, the previous output tile ID.</summary>
	public int PrevOutput { get; }
	/// <summary>When <see cref="ChangeKind.OutputSet"/>, the new output tile ID.</summary>
	public int NextOutput { get; }
	/// <summary>Optional snapshot (e.g., full previous domain for a collapse).</summary>
	public IReadOnlyCollection<int>? PrevDomainSnapshot { get; }
	// Factory helpers omitted; implementation stores enough data to reverse the operation.
}

/// <summary>
/// Append-only list of reversible mutations with checkpoint/rollback support.
/// Call <see cref="Mark"/> to capture a checkpoint, record mutations as they
/// occur, and <see cref="RollbackTo"/> to restore <c>domains</c> and <c>output</c>
/// to their state at the given checkpoint.
/// </summary>
private sealed class ChangeLog
{
	private readonly List<Change> _changes = new();
	/// <summary>Create a checkpoint representing the current tail of the log.</summary>
	public int Mark() => _changes.Count;
	/// <summary>Record removal of a single tile from a domain at (x,y).</summary>
	public void RecordDomainRemoved(int x, int y, int tileId) { /* append */ }
	/// <summary>Record collapsing a domain to a single tile, with a snapshot for undo.</summary>
	public void RecordCellCollapsed(int x, int y, IEnumerable<int> prevDomain, int chosen) { /* append */ }
	/// <summary>Record setting the output at (x,y) to a new tile, keeping the previous value.</summary>
	public void RecordOutputSet(int x, int y, int previous, int next) { /* append */ }
	/// <summary>Undo all mutations recorded after <paramref name="mark"/>.</summary>
	public void RollbackTo(int mark, HashSet<int>?[,] domains, int[,] output) { /* undo in reverse */ }
}
```

## Generate() Flow With Backtracking

ASCII sequence of the main loop inside `Generate(enableBacktracking: true, ...)`:

```
Solver                       DecisionStack                ChangeLog                   Domains/Output
|                            |                            |                           |                         |
| -> DS: FindLowestEntropy() |                            |                           |                         |
| <- DS: (x,y) or none       |                            |                           |                         |
| if none: SUCCESS           |                            |                           |                         |
| -> CL: Mark()              |                            | Mark() -> [mark]          |                         |
| -> DS: Push frame          | push (X,Y,NextIndex=0,     |                           |                         |
|                            |       ChangesMark=mark)    |                           |                         |
| Try next candidate         |                            |                           |                         |
| choose next from list      |                            |                           |                         |
| -> CL: CollapseCell(...)   |                            | record CellCollapsed      | apply collapse          |
| -> CL: Propagate(...)      |                            | record DomainRemoved      | apply removals          |
| <- CL: ok?                 |                            |                           |                         |
| yes: next cell             |                            |                           |                         |
| no: CONTRADICTION          |                            |                           |                         |
| -> CL: RollbackTo(mark)    |                            | undo in reverse           | state restored          |
| more candidates?           |                            |                           |                         |
| yes: try next              |                            |                           |                         |
| no -> DS: Pop frame        | pop                        |                           |                         |
| stack empty?               |                            |                           |                         |
| yes: FAIL                  |                            |                           |                         |
| no: resume prev frame      |                            |                           |                         |
```

Success criteria: all domains are `null` (collapsed) and `Generate()` returns `true`.

## Diagnostics

Augment WFC events in `TerrainPerformanceEventSource` and invoke them in the backtracking loop:
- `WfcDecisionPush` when pushing a `DecisionFrame`
- `WfcApplyChoice` before attempting a candidate
- `WfcContradiction` when propagation empties a domain
- `WfcRollbackBegin`/`WfcRollbackEnd` around `RollbackTo(...)`
- `WfcDecisionPop` when a frame exhausts candidates
- `WfcStats` on completion with decisions, backtracks, and max depth

Example implementation (TerrainGeneration2D.Core/Diagnostics/TerrainPerformanceEventSource.cs):

```csharp
// inside TerrainPerformanceEventSource
private const int WfcDecisionPushId = 18;
private const int WfcApplyChoiceId  = 19;
private const int WfcContradictionId = 20;
private const int WfcRollbackBeginId = 21;
private const int WfcRollbackEndId   = 22;
private const int WfcDecisionPopId   = 23;
private const int WfcStatsId         = 24;

[NonEvent]
public void WfcDecisionPush(int depth, int x, int y, int candidateCount)
	=> WfcDecisionPush(depth, x, y, candidateCount.ToString());

[Event(WfcDecisionPushId, Level = EventLevel.Informational)]
private void WfcDecisionPush(int depth, int x, int y, string candidates) { WriteEvent(WfcDecisionPushId, depth, x, y, candidates); }

[NonEvent]
public void WfcApplyChoice(int depth, int x, int y, int tileId) => WriteEvent(WfcApplyChoiceId, depth, x, y, tileId);

[Event(WfcContradictionId, Level = EventLevel.Warning)]
public void WfcContradiction(int depth, int x, int y) { WriteEvent(WfcContradictionId, depth, x, y); }

[Event(WfcRollbackBeginId, Level = EventLevel.Informational)]
public void WfcRollbackBegin(int depth, int mark) { WriteEvent(WfcRollbackBeginId, depth, mark); }

[Event(WfcRollbackEndId, Level = EventLevel.Informational)]
public void WfcRollbackEnd(int depth) { WriteEvent(WfcRollbackEndId, depth); }

[Event(WfcDecisionPopId, Level = EventLevel.Informational)]
public void WfcDecisionPop(int depth) { WriteEvent(WfcDecisionPopId, depth); }

[Event(WfcStatsId, Level = EventLevel.Informational)]
public void WfcStats(int decisions, int backtracks, int maxDepth) { WriteEvent(WfcStatsId, decisions, backtracks, maxDepth); }
```

## Integration

From `ChunkedTilemap`, prefer calling `Generate(enableBacktracking: true, ...)` and pass optional limits; maintain fallback to random fill if `Generate(...)` returns `false`.

### Game Integration Example

Where it lives: TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs, class `ChunkedTilemap`, method `GenerateChunk(Point chunkCoords)`.

```csharp
// TerrainGeneration2D.Core/Graphics/ChunkedTilemap.cs
// class ChunkedTilemap
private Chunk GenerateChunk(Point chunkCoords)
{
	var chunk = new Chunk(chunkCoords);
	int chunkSeed = _masterSeed + chunkCoords.X * 73856093 + chunkCoords.Y * 19349663;
	var random = new Random(chunkSeed);

	if (_useWaveFunctionCollapse)
	{
		var chunkOrigin = new Point(chunkCoords.X * Chunk.ChunkSize, chunkCoords.Y * Chunk.ChunkSize);
		var wfc = new WfcProvider(Chunk.ChunkSize, Chunk.ChunkSize, _tileTypeRegistry, random, _terrainRuleConfig, _heightProvider, chunkOrigin);

		// Backtracking enabled with sane limits
		bool ok = wfc.Generate(enableBacktracking: true, maxIterations: 10_000, maxBacktrackSteps: 5_000, maxDepth: 256);
		if (ok)
		{
			var output = wfc.GetOutput();
			for (int y = 0; y < Chunk.ChunkSize; y++)
				for (int x = 0; x < Chunk.ChunkSize; x++)
					chunk[x, y] = output[x, y];
		}
		else
		{
			GenerateRandomChunk(chunk, random); // Fallback remains deterministic
		}
	}
	else
	{
		GenerateRandomChunk(chunk, random);
	}

	chunk.IsDirty = true;
	return chunk;
}
```

Navigation
- Up: [WFC README](README.md)
- Previous: [03 — Propagation](03-propagation.md)
- Next: [05 — Integration](05-integration.md)
