using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Fixtures;

public static class RandomWellConditionedFixture
{
    /// <summary>Symmetric 2I+E with off-diagonal row sums below one; eigenvalues lie between one and three.</summary>
    public static LinearSystemFixture Create(int size, ulong seed = 42)
    {
        var matrix = new DenseMatrix(size, size);
        var random = new DeterministicRng(seed);
        for (int row = 0; row < size; row++)
        {
            matrix[row, row] = 2;
            for (int column = row + 1; column < size; column++)
            {
                double value = random.NextSignedDouble() / size;
                matrix[row, column] = value;
                matrix[column, row] = value;
            }
        }
        return new LinearSystemFixture("well-conditioned", matrix, LinearSystemFixture.CreateSolution(size), seed);
    }
}
