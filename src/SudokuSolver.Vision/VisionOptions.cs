namespace SudokuSolver.Vision;

public sealed class VisionOptions
{
    public required int NormalizedBoardSize { get; init; } = 450;
    public required double MinBoardConfidence { get; init; } = 0.70;
    public required double MinGridConfidence { get; init; } = 0.65;
    public required double MinContourAreaRatio { get; init; } = 0.04;
    public required int CellInnerCropPercent { get; init; } = 12;
}
