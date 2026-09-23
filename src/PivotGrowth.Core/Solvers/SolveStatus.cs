namespace PivotGrowth.Core.Solvers;

public enum SolveStatus
{
    Success,
    /// <summary>A zero pivot prevented this strategy from proceeding; without pivoting this need not imply singularity.</summary>
    ZeroPivot,
    Singular,
    NonFinite
}
