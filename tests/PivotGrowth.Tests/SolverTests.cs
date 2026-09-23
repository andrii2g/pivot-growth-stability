using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class SolverTests
{
    public static TheoryData<IPivotStrategy> Strategies => new()
    {
        new NoPivotStrategy(), new PartialPivotStrategy(), new CompletePivotStrategy()
    };

    [Theory]
    [MemberData(nameof(Strategies))]
    public void SolvesSystemWithoutMutatingInputs(IPivotStrategy strategy)
    {
        double[] data = [4, 1, 2, 1, 5, 1, 2, 1, 6];
        var matrix = new DenseMatrix(3, 3, data);
        double[] expected = [2, -3, 4];
        double[] rhs = MatrixMath.Multiply(matrix, expected);
        double[] rhsCopy = (double[])rhs.Clone();
        SolveResult result = new GaussianEliminationSolver(strategy).Solve(matrix, rhs);
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.NotNull(result.Solution);
        Assert.True(VectorMath.RelativeForwardError(result.Solution.ToArray(), expected) < 1e-14);
        Assert.True(result.Metrics.RelativeResidual < 1e-14);
        Assert.Equal(data, matrix.Values.ToArray());
        Assert.Equal(rhsCopy, rhs);
        Assert.Equal(3, result.Metrics.EliminationMultipliers);
        Assert.Equal(5, result.Metrics.MatrixUpdates);
        Assert.Equal(6, result.Metrics.Divisions);
    }

    [Fact]
    public void PartialPivotingRescuesInvertibleZeroDiagonal()
    {
        var matrix = new DenseMatrix(2, 2, [0, 1, 2, 3]);
        Assert.Equal(SolveStatus.ZeroPivot, new GaussianEliminationSolver(new NoPivotStrategy()).Solve(matrix, [1, 5]).Status);
        SolveResult result = new GaussianEliminationSolver(new PartialPivotStrategy()).Solve(matrix, [1, 5]);
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.Equal(1, result.Metrics.RowSwaps);
        Assert.Equal(new double[] { 1, 1 }, result.Solution);
    }

    [Fact]
    public void CompletePivotingRestoresMultipleColumnPermutations()
    {
        var matrix = new DenseMatrix(3, 3, [0, 0, 4, 2, 0, 0, 0, 8, 0]);
        double[] expected = [2, -3, 5];
        SolveResult result = new GaussianEliminationSolver(new CompletePivotStrategy())
            .Solve(matrix, MatrixMath.Multiply(matrix, expected));
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.Equal(expected, result.Solution);
        Assert.Equal(2, result.Metrics.ColumnSwaps);
        Assert.True(result.Metrics.RowSwaps > 0);
    }

    [Theory]
    [MemberData(nameof(Strategies))]
    public void DetectsSingularity(IPivotStrategy strategy)
    {
        var solver = new GaussianEliminationSolver(strategy);
        Assert.Equal(SolveStatus.Singular, solver.Solve(new DenseMatrix(2, 2, [1, 2, 2, 4]), [3, 6]).Status);
        SolveResult zero = solver.Solve(new DenseMatrix(2, 2), [0, 0]);
        Assert.Equal(SolveStatus.Singular, zero.Status);
        Assert.Null(zero.Solution);
        Assert.True(double.IsNaN(zero.Metrics.GrowthFactor));
    }

    [Fact]
    public void TinyNonzeroPivotIsReportedButStillSolved()
    {
        SolveResult result = new GaussianEliminationSolver(new NoPivotStrategy())
            .Solve(new DenseMatrix(2, 2, [1, 0, 0, 1e-20]), [1, 1e-20]);
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.Equal(1, result.Metrics.TinyPivotCount);
        Assert.Equal(new double[] { 1, 1 }, result.Solution);
    }

    [Fact]
    public void NonFiniteInputsAndOverflowAreReported()
    {
        var solver = new GaussianEliminationSolver(new NoPivotStrategy());
        SolveResult input = solver.Solve(new DenseMatrix(1, 1, [double.NaN]), [1]);
        Assert.Equal(SolveStatus.NonFinite, input.Status);
        Assert.True(input.Metrics.EncounteredNonFiniteValue);
        SolveResult overflow = solver.Solve(new DenseMatrix(2, 2, [1e-300, 1e300, 1e300, 1]), [1, 1]);
        Assert.Equal(SolveStatus.NonFinite, overflow.Status);
        Assert.True(overflow.Metrics.EncounteredNonFiniteValue);
        Assert.True(double.IsNaN(overflow.Metrics.GrowthFactor));
        Assert.Equal(SolveStatus.NonFinite, solver.Solve(new DenseMatrix(1, 1, [1]), [double.PositiveInfinity]).Status);
    }

    [Fact]
    public void RejectsInvalidDimensionsAndOptions()
    {
        var solver = new GaussianEliminationSolver(new NoPivotStrategy());
        Assert.Throws<ArgumentException>(() => solver.Solve(new DenseMatrix(2, 3), [1, 2]));
        Assert.Throws<ArgumentException>(() => solver.Solve(new DenseMatrix(2, 2), [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => solver.Solve(new DenseMatrix(1, 1), [1], new SolveOptions(-1)));
    }
}
