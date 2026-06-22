namespace SudokuSolver.Domain;

public readonly record struct Cell(int Row, int Column)
{
    public Cell
    {
        if ((uint)Row > 8 || (uint)Column > 8) throw new ArgumentOutOfRangeException(nameof(Row));
    }
    public int Box => (Row / 3) * 3 + Column / 3;
    public int Index => Row * 9 + Column;
}

public sealed class SudokuGrid
{
    private readonly int[] _cells;
    public const int Size = 9;
    public const int CellCount = 81;
    public SudokuGrid() => _cells = new int[CellCount];
    public SudokuGrid(ReadOnlySpan<int> values)
    {
        if (values.Length != CellCount) throw new ArgumentException("A Sudoku grid must contain exactly 81 values.", nameof(values));
        _cells = values.ToArray();
        for (var i = 0; i < _cells.Length; i++) if ((uint)_cells[i] > 9) throw new ArgumentOutOfRangeException(nameof(values), "Digits must be 0-9.");
    }
    public int this[int row, int column] { get => _cells[row * 9 + column]; set { if ((uint)value > 9) throw new ArgumentOutOfRangeException(nameof(value)); _cells[row * 9 + column] = value; } }
    public int this[int index] { get => _cells[index]; set { if ((uint)value > 9) throw new ArgumentOutOfRangeException(nameof(value)); _cells[index] = value; } }
    public int[] ToArray() => (int[])_cells.Clone();
    public SudokuGrid Clone() => new(_cells);
    public int EmptyCount { get { var count=0; for (var i=0;i<CellCount;i++) if (_cells[i]==0) count++; return count; } }
    public static SudokuGrid Parse(string text)
    {
        Span<int> values = stackalloc int[CellCount]; var n=0;
        foreach (var ch in text) if (ch is '.' or '0' || ch is >= '1' and <= '9') { if (n>=CellCount) throw new FormatException("Too many cells."); values[n++] = ch is '.' or '0' ? 0 : ch - '0'; }
        if (n != CellCount) throw new FormatException("Expected 81 cells.");
        return new SudokuGrid(values);
    }
    public override string ToString()
    {
        var chars = new char[CellCount]; for (var i=0;i<CellCount;i++) chars[i] = _cells[i] == 0 ? '.' : (char)('0' + _cells[i]); return new string(chars);
    }
}
