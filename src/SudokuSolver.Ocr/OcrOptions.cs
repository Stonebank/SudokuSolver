namespace SudokuSolver.Ocr;

public sealed class OcrOptions
{
    public required string TessDataPath { get; init; } = "./tessdata";
    public required string Language { get; init; } = "eng";
    public required double ConfidenceThreshold { get; init; } = 0.70;
}
