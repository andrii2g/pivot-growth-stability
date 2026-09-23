using System.Diagnostics;
using PivotGrowth.Core.Metrics;
using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Validation;

namespace PivotGrowth.Core.Solvers;

/// <summary>One elimination path for all pivot strategies. Input matrix and RHS are never modified.</summary>
public sealed class GaussianEliminationSolver(IPivotStrategy pivotStrategy)
{
    private readonly IPivotStrategy strategy = pivotStrategy ?? throw new ArgumentNullException(nameof(pivotStrategy));

    public SolveResult Solve(DenseMatrix matrix, ReadOnlySpan<double> rightHandSide, SolveOptions? options = null)
    {
        LinearSystemValidator.ValidateDimensions(matrix, rightHandSide);
        options ??= new SolveOptions();
        if (!double.IsFinite(options.TinyPivotMultiplier) || options.TinyPivotMultiplier < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }
        var timer = Stopwatch.StartNew();
        double originalMax = MatrixMath.MaximumAbsoluteElement(matrix);
        var metrics = new MetricsAccumulator(originalMax);

        SolveResult Finish(SolveStatus status, double[]? solution = null, double residual = double.NaN)
        {
            timer.Stop();
            return new SolveResult(status, solution is null ? null : Array.AsReadOnly(solution),
                metrics.Snapshot(timer.Elapsed, residual));
        }

        if (!LinearSystemValidator.IsFinite(matrix, rightHandSide))
        {
            metrics.EncounteredNonFiniteValue = true;
            return Finish(SolveStatus.NonFinite);
        }
        if (originalMax == 0)
        {
            metrics.MinimumAbsolutePivot = 0;
            return Finish(SolveStatus.Singular);
        }

        int size = matrix.Rows;
        DenseMatrix working = matrix.Clone();
        double[] rhs = rightHandSide.ToArray();
        int[] permutation = new int[size];
        for (int index = 0; index < size; index++)
        {
            permutation[index] = index;
        }
        double threshold = options.TinyPivotMultiplier * FloatingPoint.MachineEpsilon * size * Math.Max(1, originalMax);
        for (int step = 0; step < size; step++)
        {
            Pivot pivot = strategy.SelectPivot(working, step);
            if (pivot.Row < step || pivot.Row >= size || pivot.Column < step || pivot.Column >= size || pivot.Comparisons < 0)
            {
                throw new InvalidOperationException("Pivot strategy returned an invalid active pivot.");
            }
            metrics.PivotComparisons += pivot.Comparisons;
            if (pivot.Row != step)
            {
                working.SwapRows(step, pivot.Row);
                (rhs[step], rhs[pivot.Row]) = (rhs[pivot.Row], rhs[step]);
                metrics.RowSwaps++;
            }
            if (pivot.Column != step)
            {
                working.SwapColumns(step, pivot.Column);
                (permutation[step], permutation[pivot.Column]) = (permutation[pivot.Column], permutation[step]);
                metrics.ColumnSwaps++;
            }
            double absolutePivot = Math.Abs(working[step, step]);
            metrics.MinimumAbsolutePivot = double.IsNaN(metrics.MinimumAbsolutePivot)
                ? absolutePivot : Math.Min(metrics.MinimumAbsolutePivot, absolutePivot);
            if (absolutePivot == 0)
            {
                // A zero selected pivot is not sufficient evidence of singularity for an arbitrary strategy.
                bool zeroColumn = true;
                for (int row = step; row < size; row++)
                {
                    zeroColumn &= working[row, step] == 0;
                }
                return Finish(zeroColumn ? SolveStatus.Singular : SolveStatus.ZeroPivot);
            }
            if (absolutePivot <= threshold)
            {
                metrics.TinyPivotCount++;
            }
            for (int row = step + 1; row < size; row++)
            {
                double multiplier = working[row, step] / working[step, step];
                metrics.EliminationMultipliers++;
                metrics.Divisions++;
                if (!metrics.Observe(multiplier))
                {
                    return Finish(SolveStatus.NonFinite);
                }
                working[row, step] = 0;
                for (int column = step + 1; column < size; column++)
                {
                    double updated = working[row, column] - multiplier * working[step, column];
                    working[row, column] = updated;
                    metrics.MatrixUpdates++;
                    if (!metrics.Observe(updated, matrixElement: true))
                    {
                        return Finish(SolveStatus.NonFinite);
                    }
                }
                rhs[row] -= multiplier * rhs[step];
                if (!metrics.Observe(rhs[row]))
                {
                    return Finish(SolveStatus.NonFinite);
                }
            }
        }
        double[]? permuted = BackSubstitution.Solve(working, rhs, metrics);
        if (permuted is null)
        {
            return Finish(SolveStatus.NonFinite);
        }
        double[] solution = new double[size];
        for (int column = 0; column < size; column++)
        {
            solution[permutation[column]] = permuted[column];
        }
        double residual = MatrixMath.RelativeResidual(matrix, solution, rightHandSide);
        if (!metrics.Observe(residual))
        {
            return Finish(SolveStatus.NonFinite);
        }
        return Finish(SolveStatus.Success, solution, residual);
    }
}
