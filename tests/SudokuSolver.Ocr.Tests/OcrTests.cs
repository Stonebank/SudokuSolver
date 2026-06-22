using FluentAssertions;
using Microsoft.Extensions.Options;
using SudokuSolver.Application;
using SudokuSolver.Ocr;
using Xunit;

namespace SudokuSolver.Ocr.Tests;

public sealed class OcrTests
{
    [Fact] public void Recognizer_EmptyCell_DoesNotRequireTesseract() { using var r = new TesseractDigitRecognizer(Options.Create(new OcrOptions { TessDataPath = "./missing-tessdata", Language = "eng", ConfidenceThreshold = 0.7 })); var result = r.Recognize(new CellImage(0,0,[],true,1), CancellationToken.None); result.IsEmpty.Should().BeTrue(); }
}
