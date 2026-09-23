# Experiments

The CLI runs each selected strategy on identical fixture data. Default strategies are `none,partial,complete`, with seed 42 and one repetition. The runner computes forward error from the known solution after each solve.

## Fixture construction

All indices below are zero-based. Every fixture derives `b = A*x_true` in binary64.

| CLI fixture | Matrix | Known solution |
|---|---|---|
| `wilkinson` | Unit diagonal, −1 below it, final column of ones; remaining entries zero | x[i] = (−1)^i / (i+1) |
| `hilbert` | A[i,j] = 1/(i+j+1) | All ones |
| `near-dependent` | A seeded diagonally dominant base, with final row replaced by row zero + epsilon × original final row | x[i] = (−1)^i / (i+1) |
| `diagonally-dominant` | Seeded off-diagonal entries in [−1,1); diagonal equals absolute off-diagonal row sum + 1 | x[i] = (−1)^i / (i+1) |
| `well-conditioned` | Symmetric 2I+E, with seeded off-diagonal entries in [−1/n,1/n) | x[i] = (−1)^i / (i+1) |

For the well-conditioned fixture, absolute off-diagonal row sums are below 1. Eigenvalues lie between 1 and 3, so its spectral condition number is below 3.

Only the diagonally dominant, near-dependent and well-conditioned families use the requested seed. Hilbert and Wilkinson have fixed construction and record their default seed value of 42.

## Canonical coverage

| Preset | Fixture coverage | Solves with default strategies and one repetition |
|---|---|---:|
| `smoke` | All five families at n=8; near-dependent epsilon=1e−8 | 15 |
| `readme` | Wilkinson n=4:40:4; Hilbert n=2:16:1; near-dependent n=8 at seven epsilons; well-conditioned n=64,128,256,512 | 108 |
| `performance` | Well-conditioned n=64,128,256,512 | 12 |

The seven canonical epsilons are `1e-2,1e-4,1e-6,1e-8,1e-10,1e-12,1e-14`. Keeping size and seed fixed keeps the near-dependent base and perturbation direction fixed. Epsilon zero is accepted for an explicit singular-system experiment.

## Interpreting results

- Wilkinson demonstrates coefficient growth of 2^(n−1) under no and partial pivoting for the tested sizes; complete pivoting holds growth to 2. The alternating known solution avoids artificially exact all-ones arithmetic.
- Hilbert demonstrates that a small residual and low growth do not ensure a small forward error.
- Near-dependent rows expose increasing sensitivity as epsilon shrinks. Individual errors need not increase monotonically.
- Well-conditioned inputs provide the canonical performance baseline. The separate BenchmarkDotNet harness measures runtime and allocations without fixture construction.

The [saved CSV](results/results.csv) contains the actual README snapshot. Selected plots are linked in the [README](../README.md). Commands and the procedure for refreshing this snapshot are in [usage](usage.md).
