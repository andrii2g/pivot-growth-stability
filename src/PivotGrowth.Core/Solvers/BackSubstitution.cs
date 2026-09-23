using PivotGrowth.Core.Metrics;
using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Solvers;

internal static class BackSubstitution
{
    internal static double[]? Solve(DenseMatrix matrix, double[] rightHandSide, MetricsAccumulator metrics)
    {
        int size = matrix.Rows;
        double[] solution = new double[size];
        for (int row = size - 1; row >= 0; row--)
        {
            double sum = rightHandSide[row];
            for (int column = row + 1; column < size; column++)
            {
                sum -= matrix[row, column] * solution[column];
                if (!metrics.Observe(sum))
                {
                    return null;
                }
            }
            metrics.Divisions++;
            solution[row] = sum / matrix[row, row];
            if (!metrics.Observe(solution[row]))
            {
                return null;
            }
        }
        return solution;
    }
}
