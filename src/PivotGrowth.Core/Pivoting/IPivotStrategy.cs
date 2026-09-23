using PivotGrowth.Core.Numerics;

namespace PivotGrowth.Core.Pivoting;

/// <summary>Selects a pivot in the active submatrix without mutating the matrix.</summary>
public interface IPivotStrategy
{
    string Name { get; }
    Pivot SelectPivot(DenseMatrix matrix, int pivotIndex);
}
