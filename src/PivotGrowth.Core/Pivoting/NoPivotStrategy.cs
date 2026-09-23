using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Pivoting;

public sealed class NoPivotStrategy : IPivotStrategy
{
    public string Name => "none";

    public Pivot SelectPivot(DenseMatrix matrix, int pivotIndex)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        _ = matrix[pivotIndex, pivotIndex];
        return new Pivot(pivotIndex, pivotIndex, 0);
    }
}
