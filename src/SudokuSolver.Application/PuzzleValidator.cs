using SudokuSolver.Domain;

namespace SudokuSolver.Application;

public static class PuzzleValidator
{
    public static string? Validate(SudokuGrid grid)
    {
        Span<int> rows = stackalloc int[9]; Span<int> cols = stackalloc int[9]; Span<int> boxes = stackalloc int[9]; var givens=0;
        for (var i=0;i<81;i++)
        {
            var d=grid[i]; if (d==0) continue; givens++; var bit=1<<d; var r=i/9; var c=i%9; var b=(r/3)*3+c/3;
            if ((rows[r]&bit)!=0) return $"Duplicate digit {d} in row {r+1}."; rows[r]|=bit;
            if ((cols[c]&bit)!=0) return $"Duplicate digit {d} in column {c+1}."; cols[c]|=bit;
            if ((boxes[b]&bit)!=0) return $"Duplicate digit {d} in box {b+1}."; boxes[b]|=bit;
        }
        return givens < 8 ? "Too few givens were recognized to trust OCR." : null;
    }
}
