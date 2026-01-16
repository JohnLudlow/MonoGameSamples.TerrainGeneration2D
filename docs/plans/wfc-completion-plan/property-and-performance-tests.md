# Comprehensive Property-Based and Performance Regression Tests for WFC

## Overview

Achieve comprehensive property-based and performance regression testing for the WFC system, targeting ≥95% code coverage and robust performance guarantees. Intended for teams maintaining or extending WFC.

## Table of contents

- [Overview](#overview)
- [Feature requirements](#feature-requirements)
- [Feature status](#feature-status)
- [Definition of terms](#definition-of-terms)
- [Architectural considerations and constraints](#architectural-considerations-and-constraints)
- [Implementation guide](#implementation-guide)

## Feature requirements

- Property-based tests for constraint satisfaction, determinism, and completeness
- Performance regression tests for chunk generation time and memory usage
- Automated test integration with CI pipeline
- Statistical analysis of test results (e.g., 95th percentile timing)

## Feature status

- Not started

## Definition of terms

| Term                   | Meaning                                                     | Reference |
| ---------------------- | ----------------------------------------------------------- | --------- |
| Property-based testing | Testing by generating random inputs and checking invariants |           |
| Performance regression | Detecting slowdowns or increased resource usage over time   |           |

## Architectural considerations and constraints

- Tests must be reproducible and deterministic
- Performance tests should run on stable hardware or CI agents
- Document how to interpret and act on test failures

## Implementation guide

### Phase 1: Property-Based Test Suites

#### Objective

Implement property-based tests for WFC core properties, ensuring that all generated outputs satisfy key invariants under a wide range of random inputs.

#### Technical details

- Use property-based testing frameworks (e.g., FsCheck, xUnit.Theories) to generate random rule sets, configurations, and seeds.
- Check invariants such as:
  - Constraint satisfaction: All outputs must satisfy adjacency and domain rules.
  - Determinism: Identical inputs produce identical outputs.
  - Completeness: The solver either finds a valid solution or reports unsatisfiability.
- Integrate mutation testing using Stryker.NET to validate the effectiveness of property-based tests by introducing code mutations and ensuring tests catch them.
- Document how to interpret property test failures and how to extend the test suite for new invariants.

#### Examples

The following test checks that all generated solutions satisfy adjacency constraints for randomly generated rule sets and configurations.

```csharp
[Property]
public void WfcSolver_AlwaysSatisfiesConstraints_ForValidInputs(RandomRuleSet rules, RandomConfig config) {
  var solver = new WfcProvider(rules, config);
  var result = solver.Generate();
  Assert.True(result.IsValid);
  Assert.True(result.SatisfiesAllConstraints());
}
```

To ensure property-based tests are effective, use Stryker.NET for mutation testing. This tool will mutate your code and verify that your property-based tests fail as expected, indicating strong test coverage.

```shell
dotnet tool install -g dotnet-stryker
dotnet stryker
```

If Stryker reports surviving mutants, add or strengthen property-based tests to catch them.

### Phase 2: Performance Regression Benchmarks

#### Objective

Add performance regression benchmarks for chunk generation and other hot-path WFC operations, ensuring no regressions over time.

#### Technical details

- Use benchmarking tools such as BenchmarkDotNet to measure chunk generation time, memory usage, and allocation rates.
- Track key metrics:
  - Mean and 95th percentile chunk generation time
  - Memory allocations per chunk
  - Throughput (chunks/sec)
- Compare results to established baselines and fail the build if regressions are detected.
- Automate benchmarks to run on stable CI hardware or dedicated agents for consistency.
- Document how to interpret benchmark results and update baselines when intentional improvements are made.

#### Examples

The following benchmark measures the time to generate a 64x64 chunk using the current WFC configuration.

```csharp
[Benchmark]
public void ChunkGeneration_Benchmark() {
  var chunk = new ChunkedTilemap(/* ... */);
  chunk.GenerateChunk(new Point(0, 0));
}
```

After running benchmarks, analyze the 95th percentile timing and compare to the performance budget (e.g., <100ms per chunk).

### Phase 3: CI Integration and Reporting

#### Objective

Integrate property-based and performance tests into the CI pipeline, providing actionable diagnostics and enforcing quality gates.

#### Technical details

- Automate execution of all property-based and performance tests in the CI pipeline (e.g., GitHub Actions, Azure DevOps).
- Use Stryker.NET in CI to ensure mutation coverage remains high.
- Report failures and regressions with clear, actionable diagnostics (e.g., which invariant failed, which metric regressed).
- Fail the build if property-based tests fail, if performance falls below baseline, or if mutation coverage drops.
- Provide links to documentation on interpreting failures and next steps for remediation.

#### Examples

Example GitHub Actions workflow step for running Stryker.NET:

```yaml
- name: Run mutation tests
 run: |
  dotnet tool restore
  dotnet stryker
```

Example workflow step for running performance benchmarks:

```yaml
- name: Run performance benchmarks
 run: |
  dotnet run --project TerrainGeneration2D.Benchmarks
```
