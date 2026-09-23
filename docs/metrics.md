# Metrics

All metrics use the original input matrix and RHS where indicated. Solvers preserve those inputs.

| Field | Definition |
|---|---|
| PivotComparisons | Candidate magnitude comparisons against the current best, excluding initialization. No pivoting: 0; partial: n(n−1)/2; complete: n(n+1)(2n+1)/6−n for a completed solve. Ties retain the first candidate in row-major order. |
| RowSwaps / ColumnSwaps | Actual exchanges of distinct rows or columns. RHS and permutation bookkeeping are included in the corresponding exchange, not counted separately. |
| EliminationMultipliers | One for each attempted a[i,k]/a[k,k], including a zero numerator. Completed solve: n(n−1)/2. |
| MatrixUpdates | One attempted a[i,j]−multiplier*a[k,j] for each trailing matrix cell. Completed solve: n(n−1)(2n−1)/6. Excludes RHS updates, explicit zeros and back substitution. |
| Divisions | Multiplier divisions plus back-substitution divisions. Completed solve: n(n−1)/2+n. Excludes diagnostic norms, residuals and growth ratios. |
| OriginalMaxAbs | max absolute element in the original coefficient matrix. |
| MaxObservedAbs | Maximum of the original maximum and all finite updated coefficient elements, across every elimination step. Excludes multipliers and RHS. On failure it is only the maximum finite value seen before termination. |
| GrowthFactor | MaxObservedAbs / OriginalMaxAbs. Unavailable for a zero matrix or non-finite failure. Intermediate trailing entries are included, not only final U. |
| MinimumAbsolutePivot | Minimum absolute selected diagonal pivot inspected, including an exact-zero pivot that terminates elimination. Unavailable if no pivot was inspected, except zero matrices report 0. |
| TinyPivotCount | Number of nonzero pivots <= multiplier × machine epsilon × n × max(1, OriginalMaxAbs). Default multiplier: 1. Tiny pivots do not stop the solve. |
| RelativeResidual | ‖Ax−b‖∞ / (‖A‖∞ × ‖x‖∞ + ‖b‖∞), with ‖A‖∞ = maximum absolute row sum. This is a normwise backward-error diagnostic. |
| RelativeForwardError | ‖x−x_true‖₂ / ‖x_true‖₂. Calculated by the experiment runner, outside the solver timer. |
| EncounteredNonFiniteValue | Non-finite input or arithmetic detected in elimination, RHS updates, back substitution or residual calculation. |
| Elapsed | Wall time including finite-input validation, matrix maximum scan, input copies, elimination, back substitution and residual. Excludes dimension/option validation, fixture construction, forward error and report generation. |

Counters retain partial values on failure. They describe operations, not a total FLOP estimate.

An identically zero numerator and denominator produce zero error. A zero denominator with nonzero error is unavailable. Euclidean norms use scaled sums of squares; matrix products and infinity norms use ordinary binary64 arithmetic. Overflow in residual evaluation returns an unavailable residual and a non-finite solve status rather than a misleading zero.

CSV uses invariant-culture G17 numbers and empty fields for unavailable/non-finite metrics. Markdown uses six significant digits and an em dash for unavailable metrics. Elapsed time is serialized in milliseconds. CSV preserves every run in fixture, repetition, then requested strategy order.

SVG charts consume the same experiment records without re-running a solver. The vertical axis is logarithmic. Zero metrics use a floor one decade below the smallest positive value; missing values are omitted. Near-dependent plots use log epsilon, omit epsilon zero, and separate each matrix size into its own chart. Repetitions remain individual traces.

Single CLI timings include JIT and scheduling effects and are not reliable performance comparisons. Use the separate BenchmarkDotNet harness for warmed-up runtime and allocation measurements; a Dry job only verifies that the harness runs.
