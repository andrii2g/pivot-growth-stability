# Algorithms

All strategies use [GaussianEliminationSolver](../src/PivotGrowth.Core/Solvers/GaussianEliminationSolver.cs). Only pivot selection changes.

## Elimination

For each step `k = 0..n-1`:

1. Select an active pivot `(p,q)`.
2. Exchange rows `k` and `p`, including RHS entries.
3. Exchange columns `k` and `q`, including column-permutation entries.
4. Inspect the selected pivot for exact zero and the tiny-pivot threshold.
5. Eliminate each row `i > k`.

For each row below the pivot:

```text
multiplier = A[i,k] / A[k,k]
A[i,k] = 0
A[i,j] = A[i,j] - multiplier * A[k,j]    for j = k+1..n-1
b[i]   = b[i]   - multiplier * b[k]
```

Updated coefficient magnitudes contribute to element growth. Multipliers and RHS values are checked for finiteness but do not contribute to coefficient growth. Working copies preserve the original inputs.

## Pivot selection

| Strategy | Search | Exchanges |
|---|---|---|
| None | Always selects (k,k) | None |
| Partial | Largest absolute entry in column k, rows k..n−1 | Rows |
| Complete | Largest absolute entry in the active trailing submatrix | Rows and columns |

Candidates are visited in row-major order. Only a strictly larger magnitude replaces the current best, so ties retain the smallest row and then column index.

## Back substitution and variable order

Back substitution processes rows from `n-1` down to zero:

```text
y[i] = (b[i] - sum(A[i,j] * y[j], j=i+1..n-1)) / A[i,i]
```

The column permutation starts as `permutation[i] = i` and follows every column exchange. The returned solution restores the original variable order:

```text
solution[permutation[currentColumn]] = y[currentColumn]
```

## Failure handling

An exact-zero pivot stops elimination. If the active column is all zero, the status is `Singular`; otherwise it is `ZeroPivot`. In particular, no pivoting can fail on an invertible matrix whose current diagonal is zero.

A tiny nonzero pivot increments `TinyPivotCount` and computation continues. Non-finite input or arithmetic produces `NonFinite`. These are floating-point diagnoses, not symbolic rank or conditioning tests. See [numerical notes](numerics.md) for thresholds and [metrics](metrics.md) for counters and error definitions.
