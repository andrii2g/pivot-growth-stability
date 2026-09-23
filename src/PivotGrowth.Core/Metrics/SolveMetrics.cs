namespace PivotGrowth.Core.Metrics;

public sealed record SolveMetrics(
    long PivotComparisons,
    long RowSwaps,
    long ColumnSwaps,
    long EliminationMultipliers,
    long MatrixUpdates,
    long Divisions,
    double OriginalMaxAbs,
    double MaxObservedAbs,
    double GrowthFactor,
    double MinimumAbsolutePivot,
    int TinyPivotCount,
    double RelativeResidual,
    bool EncounteredNonFiniteValue,
    TimeSpan Elapsed);
