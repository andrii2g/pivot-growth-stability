namespace PivotGrowth.Core.Numerics;

public static class MatrixMath
{
    public static double[] Multiply(DenseMatrix matrix, ReadOnlySpan<double> vector)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        if (matrix.Columns != vector.Length)
        {
            throw new ArgumentException("Matrix columns must match vector length.", nameof(vector));
        }
        double[] result = new double[matrix.Rows];
        for (int row = 0; row < matrix.Rows; row++)
        {
            double sum = 0;
            for (int column = 0; column < matrix.Columns; column++)
            {
                sum += matrix[row, column] * vector[column];
            }
            result[row] = sum;
        }
        return result;
    }

    public static double MaximumAbsoluteElement(DenseMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        return VectorMath.InfinityNorm(matrix.Values);
    }

    public static double InfinityNorm(DenseMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        double maximum = 0;
        for (int row = 0; row < matrix.Rows; row++)
        {
            double sum = 0;
            for (int column = 0; column < matrix.Columns; column++)
            {
                sum += Math.Abs(matrix[row, column]);
            }
            maximum = Math.Max(maximum, sum);
        }
        return maximum;
    }

    /// <summary>Normwise backward error: ||Ax-b||∞ / (||A||∞ ||x||∞ + ||b||∞).</summary>
    public static double RelativeResidual(DenseMatrix matrix, ReadOnlySpan<double> solution, ReadOnlySpan<double> rightHandSide)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        if (rightHandSide.Length != matrix.Rows)
        {
            throw new ArgumentException("RHS length must match matrix rows.", nameof(rightHandSide));
        }
        double[] residual = Multiply(matrix, solution);
        for (int index = 0; index < residual.Length; index++)
        {
            residual[index] -= rightHandSide[index];
        }
        double numerator = VectorMath.InfinityNorm(residual);
        double denominator = InfinityNorm(matrix) * VectorMath.InfinityNorm(solution)
            + VectorMath.InfinityNorm(rightHandSide);
        if (!double.IsFinite(numerator) || !double.IsFinite(denominator))
        {
            return double.NaN;
        }
        return denominator == 0 ? (numerator == 0 ? 0 : double.NaN) : numerator / denominator;
    }
}
