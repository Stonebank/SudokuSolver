using FluentAssertions;
using SudokuSolver.Application;
using Xunit;

namespace SudokuSolver.Automation.Tests;

public sealed class AutomationTests
{
    [Fact] public void CoordinateMapper_OnlyFillsEmptyCellCenters() { var mapper = new BoardCoordinateMapper(0,0,900,900); mapper.CellCenter(8,8).Should().Be((850,850)); }
}
