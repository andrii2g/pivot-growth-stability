namespace PivotGrowth.Core.Solvers;

/// <summary>Tiny pivots are recorded, not rejected. Threshold = multiplier * machine epsilon * n * max(1, original max).</summary>
public sealed record SolveOptions(double TinyPivotMultiplier = 1);
