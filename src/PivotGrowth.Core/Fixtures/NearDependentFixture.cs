namespace PivotGrowth.Core.Fixtures;

public static class NearDependentFixture
{
    /// <summary>The last row is row zero plus epsilon times the original last row of a fixed diagonally dominant matrix.</summary>
    public static LinearSystemFixture Create(int size, double epsilon = 1e-8, ulong seed = 42)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 2);
        if (!double.IsFinite(epsilon) || epsilon < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon));
        }
        LinearSystemFixture basis = DiagonallyDominantFixture.Create(size, seed);
        var matrix = basis.Matrix.Clone();
        for (int column = 0; column < size; column++)
        {
            matrix[size - 1, column] = matrix[0, column] + epsilon * matrix[size - 1, column];
        }
        return new LinearSystemFixture("near-dependent", matrix, basis.TrueSolution.ToArray(), seed, epsilon);
    }
}
