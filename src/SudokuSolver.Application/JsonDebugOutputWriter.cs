using System.Text.Json;

namespace SudokuSolver.Application;

public sealed class JsonDebugOutputWriter : IDebugOutputWriter
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public Task WriteRecognitionAsync(string? debugOutputPath, object payload, CancellationToken cancellationToken) => WriteAsync(debugOutputPath, "recognition.json", payload, cancellationToken);
    public Task WriteTimingsAsync(string? debugOutputPath, IReadOnlyList<TimedStage> timings, CancellationToken cancellationToken) => WriteAsync(debugOutputPath, "timings.json", timings, cancellationToken);
    private static async Task WriteAsync(string? path, string file, object payload, CancellationToken ct)
    { if (string.IsNullOrWhiteSpace(path)) return; Directory.CreateDirectory(path); await using var stream = File.Create(Path.Combine(path, file)); await JsonSerializer.SerializeAsync(stream, payload, Options, ct).ConfigureAwait(false); }
}
