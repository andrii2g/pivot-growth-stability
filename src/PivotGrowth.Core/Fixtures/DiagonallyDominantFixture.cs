using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Fixtures;

public static class DiagonallyDominantFixture
{
    public static LinearSystemFixture Create(int size, ulong seed = 42)
    {
        var matrix = new DenseMatrix(size, size);
        var random = new DeterministicRng(seed);
        for (int row = 0; row < size; row++)
        {
            double sum = 0;
            for (int column = 0; column < size; column++)
            {
                if (row != column)
                {
                    matrix[row, column] = random.NextSignedDouble();
                    sum += Math.Abs(matrix[row, column]);
                }
            }
            matrix[row, row] = sum + 1;
        }
        return new LinearSystemFixture("diagonally-dominant", matrix, LinearSystemFixture.CreateSolution(size), seed);
    }
}
