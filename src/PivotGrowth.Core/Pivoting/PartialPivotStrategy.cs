using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Pivoting;

public sealed class PartialPivotStrategy : IPivotStrategy
{
    public string Name => "partial";

    public Pivot SelectPivot(DenseMatrix matrix, int pivotIndex)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        double maximum = Math.Abs(matrix[pivotIndex, pivotIndex]);
        int pivotRow = pivotIndex;
        long comparisons = 0;
        for (int row = pivotIndex + 1; row < matrix.Rows; row++)
        {
            comparisons++;
            double candidate = Math.Abs(matrix[row, pivotIndex]);
            if (candidate > maximum)
            {
                maximum = candidate;
                pivotRow = row;
            }
        }
        return new Pivot(pivotRow, pivotIndex, comparisons);
    }
}
