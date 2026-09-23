using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Pivoting;

public sealed class CompletePivotStrategy : IPivotStrategy
{
    public string Name => "complete";

    public Pivot SelectPivot(DenseMatrix matrix, int pivotIndex)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        double maximum = Math.Abs(matrix[pivotIndex, pivotIndex]);
        int pivotRow = pivotIndex;
        int pivotColumn = pivotIndex;
        long comparisons = 0;
        for (int row = pivotIndex; row < matrix.Rows; row++)
        {
            for (int column = pivotIndex; column < matrix.Columns; column++)
            {
                if (row == pivotIndex && column == pivotIndex)
                {
                    continue;
                }
                comparisons++;
                double candidate = Math.Abs(matrix[row, column]);
                if (candidate > maximum)
                {
                    maximum = candidate;
                    pivotRow = row;
                    pivotColumn = column;
                }
            }
        }
        return new Pivot(pivotRow, pivotColumn, comparisons);
    }
}
