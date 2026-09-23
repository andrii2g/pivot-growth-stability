namespace PivotGrowth.Core.Numerics;

/// <summary>A mutable, row-major matrix owning a contiguous buffer.</summary>
public sealed class DenseMatrix
{
    private readonly double[] values;

    public DenseMatrix(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        Rows = rows;
        Columns = columns;
        values = new double[checked(rows * columns)];
    }

    public DenseMatrix(int rows, int columns, ReadOnlySpan<double> values) : this(rows, columns)
    {
        if (values.Length != this.values.Length)
        {
            throw new ArgumentException("Data length must equal rows times columns.", nameof(values));
        }
        values.CopyTo(this.values);
    }

    public int Rows { get; }
    public int Columns { get; }
    public ReadOnlySpan<double> Values => values;

    public double this[int row, int column]
    {
        get => values[Index(row, column)];
        set => values[Index(row, column)] = value;
    }

    public DenseMatrix Clone() => new(Rows, Columns, values);

    public void SwapRows(int first, int second)
    {
        ValidateIndex(first, Rows);
        ValidateIndex(second, Rows);
        for (int column = 0; column < Columns; column++)
        {
            (this[first, column], this[second, column]) = (this[second, column], this[first, column]);
        }
    }

    public void SwapColumns(int first, int second)
    {
        ValidateIndex(first, Columns);
        ValidateIndex(second, Columns);
        for (int row = 0; row < Rows; row++)
        {
            (this[row, first], this[row, second]) = (this[row, second], this[row, first]);
        }
    }

    private int Index(int row, int column)
    {
        ValidateIndex(row, Rows);
        ValidateIndex(column, Columns);
        return row * Columns + column;
    }

    private static void ValidateIndex(int index, int length)
    {
        if ((uint)index >= (uint)length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
