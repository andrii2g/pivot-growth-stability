using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class PivotTests
{
    [Fact]
    public void StrategiesSelectExpectedPivots()
    {
        var matrix = new DenseMatrix(3, 3, [1, 2, 3, -4, 5, 9, 4, -9, 6]);
        Assert.Equal(new Pivot(0, 0, 0), new NoPivotStrategy().SelectPivot(matrix, 0));
        Assert.Equal(new Pivot(1, 0, 2), new PartialPivotStrategy().SelectPivot(matrix, 0));
        Assert.Equal(new Pivot(1, 2, 8), new CompletePivotStrategy().SelectPivot(matrix, 0));
        Assert.Equal(new Pivot(2, 1, 1), new PartialPivotStrategy().SelectPivot(matrix, 1));
        Assert.Equal(new Pivot(1, 2, 3), new CompletePivotStrategy().SelectPivot(matrix, 1));
        Assert.Equal(new Pivot(2, 2, 0), new CompletePivotStrategy().SelectPivot(matrix, 2));
    }

    [Fact]
    public void EqualMagnitudesKeepFirstRowThenColumn()
    {
        var matrix = new DenseMatrix(2, 2, [-2, 2, 2, -2]);
        Assert.Equal(new Pivot(0, 0, 3), new CompletePivotStrategy().SelectPivot(matrix, 0));
        Assert.Equal(new Pivot(0, 0, 1), new PartialPivotStrategy().SelectPivot(matrix, 0));
    }
}
