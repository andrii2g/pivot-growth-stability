using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class MetricsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(10)]
    public void CountersMatchEliminationAndPivotSearches(int size)
    {
        LinearSystemFixture fixture = DiagonallyDominantFixture.Create(size);
        long multipliers = (long)size * (size - 1) / 2;
        long updates = (long)size * (size - 1) * (2 * size - 1) / 6;
        IPivotStrategy[] strategies = [new NoPivotStrategy(), new PartialPivotStrategy(), new CompletePivotStrategy()];
        long[] comparisons = [0, multipliers, (long)size * (size + 1) * (2 * size + 1) / 6 - size];
        for (int index = 0; index < strategies.Length; index++)
        {
            SolveResult result = new GaussianEliminationSolver(strategies[index])
                .Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
            Assert.Equal(comparisons[index], result.Metrics.PivotComparisons);
            Assert.Equal(multipliers, result.Metrics.EliminationMultipliers);
            Assert.Equal(updates, result.Metrics.MatrixUpdates);
            Assert.Equal(multipliers + size, result.Metrics.Divisions);
            Assert.True(result.Metrics.GrowthFactor >= 1);
            Assert.False(result.Metrics.EncounteredNonFiniteValue);
        }
    }

    [Fact]
    public void ErrorMetricsUseSpecifiedNorms()
    {
        var matrix = new DenseMatrix(2, 2, [2, 0, 0, 4]);
        // Residual is [0, 4]; denominator is 4*2+4 = 12.
        Assert.Equal(1.0 / 3, MatrixMath.RelativeResidual(matrix, [1, 2], [2, 4]), 14);
        Assert.Equal(1 / Math.Sqrt(2), VectorMath.RelativeForwardError([1, 2], [1, 1]), 14);
    }
}
