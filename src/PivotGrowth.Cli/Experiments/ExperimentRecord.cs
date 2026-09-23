using PivotGrowth.Core.Metrics;
using PivotGrowth.Core.Solvers;

namespace PivotGrowth.Cli.Experiments;

public sealed record ExperimentRecord(
    string Fixture, int Size, ulong Seed, double? Epsilon, string Pivot, int Repetition,
    SolveStatus Status, double? RelativeForwardError, SolveMetrics Metrics);
