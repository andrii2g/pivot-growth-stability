# Running the lab

Requires the .NET 10 SDK. The repository pins the 10.0.100 feature baseline and permits later .NET 10 feature bands. Run the commands from the repository root.

## Validation

``## Validation

`sh
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes
dotnet test -c Release
dotnet run --project src/PivotGrowth.Cli -- suite --preset smoke
```

Commands below are single-line commands usable in PowerShell or a Unix shell.

```sh
dotnet run --project src/PivotGrowth.Cli -c Release -- run --fixture wilkinson --sizes 4:40:4 --pivot none,partial,complete --out artifacts/wilkinson
dotnet run --project src/PivotGrowth.Cli -c Release -- run --fixture hilbert --sizes 2:16:1 --out artifacts/hilbert
dotnet run --project src/PivotGrowth.Cli -c Release -- run --fixture near-dependent --sizes 8 --epsilon 1e-2,1e-4,1e-6,1e-8,1e-10,1e-12,1e-14 --seed 42 --out artifacts/near-dependent
dotnet run --project src/PivotGrowth.Cli -c Release -- suite --preset readme --out artifacts/readme
dotnet run --project src/PivotGrowth.Cli -c Release -- suite --preset performance --repetitions 3 --out artifacts/performance
dotnet run --project src/PivotGrowth.Cli -- --help
```

The CLI supports `run` and `suite`. Every option takes a value; unknown, duplicate, missing and command-inapplicable options are errors. Sizes accept comma-separated values or inclusive `start:end:step` ranges; they are sorted and deduplicated. CLI sizes are limited to 1..4096 to prevent accidental oversized allocations. Near-dependent fixtures require size >= 2. Epsilon is nonnegative and finite. Epsilon zero deliberately creates a singular problem.

Each output directory gets `results.csv`, `results.md` and five SVGs per fixture (growth, residual, forward error, comparisons and runtime). Existing files of those names are overwritten; unrelated files are preserved. Use a fresh output directory when changing experiment coverage to avoid retaining old charts.

Exit code 0 means all solves succeeded, 1 means at least one numerical failure (reports are still written), and 2 means invalid input or an output error.


## Commands and defaults

Running with no arguments, `--help`, `-h`, or `help` prints usage. Option names and fixture/strategy names are case-sensitive.

| Option | Command | Default | Accepted values |
|---|---|---|---|
| `--fixture` | `run` | `wilkinson` | The five names in [experiments](experiments.md) |
| `--sizes` | `run` | `8` | Comma-separated sizes or inclusive start:end:step |
| `--epsilon` | `run`, near-dependent only | `1e-8` | Comma-separated finite, nonnegative numbers |
| `--preset` | `suite` | `smoke` | smoke, readme, performance |
| `--pivot` | Both | `none,partial,complete` | Unique comma-separated strategies, in execution order |
| `--seed` | Both | `42` | Unsigned 64-bit integer; used by seeded fixtures |
| `--repetitions` | Both | `1` | Positive integer |
| `--out` | Both | `artifacts/<fixture>` or `artifacts/<preset>` | Output directory, resolved from the working directory |

Near-dependent runs generate every requested size/epsilon pair. Each size gets a separate set of charts with epsilon on the horizontal axis. Other fixture charts use matrix size. Reports retain every repetition rather than averaging results.

## Presets

- `smoke`: all five fixture families at size 8; 15 solves.
- `readme`: Wilkinson 4:40:4, Hilbert 2:16:1, near-dependent size 8 at seven epsilons, and well-conditioned sizes 64,128,256,512; 108 solves.
- `performance`: well-conditioned sizes 64,128,256,512; 12 solves per repetition.

## Benchmarks

```sh
dotnet run --project src/PivotGrowth.Benchmarks -c Release -- --filter "*"
dotnet run --project src/PivotGrowth.Benchmarks -c Release -- --job Dry --filter "*Size: 64*" --artifacts artifacts/benchmark-smoke
```

The full harness measures all three strategies at sizes 64,128,256,512 on the same seeded well-conditioned fixture, with `MemoryDiagnoser`. Fixture construction is outside measurement; solver input cloning and instrumentation are inside. Dry mode is a build/execution check, not statistical evidence. No timing relationship is asserted in tests.

## Library example

```csharp
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;

var fixture = WilkinsonFixture.Create(40);
var solver = new GaussianEliminationSolver(new CompletePivotStrategy());
var result = solver.Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
Console.WriteLine(result.Metrics.GrowthFactor);
```

`Success` means the computation completed with finite arithmetic; it is not an accuracy guarantee. `ZeroPivot` means the chosen strategy could not proceed, without proving singularity. `Singular` means a zero active column was found in the computed elimination state. This is a floating-point diagnosis, not a symbolic proof about exact input values. `NonFinite` means a non-finite input or arithmetic result was detected. Tiny nonzero pivots are warnings in the metrics, not failures.

## Refreshing the documented results

Generate a new snapshot using the documented defaults:

```sh
dotnet run --project src/PivotGrowth.Cli -c Release -- suite --preset readme --seed 42 --repetitions 1 --out artifacts/readme
```

After checking the command succeeds:

1. Copy `results.csv`, `wilkinson-growth.svg`, `hilbert-forward-error.svg`, `hilbert-residual.svg`, and `near-dependent-n8-forward-error.svg` from `artifacts/readme/` into `docs/results/`.
2. Update the README's six table rows from the CSV: all three strategies for Wilkinson n=40 and Hilbert n=12. Display numeric values with six significant digits and invariant culture.
3. Update the snapshot environment (SDK, runtime and platform) and any numerical interpretation affected by the new data.

This is an explicit documentation update: running the suite alone does not replace the saved documentation snapshot. Keep CSV and selected charts from the same run. Single-run elapsed times remain diagnostic.

The saved [Hilbert residual chart](results/hilbert-residual.svg) complements the forward-error chart in the README.
