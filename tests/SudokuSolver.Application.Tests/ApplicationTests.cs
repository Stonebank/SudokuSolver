using FluentAssertions;
using SudokuSolver.Application;
using SudokuSolver.Domain;
using Xunit;

namespace SudokuSolver.Application.Tests;

public sealed class ApplicationTests
{
    [Fact] public void Validator_FindsDuplicateRow() { var grid=SudokuGrid.Parse("550070000600195000098000060800060003400803001700020006060000280000419005000080079"); PuzzleValidator.Validate(grid).Should().Contain("row"); }
    [Fact] public void Mapper_MapsCenter() { var (x,y)=new BoardCoordinateMapper(90,90,900,900).CellCenter(0,0); x.Should().Be(140); y.Should().Be(140); }
}
