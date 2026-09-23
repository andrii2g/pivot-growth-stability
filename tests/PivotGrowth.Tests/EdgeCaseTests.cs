using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class EdgeCaseTests
{
    [Fact]
    public void GrowthIncludesIntermediateEntriesThatDisappearFromFinalU()
    {
        // The first step creates -4 at (2,1), then the second step eliminates it.
        var matrix = new DenseMatrix(3, 3, [1, 1, 1, 1, -1, 1, 1, -3, 0]);
        SolveResult result = new GaussianEliminationSolver(new NoPivotStrategy()).Solve(matrix, [3, 1, -2]);
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.Equal(3, result.Metrics.OriginalMaxAbs);
        Assert.Equal(4, result.Metrics.MaxObservedAbs);
        Assert.Equal(4.0 / 3, result.Metrics.GrowthFactor);
    }

    [Fact]
    public void BackSubstitutionAndResidualOverflowAreExplicitFailures()
    {
        var solver = new GaussianEliminationSolver(new NoPivotStrategy());
        SolveResult substitution = solver.Solve(new DenseMatrix(1, 1, [1e-300]), [1e300]);
        Assert.Equal(SolveStatus.NonFinite, substitution.Status);
        Assert.Null(substitution.Solution);
        SolveResult residual = solver.Solve(new DenseMatrix(1, 1, [1e308]), [1e308]);
        Assert.Equal(SolveStatus.NonFinite, residual.Status);
        Assert.True(residual.Metrics.EncounteredNonFiniteValue);
        Assert.Null(residual.Solution);
    }

    [Fact]
    public void CompletePivotingRestoresSolutionsAcrossSeededDenseMatrices()
    {
        var random = new DeterministicRng(1729);
        var solver = new GaussianEliminationSolver(new CompletePivotStrategy());
        for (int size = 2; size <= 12; size++)
        {
            var matrix = new DenseMatrix(size, size);
            double[] expected = new double[size];
            for (int row = 0; row < size; row++)
            {
                expected[row] = random.NextSignedDouble();
                for (int column = 0; column < size; column++)
                {
                    matrix[row, column] = random.NextSignedDouble();
                }
            }
            SolveResult result = solver.Solve(matrix, MatrixMath.Multiply(matrix, expected));
            Assert.Equal(SolveStatus.Success, result.Status);
            Assert.True(VectorMath.RelativeForwardError(result.Solution!.ToArray(), expected) < 1e-12);
        }
    }
}
