using FluentAssertions;
using Microsoft.Extensions.Options;
using SudokuSolver.Vision;
using Xunit;

namespace SudokuSolver.Vision.Tests;

public sealed class VisionTests
{
    [Fact] public void Options_Defaults_AreUsable() { var options = Options.Create(new VisionOptions { NormalizedBoardSize = 450, MinBoardConfidence = 0.70, MinGridConfidence = 0.65, MinContourAreaRatio = 0.04, CellInnerCropPercent = 12 }); options.Value.NormalizedBoardSize.Should().Be(450); }
}
