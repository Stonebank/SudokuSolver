using System.Numerics;

namespace SudokuSolver.Domain;

public enum SudokuSolveStatus { Solved, InvalidPuzzle, NoSolution, MultipleSolutions, Cancelled, InternalFailure }
public sealed record SudokuSolverOptions(bool RequireUniqueSolution = false, int MaxSolutions = 2);
public sealed record SudokuSolverStats(int Backtracks, int Assignments, int EmptyCells, int SolutionsFound);
public sealed record SudokuSolveResult(SudokuSolveStatus Status, SudokuGrid? Solution, SudokuSolverStats Stats, string? FailureReason)
{
    public bool IsSolved => Status is SudokuSolveStatus.Solved;
}
public interface ISudokuSolver { SudokuSolveResult Solve(SudokuGrid puzzle, SudokuSolverOptions? options = null, CancellationToken cancellationToken = default); }

public sealed class BitMaskSudokuSolver : ISudokuSolver
{
    private const int All = 0b11_1111_1110;
    public SudokuSolveResult Solve(SudokuGrid puzzle, SudokuSolverOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new SudokuSolverOptions();
        var grid = puzzle.ToArray(); Span<int> rows = stackalloc int[9]; Span<int> cols = stackalloc int[9]; Span<int> boxes = stackalloc int[9];
        for (var i=0;i<9;i++) rows[i]=cols[i]=boxes[i]=All;
        var empty = 0;
        for (var i=0;i<81;i++)
        {
            var digit = grid[i]; if (digit == 0) { empty++; continue; }
            var bit = 1 << digit; var row=i/9; var col=i%9; var box=(row/3)*3+col/3;
            if ((rows[row]&bit)==0 || (cols[col]&bit)==0 || (boxes[box]&bit)==0) return new(SudokuSolveStatus.InvalidPuzzle, null, new(0,0,empty,0), $"Duplicate digit {digit} at r{row+1}c{col+1}.");
            rows[row]&=~bit; cols[col]&=~bit; boxes[box]&=~bit;
        }
        var solver = new Search(grid, rows.ToArray(), cols.ToArray(), boxes.ToArray(), options, empty, cancellationToken);
        try
        {
            solver.Run();
            if (cancellationToken.IsCancellationRequested) return new(SudokuSolveStatus.Cancelled, null, solver.Stats, "Solving was cancelled.");
            if (solver.Solutions == 0) return new(SudokuSolveStatus.NoSolution, null, solver.Stats, "Puzzle has no solution.");
            if (options.RequireUniqueSolution && solver.Solutions > 1) return new(SudokuSolveStatus.MultipleSolutions, new SudokuGrid(solver.FirstSolution), solver.Stats, "Puzzle has multiple solutions.");
            return new(SudokuSolveStatus.Solved, new SudokuGrid(solver.FirstSolution), solver.Stats, null);
        }
        catch (OperationCanceledException) { return new(SudokuSolveStatus.Cancelled, null, solver.Stats, "Solving was cancelled."); }
        catch (Exception ex) { return new(SudokuSolveStatus.InternalFailure, null, solver.Stats, ex.Message); }
    }
    private sealed class Search(int[] grid, int[] rows, int[] cols, int[] boxes, SudokuSolverOptions options, int emptyCells, CancellationToken ct)
    {
        private int _backtracks; private int _assignments; public int Solutions { get; private set; } public int[] FirstSolution { get; } = new int[81];
        public SudokuSolverStats Stats => new(_backtracks,_assignments,emptyCells,Solutions);
        public void Run() => SolveCore(emptyCells);
        private bool SolveCore(int remaining)
        {
            ct.ThrowIfCancellationRequested();
            if (remaining == 0) { Solutions++; if (Solutions == 1) Array.Copy(grid, FirstSolution, 81); return !options.RequireUniqueSolution || Solutions >= Math.Max(1, options.MaxSolutions); }
            var best=-1; var bestMask=0; var bestCount=10;
            for (var i=0;i<81;i++) if (grid[i]==0) { var r=i/9; var c=i%9; var b=(r/3)*3+c/3; var mask=rows[r]&cols[c]&boxes[b]; var count=BitOperations.PopCount((uint)mask); if (count==0) return false; if (count<bestCount) { best=i; bestMask=mask; bestCount=count; if (count==1) break; } }
            var row=best/9; var col=best%9; var box=(row/3)*3+col/3;
            for (var mask=bestMask; mask!=0; mask &= mask-1)
            {
                var bit = mask & -mask; var digit = BitOperations.TrailingZeroCount(bit); grid[best]=digit; rows[row]&=~bit; cols[col]&=~bit; boxes[box]&=~bit; _assignments++;
                if (SolveCore(remaining-1)) return true;
                rows[row]|=bit; cols[col]|=bit; boxes[box]|=bit; grid[best]=0; _backtracks++;
            }
            return false;
        }
    }
}
