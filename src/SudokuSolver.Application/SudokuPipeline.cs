using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SudokuSolver.Domain;

namespace SudokuSolver.Application;

public sealed class SudokuPipeline(IBrowserAutomation browser, IBoardDetector boardDetector, IDigitRecognizer recognizer, ISudokuSolver solver, IDebugOutputWriter debugWriter, ILogger<SudokuPipeline> logger)
{
    public async Task<PipelineResult> SolveAsync(SolveCommandOptions options, CancellationToken cancellationToken)
    {
        List<TimedStage> timings = [];
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds)); var ct = timeout.Token;
        try
        {
            var total = Stopwatch.StartNew();
            DirectoryInfo? debugDir = string.IsNullOrWhiteSpace(options.DebugOutputPath) ? null : Directory.CreateDirectory(options.DebugOutputPath);
            logger.LogInformation("Solving Sudoku Url={Url} Headless={Headless} Fill={Fill} DebugOutput={DebugOutput}", options.Url, options.Headless, options.Fill, debugDir?.FullName);
            var shot = await MeasureAsync("BrowserReadiness", timings, () => browser.CaptureSudokuScreenshotAsync(new(options.Url, options.Headless, options.Difficulty, options.ViewportWidth, options.ViewportHeight, options.Browser, options.TimeoutSeconds), ct)).ConfigureAwait(false);
            var detection = Measure("BoardDetection", timings, () => boardDetector.Detect(shot.PngBytes, options.DebugOutputPath, ct), new Dictionary<string,string>());
            if (!detection.Success || detection.BoardConfidence < options.BoardConfidenceThreshold) return await FinishAsync(PipelineExitCode.BoardDetectionFailed, null, null, detection.FailureReason ?? "Board detection confidence was too low.", timings, options, ct).ConfigureAwait(false);
            var recognition = Measure("Ocr", timings, () => Recognize(detection.Cells, options.OcrConfidenceThreshold, ct), []);
            if (recognition.Failure is not null) return await FinishAsync(PipelineExitCode.OcrConfidenceTooLow, null, null, recognition.Failure, timings, options, ct).ConfigureAwait(false);
            var puzzle = new SudokuGrid(recognition.Values);
            var validation = Measure("PuzzleValidation", timings, () => PuzzleValidator.Validate(puzzle), []);
            if (validation is not null) return await FinishAsync(PipelineExitCode.RecognizedPuzzleInvalid, puzzle, null, validation, timings, options, ct).ConfigureAwait(false);
            var solveResult = Measure("Solver", timings, () => solver.Solve(puzzle, new SudokuSolverOptions(RequireUniqueSolution: false), ct), []);
            if (solveResult.Status == SudokuSolveStatus.NoSolution) return await FinishAsync(PipelineExitCode.PuzzleHasNoSolution, puzzle, null, solveResult.FailureReason ?? "No solution.", timings, options, ct).ConfigureAwait(false);
            if (!solveResult.IsSolved || solveResult.Solution is null) return await FinishAsync(PipelineExitCode.UnexpectedFailure, puzzle, null, solveResult.FailureReason ?? solveResult.Status.ToString(), timings, options, ct).ConfigureAwait(false);
            if (options.Fill) await MeasureAsync("BoardFilling", timings, () => browser.FillBoardAsync(puzzle, solveResult.Solution, detection.Mapper, ct)).ConfigureAwait(false);
            total.Stop(); timings.Add(new("Total", total.Elapsed.TotalMilliseconds, new Dictionary<string,string>()));
            return await FinishAsync(PipelineExitCode.Success, puzzle, solveResult.Solution, "Solved successfully.", timings, options, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return new(PipelineExitCode.TimeoutOrCancellation, null, null, "Operation timed out or was cancelled.", timings); }
        catch (Exception ex) { logger.LogError(ex, "Pipeline failed unexpectedly"); return new(PipelineExitCode.UnexpectedFailure, null, null, ex.Message, timings); }
    }
    private async Task<PipelineResult> FinishAsync(PipelineExitCode code, SudokuGrid? puzzle, SudokuGrid? solution, string message, IReadOnlyList<TimedStage> timings, SolveCommandOptions options, CancellationToken ct)
    { await debugWriter.WriteRecognitionAsync(options.DebugOutputPath, new { message, puzzle = puzzle?.ToString(), solution = solution?.ToString() }, ct).ConfigureAwait(false); await debugWriter.WriteTimingsAsync(options.DebugOutputPath, timings, ct).ConfigureAwait(false); return new(code,puzzle,solution,message,timings); }
    private (int[] Values,string? Failure) Recognize(IReadOnlyList<CellImage> cells, double threshold, CancellationToken ct)
    { var values = new int[81]; foreach (var cell in cells) { var r = recognizer.Recognize(cell, ct); if (!r.IsEmpty && (!r.Digit.HasValue || r.Confidence < threshold)) return (values, $"OCR failed at r{cell.Row+1}c{cell.Column+1}: {r.FailureReason ?? "low confidence"} ({r.Confidence:0.00})."); values[cell.Row*9+cell.Column] = r.IsEmpty ? 0 : r.Digit!.Value; } return (values,null); }
    private static T Measure<T>(string stage, List<TimedStage> timings, Func<T> action, IReadOnlyDictionary<string,string> props) { var sw=Stopwatch.StartNew(); var result=action(); sw.Stop(); timings.Add(new(stage, sw.Elapsed.TotalMilliseconds, props)); return result; }
    private static async Task<T> MeasureAsync<T>(string stage, List<TimedStage> timings, Func<Task<T>> action) { var sw=Stopwatch.StartNew(); var result=await action().ConfigureAwait(false); sw.Stop(); timings.Add(new(stage, sw.Elapsed.TotalMilliseconds, new Dictionary<string,string>())); return result; }
    private static async Task MeasureAsync(string stage, List<TimedStage> timings, Func<Task> action) { var sw=Stopwatch.StartNew(); await action().ConfigureAwait(false); sw.Stop(); timings.Add(new(stage, sw.Elapsed.TotalMilliseconds, new Dictionary<string,string>())); }
}
