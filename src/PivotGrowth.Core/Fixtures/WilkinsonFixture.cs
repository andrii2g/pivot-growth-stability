using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Fixtures;

public static class WilkinsonFixture
{
    /// <summary>Unit diagonal, -1 below it, and a final column of ones.</summary>
    public static LinearSystemFixture Create(int size)
    {
        var matrix = new DenseMatrix(size, size);
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                matrix[row, column] = column == size - 1 || row == column ? 1 : row > column ? -1 : 0;
            }
        }
        return new LinearSystemFixture("wilkinson", matrix, LinearSystemFixture.CreateSolution(size));
    }
}
