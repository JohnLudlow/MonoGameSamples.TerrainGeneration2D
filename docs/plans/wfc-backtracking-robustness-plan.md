# WFC Backtracking Robustness Plan

## Overview
This plan describes a robust approach to Wave Function Collapse (WFC) backtracking that correctly handles singleton domains, contradictions, and decision stack unwinding. The goal is to ensure that the solver can recover from propagation-induced contradictions, even when domains are restricted to a single candidate, and always explores all possible solutions.

## Definition of Terms
| Term         | Meaning                                                                 | Reference |
|--------------|-------------------------------------------------------------------------|-----------|
| WFC          | Wave Function Collapse, a constraint-based procedural generation method | [WFC Wiki](https://en.wikipedia.org/wiki/Wave_function_collapse_algorithm) |
| Domain       | The set of possible tile values for a cell                             |           |
| Singleton    | A domain containing only one possible value                            |           |
| Contradiction| A state where a cell's domain is empty (no valid candidates)           |           |
| Decision Stack| Stack of choices made during solving, used for backtracking           |           |
| Propagation  | The process of updating domains based on constraints                   |           |

## Requirements
- The solver must:
  - Always push every cell assignment (even singletons) onto the decision stack.
  - Detect contradictions during propagation and unwind the stack to retry alternate candidates.
  - Support recovery from propagation-induced contradictions, not just assignment failures.
  - Assign values to all cells, or report unsatisfiability if no solution exists.
  - Maintain deterministic behavior for reproducibility.

## Implementation Steps
1. **Decision Stack Refactor**
   - Push every cell assignment (including singletons) onto the stack.
   - Record the domain and output state before each assignment.
2. **Propagation Contradiction Handling**
   - If propagation empties a domain, treat as a contradiction and unwind the stack.
   - Roll back to the previous decision point and try the next candidate.
3. **Assignment and Propagation Loop**
   - Select the cell with the lowest entropy (smallest domain).
   - For singleton domains, treat as a decision with only one candidate.
   - For multi-candidate domains, try each candidate in order.
   - After assignment, propagate constraints and check for contradictions.
4. **Stack Unwinding**
   - On contradiction, restore domains and outputs to the previous stack frame.
   - Continue until a solution is found or all candidates are exhausted.
5. **Test Coverage**
   - Add tests for:
     - Contradictions caused by propagation after singleton assignment.
     - Recovery from multi-level contradictions.
     - Deterministic solution finding.

## Implementation Considerations
- **Readability:** Use clear stack frame structures and explicit rollback logic.
- **Reliability:** Ensure all state changes are reversible; avoid side effects outside stack frames.
- **Testability:** Isolate stack/rollback logic for unit testing; provide hooks for diagnostics.
- **Performance:** Minimize allocations in hot paths; avoid deep recursion.
- **Extensibility:** Design stack frames to support future constraint types and diagnostics.

## Testing
- Property-based tests for random domain restrictions and contradiction recovery.
- Integration tests for multi-chunk seam consistency with backtracking.
- Determinism tests for reproducible solutions.

## Next Steps
- Review current WFC implementation and refactor decision stack logic as described.
- Implement new tests for contradiction and recovery scenarios.
- Validate with existing and new property/integration tests.

---
*This plan supersedes the current backtracking logic and will enable robust, diagnosable WFC solving for all domain scenarios.*
