using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Fixtures;

public static class HilbertFixture
{
    public static LinearSystemFixture Create(int size)
    {
        var matrix = new DenseMatrix(size, size);
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                matrix[row, column] = 1.0 / (row + column + 1);
            }
        }
        double[] solution = new double[size];
        Array.Fill(solution, 1);
        return new LinearSystemFixture("hilbert", matrix, solution);
    }
}
