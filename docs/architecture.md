# Architecture

Pivot selection is the independent variable under experiment. All three strategies use one Gaussian-elimination implementation, the same row-major matrix representation, and the same diagnostics.

## Projects and dependencies

```text
PivotGrowth.Cli ---------> PivotGrowth.Core
PivotGrowth.Benchmarks --> PivotGrowth.Core
PivotGrowth.Tests -------> PivotGrowth.Core + PivotGrowth.Cli
```

Core and CLI have no runtime package dependencies. Tests use xUnit; the benchmark project alone uses BenchmarkDotNet. Package versions are managed in `Directory.Packages.props`; compiler settings are shared through `Directory.Build.props`.

## Solver flow

`GaussianEliminationSolver` receives an `IPivotStrategy`: `NoPivotStrategy`, `PartialPivotStrategy`, or `CompletePivotStrategy`.

1. Validate dimensions and options, then inspect input finiteness and matrix scale.
2. Clone the matrix and RHS into working storage.
3. Select pivots, swap rows and columns, and eliminate entries below each pivot.
4. Accumulate growth, pivot diagnostics and operation counters.
5. Back-substitute and restore the original variable order.
6. Calculate the residual against the original inputs and return a `SolveResult`.

`DenseMatrix` owns contiguous `double[]` storage. The solver is sequential, uses binary64 arithmetic, and has no per-cell allocations or LINQ in elimination loops. It does not form an inverse or round intermediate values.

A result contains a status, a read-only solution on success, and an immutable metrics snapshot. Numerical failures return no solution and retain partial metrics. Invalid dimensions or options throw argument exceptions. See [algorithms](algorithms.md) and [metrics](metrics.md).

## Experiment and reporting flow

```text
CLI options -> FixtureFactory / ExperimentPresets
            -> LinearSystemFixture instances
            -> ExperimentRunner
            -> ExperimentRecord collection
            -> console + CSV + Markdown + SVG
```

Every fixture derives its RHS from its matrix and known solution. The runner passes identical fixture data to every strategy and calculates forward error outside the solver timer. Records preserve fixture, repetition, and requested strategy order. Report writers consume those records; SVG generation does not rerun experiments.

The benchmark harness references Core directly and creates fixtures outside timed measurement. Solver cloning and diagnostics remain inside measurement.

## Repository layout

| Path | Contents |
|---|---|
| `src/PivotGrowth.Core/` | Numerics, pivot strategies, solver, validation, fixtures and metrics |
| `src/PivotGrowth.Cli/` | Argument parsing, experiment orchestration and report writers |
| `src/PivotGrowth.Benchmarks/` | BenchmarkDotNet entry point and solver benchmarks |
| `tests/PivotGrowth.Tests/` | Numerical, reporting and CLI regression tests |
| `docs/` | Usage and implementation reference |
| `docs/results/` | Saved canonical CSV and selected SVG charts used by documentation |
| `artifacts/` | Ignored generated experiment and benchmark output |
| `.github/workflows/ci.yml` | Windows/Linux build, tests, formatting and smoke checks |

## Maintenance checks

Nullable references, analyzers, deterministic builds and warnings as errors are enabled. Numerical tests use deterministic inputs and tolerances, with exact assertions for guaranteed binary values. Dedicated regressions cover Wilkinson growth, column-permutation restoration, non-finite arithmetic, operation counts and input preservation. Reporting tests cover locale independence, finite SVG coordinates and CLI output.

Run the [validation commands](usage.md#validation) after changes. Timing relationships belong in measured benchmark analysis, not correctness assertions.
