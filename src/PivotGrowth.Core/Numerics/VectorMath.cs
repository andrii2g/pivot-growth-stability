namespace PivotGrowth.Core.Numerics;

public static class VectorMath
{
    public static double InfinityNorm(ReadOnlySpan<double> vector)
    {
        double maximum = 0;
        foreach (double value in vector)
        {
            maximum = Math.Max(maximum, Math.Abs(value));
        }
        return maximum;
    }

    /// <summary>Scaled sum of squares avoids unnecessary overflow and underflow.</summary>
    public static double EuclideanNorm(ReadOnlySpan<double> vector)
    {
        double scale = InfinityNorm(vector);
        if (scale == 0 || !double.IsFinite(scale))
        {
            return scale;
        }
        double sum = 0;
        foreach (double value in vector)
        {
            double scaled = value / scale;
            sum += scaled * scaled;
        }
        return scale * Math.Sqrt(sum);
    }

    public static double RelativeForwardError(ReadOnlySpan<double> solution, ReadOnlySpan<double> expected)
    {
        if (solution.Length != expected.Length)
        {
            throw new ArgumentException("Vector lengths must match.", nameof(expected));
        }
        double[] difference = new double[solution.Length];
        for (int index = 0; index < solution.Length; index++)
        {
            difference[index] = solution[index] - expected[index];
        }
        double numerator = EuclideanNorm(difference);
        double denominator = EuclideanNorm(expected);
        return denominator == 0 ? (numerator == 0 ? 0 : double.NaN) : numerator / denominator;
    }
}
