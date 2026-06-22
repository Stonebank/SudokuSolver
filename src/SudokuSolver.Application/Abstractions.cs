using SudokuSolver.Domain;

namespace SudokuSolver.Application;

public enum BrowserKind { Chromium, Firefox, Webkit }
public enum SudokuDifficulty { Easy, Medium, Hard, Expert, Evil }
public enum PipelineExitCode { Success=0, UnexpectedFailure=1, BoardDetectionFailed=2, OcrConfidenceTooLow=3, RecognizedPuzzleInvalid=4, PuzzleHasNoSolution=5, BrowserAutomationFailed=6, TimeoutOrCancellation=7 }

public sealed record SolveCommandOptions(Uri Url, bool Headless, SudokuDifficulty? Difficulty, string? DebugOutputPath, int TimeoutSeconds, double OcrConfidenceThreshold, double BoardConfidenceThreshold, bool Fill, int ViewportWidth, int ViewportHeight, BrowserKind Browser);
public sealed record TimedStage(string Stage, double ElapsedMs, IReadOnlyDictionary<string,string> Properties);
public sealed record PipelineResult(PipelineExitCode ExitCode, SudokuGrid? RecognizedPuzzle, SudokuGrid? Solution, string Message, IReadOnlyList<TimedStage> Timings);
public sealed record BrowserSessionOptions(Uri Url, bool Headless, SudokuDifficulty? Difficulty, int ViewportWidth, int ViewportHeight, BrowserKind Browser, int TimeoutSeconds);
public sealed record ScreenshotCapture(byte[] PngBytes, BoardCoordinateMapper CoordinateMapper);
public sealed record BoardCoordinateMapper(double OriginX, double OriginY, double Width, double Height)
{
    public (double X, double Y) CellCenter(int row, int column) => (OriginX + ((column + 0.5) * Width / 9.0), OriginY + ((row + 0.5) * Height / 9.0));
}
public sealed record BoardReadinessResult(bool IsReady, ScreenshotCapture? Screenshot, string Reason, double BoardConfidence);
public sealed record CellImage(int Row, int Column, byte[] PngBytes, bool IsEmptyCandidate, double SegmentationConfidence);
public sealed record BoardDetectionResult(bool Success, IReadOnlyList<CellImage> Cells, double BoardConfidence, double GridConfidence, BoardCoordinateMapper Mapper, string? FailureReason);
public sealed record DigitRecognitionResult(int Row, int Column, int? Digit, double Confidence, bool IsEmpty, string? FailureReason);
public interface IBrowserAutomation : IAsyncDisposable
{
    Task<ScreenshotCapture> CaptureSudokuScreenshotAsync(BrowserSessionOptions options, CancellationToken cancellationToken);
    Task<BoardReadinessResult> WaitForBoardReadyAsync(CancellationToken cancellationToken);
    Task FillBoardAsync(SudokuGrid original, SudokuGrid solution, BoardCoordinateMapper mapper, CancellationToken cancellationToken);
}
public interface IBoardDetector { BoardDetectionResult Detect(byte[] screenshotPng, string? debugOutputPath, CancellationToken cancellationToken); }
public interface IDigitRecognizer : IDisposable { DigitRecognitionResult Recognize(CellImage cell, CancellationToken cancellationToken); }
public interface IDebugOutputWriter { Task WriteRecognitionAsync(string? debugOutputPath, object payload, CancellationToken cancellationToken); Task WriteTimingsAsync(string? debugOutputPath, IReadOnlyList<TimedStage> timings, CancellationToken cancellationToken); }
