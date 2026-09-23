using PivotGrowth.Core.Metrics;

namespace PivotGrowth.Core.Solvers;

/// <summary>Failed solves have no solution; their partial operation counters remain available.</summary>
public sealed record SolveResult(SolveStatus Status, IReadOnlyList<double>? Solution, SolveMetrics Metrics);
