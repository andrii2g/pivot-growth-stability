using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Validation;

public static class LinearSystemValidator
{
    public static void ValidateDimensions(DenseMatrix matrix, ReadOnlySpan<double> rightHandSide)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        if (matrix.Rows != matrix.Columns || rightHandSide.Length != matrix.Rows)
        {
            throw new ArgumentException("A square matrix and matching RHS are required.", nameof(matrix));
        }
    }

    public static bool IsFinite(DenseMatrix matrix, ReadOnlySpan<double> rightHandSide)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        foreach (double value in matrix.Values)
        {
            if (!double.IsFinite(value))
            {
                return false;
            }
        }
        foreach (double value in rightHandSide)
        {
            if (!double.IsFinite(value))
            {
                return false;
            }
        }
        return true;
    }
}
