# Numerical notes

The lab uses IEEE-754 binary64 (`double`) for input generation, elimination and error diagnostics. There is no arbitrary-precision reference solve.

## Machine epsilon and tiny pivots

`FloatingPoint.MachineEpsilon` is:

```text
2.2204460492503130808472633361816e-16
```

This is the spacing above 1, not `double.Epsilon`, which is the smallest positive subnormal value.

The implemented threshold is:

```text
SolveOptions.TinyPivotMultiplier * machineEpsilon * n * max(1, OriginalMaxAbs)
```

The multiplier defaults to 1 and is configurable through the Core API. Nonzero pivots at or below the threshold are counted and used; exact-zero pivots stop the solve. The `max(1, ...)` floor means very small uniformly scaled systems may be flagged even if they are well-conditioned. The threshold is a diagnostic, not a rank test.

## Conditioning, stability and growth

Conditioning describes sensitivity of the mathematical solution to perturbations in the input. Stability describes the algorithm's numerical error. Element growth describes how large coefficients become along the elimination path:

```text
rho = maximum absolute coefficient over all elimination states / original maximum
```

Growth includes the original matrix and every updated trailing coefficient. It excludes RHS entries and elimination multipliers. Growth is not a condition number.

The Wilkinson fixture exposes a path with exponential growth for no and partial pivoting. Hilbert exposes a different problem: low growth can coexist with substantial forward error because the system is ill-conditioned.

## Residual and forward error

Relative residual is `‖Ax−b‖∞ / (‖A‖∞ ‖x‖∞ + ‖b‖∞)`, where the matrix infinity norm is the maximum absolute row sum. This is a normwise backward-error diagnostic.

Relative forward error is `‖x−x_true‖₂ / ‖x_true‖₂`. The known solution comes from fixture generation. Since `b = A*x_true` is itself evaluated in binary64, its rounding is part of the experiment.

A tiny residual does not guarantee a small forward error. `Success` means finite computation completed, not that any error tolerance was met. The implementation does not estimate a condition number.

## Extreme values and reproducibility

Euclidean norms use scaled sums of squares. Matrix-vector products, row sums and residual denominators use ordinary binary64 arithmetic and can overflow. The solver reports non-finite evaluation, including residual overflow, rather than returning a misleading zero residual. Failure results retain partial counters but no solution.

Seeded fixtures use the repository's SplitMix64 generator and a fixed mapping of its high 53 bits to binary64. Pivot ties and iteration order are deterministic. Timings vary across runs; last-bit numerical results can vary across runtime or hardware environments.

See [metrics](metrics.md) for exact conventions, including unavailable values, and [experiments](experiments.md) for fixture construction.
