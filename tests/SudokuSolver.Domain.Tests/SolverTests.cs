using FluentAssertions;
using SudokuSolver.Domain;
using Xunit;

namespace SudokuSolver.Domain.Tests;

public sealed class SolverTests
{
    private readonly ISudokuSolver _solver = new BitMaskSudokuSolver();
    [Theory]
    [InlineData("530070000600195000098000060800060003400803001700020006060000280000419005000080079")]
    [InlineData("000000010400000000020000000000050407008000300001090000300400200050100000000806000")]
    public void Solve_ValidPuzzle_ReturnsSolution(string puzzle)
    { var result = _solver.Solve(SudokuGrid.Parse(puzzle)); result.Status.Should().Be(SudokuSolveStatus.Solved); result.Solution!.EmptyCount.Should().Be(0); }
    [Fact] public void Solve_RowDuplicate_IsInvalid() { _solver.Solve(SudokuGrid.Parse("550070000600195000098000060800060003400803001700020006060000280000419005000080079")).Status.Should().Be(SudokuSolveStatus.InvalidPuzzle); }
    [Fact] public void Solve_ColumnDuplicate_IsInvalid() { _solver.Solve(SudokuGrid.Parse("530070000500195000098000060800060003400803001700020006060000280000419005000080079")).Status.Should().Be(SudokuSolveStatus.InvalidPuzzle); }
    [Fact] public void Solve_BoxDuplicate_IsInvalid() { _solver.Solve(SudokuGrid.Parse("590070000600195000098000060800060003400803001700020006060000280000419005000080079")).Status.Should().Be(SudokuSolveStatus.InvalidPuzzle); }
    [Fact] public void Solve_AlreadySolved_ReturnsSolved() { var solved="534678912672195348198342567859761423426853791713924856961537284287419635345286179"; _solver.Solve(SudokuGrid.Parse(solved)).Status.Should().Be(SudokuSolveStatus.Solved); }
    [Fact] public void Solve_UniquenessDetectsMultiple() { var grid=SudokuGrid.Parse("000000000000000000000000000000000000000000000000000000000000000000000000000000000"); _solver.Solve(grid,new SudokuSolverOptions(true,2)).Status.Should().Be(SudokuSolveStatus.MultipleSolutions); }
}
