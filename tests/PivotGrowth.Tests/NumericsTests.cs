using PivotGrowth.Core.Numerics;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class NumericsTests
{
    [Fact]
    public void MatrixOwnsItsBufferAndClone()
    {
        double[] values = [1, 2, 3, 4, 5, 6];
        var matrix = new DenseMatrix(2, 3, values);
        values[0] = 99;
        DenseMatrix clone = matrix.Clone();
        clone[0, 0] = 42;
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(6, matrix[1, 2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix[0, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => matrix[-1, 0]);
    }

    [Fact]
    public void SwapsAndMultiplyRespectRowMajorStorage()
    {
        var matrix = new DenseMatrix(2, 2, [1, 2, 3, 4]);
        matrix.SwapRows(0, 1);
        matrix.SwapColumns(0, 1);
        Assert.Equal(new double[] { 4, 3, 2, 1 }, matrix.Values.ToArray());
        Assert.Equal(new double[] { 10, 4 }, MatrixMath.Multiply(matrix, [1, 2]));
        Assert.Equal(7, MatrixMath.InfinityNorm(matrix));
        Assert.Equal(4, MatrixMath.MaximumAbsoluteElement(matrix));
    }

    [Fact]
    public void NormsHandleScaleAndZero()
    {
        Assert.Equal(5, VectorMath.EuclideanNorm([3, 4]));
        Assert.Equal(0, VectorMath.EuclideanNorm([0, 0]));
        Assert.True(double.IsFinite(VectorMath.EuclideanNorm([1e200, 1e200])));
        Assert.True(VectorMath.EuclideanNorm([1e-200, 1e-200]) > 0);
        Assert.Equal(0, VectorMath.RelativeForwardError([0], [0]));
        Assert.Equal(0, MatrixMath.RelativeResidual(new DenseMatrix(1, 1), [0], [0]));
    }
}
