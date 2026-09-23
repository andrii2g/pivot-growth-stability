namespace PivotGrowth.Core.Metrics;

internal sealed class MetricsAccumulator
{
    private readonly double originalMaxAbs;

    public MetricsAccumulator(double originalMaxAbs)
    {
        this.originalMaxAbs = originalMaxAbs;
        MaxObservedAbs = originalMaxAbs;
    }

    public long PivotComparisons;
    public long RowSwaps;
    public long ColumnSwaps;
    public long EliminationMultipliers;
    public long MatrixUpdates;
    public long Divisions;
    public double MaxObservedAbs;
    public double MinimumAbsolutePivot = double.NaN;
    public int TinyPivotCount;
    public bool EncounteredNonFiniteValue;

    public bool Observe(double value, bool matrixElement = false)
    {
        if (!double.IsFinite(value))
        {
            EncounteredNonFiniteValue = true;
            return false;
        }
        if (matrixElement)
        {
            MaxObservedAbs = Math.Max(MaxObservedAbs, Math.Abs(value));
        }
        return true;
    }

    public SolveMetrics Snapshot(TimeSpan elapsed, double residual) => new(
        PivotComparisons, RowSwaps, ColumnSwaps, EliminationMultipliers, MatrixUpdates, Divisions,
        originalMaxAbs, MaxObservedAbs,
        originalMaxAbs > 0 && double.IsFinite(originalMaxAbs) && !EncounteredNonFiniteValue
            ? MaxObservedAbs / originalMaxAbs : double.NaN,
        MinimumAbsolutePivot, TinyPivotCount, residual, EncounteredNonFiniteValue, elapsed);
}
