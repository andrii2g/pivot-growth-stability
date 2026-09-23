using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;

namespace PivotGrowth.Cli.Experiments;

public static class ExperimentRunner
{
    /// <summary>Each strategy receives the same original fixture for every repetition. Solvers clone their working data.</summary>
    public static IReadOnlyList<ExperimentRecord> Run(
        IEnumerable<LinearSystemFixture> fixtures, IReadOnlyList<IPivotStrategy> strategies, int repetitions = 1)
    {
        ArgumentNullException.ThrowIfNull(fixtures);
        ArgumentNullException.ThrowIfNull(strategies);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repetitions);
        if (strategies.Count == 0)
        {
            throw new ArgumentException("At least one strategy is required.", nameof(strategies));
        }
        var records = new List<ExperimentRecord>();
        foreach (LinearSystemFixture fixture in fixtures)
        {
            double[] rhs = fixture.RightHandSide.ToArray();
            double[] expected = fixture.TrueSolution.ToArray();
            for (int repetition = 1; repetition <= repetitions; repetition++)
            {
                foreach (IPivotStrategy strategy in strategies)
                {
                    SolveResult result = new GaussianEliminationSolver(strategy).Solve(fixture.Matrix, rhs);
                    double? error = result.Solution is null ? null :
                        VectorMath.RelativeForwardError(result.Solution.ToArray(), expected);
                    records.Add(new ExperimentRecord(fixture.Name, fixture.Matrix.Rows, fixture.Seed, fixture.Epsilon,
                        strategy.Name, repetition, result.Status, error, result.Metrics));
                }
            }
        }
        return records.AsReadOnly();
    }

    public static IPivotStrategy CreateStrategy(string name) => name switch
    {
        "none" => new NoPivotStrategy(),
        "partial" => new PartialPivotStrategy(),
        "complete" => new CompletePivotStrategy(),
        _ => throw new ArgumentException($"Unknown pivot strategy: {name}", nameof(name))
    };
}
