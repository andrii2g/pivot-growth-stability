using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Fixtures;

/// <summary>Known-solution problem. RHS is always derived from the stored matrix and exact-solution vector.</summary>
public sealed class LinearSystemFixture
{
    public LinearSystemFixture(string name, DenseMatrix matrix, ReadOnlySpan<double> trueSolution, ulong seed = 42, double? epsilon = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(matrix);
        if (matrix.Rows != matrix.Columns || trueSolution.Length != matrix.Rows)
        {
            throw new ArgumentException("Fixture requires square matrix and matching solution.", nameof(matrix));
        }
        Name = name;
        Matrix = matrix.Clone();
        TrueSolution = Array.AsReadOnly(trueSolution.ToArray());
        RightHandSide = Array.AsReadOnly(MatrixMath.Multiply(Matrix, trueSolution));
        Seed = seed;
        Epsilon = epsilon;
    }

    public string Name { get; }
    public DenseMatrix Matrix { get; }
    public IReadOnlyList<double> TrueSolution { get; }
    public IReadOnlyList<double> RightHandSide { get; }
    public ulong Seed { get; }
    public double? Epsilon { get; }

    internal static double[] CreateSolution(int size)
    {
        double[] result = new double[size];
        for (int index = 0; index < size; index++)
        {
            result[index] = (index % 2 == 0 ? 1.0 : -1.0) / (index + 1);
        }
        return result;
    }
}
