using Microsoft.Extensions.Options;
using SudokuSolver.Application;
using Tesseract;

namespace SudokuSolver.Ocr;

public sealed class TesseractDigitRecognizer : IDigitRecognizer
{
    private readonly OcrOptions _options;
    private readonly TesseractEngine? _engine;
    public TesseractDigitRecognizer(IOptions<OcrOptions> options)
    {
        _options = options.Value;
        if (Directory.Exists(_options.TessDataPath))
        {
            _engine = new TesseractEngine(_options.TessDataPath, _options.Language, EngineMode.LstmOnly);
            _engine.SetVariable("tessedit_char_whitelist", "123456789");
            _engine.SetVariable("classify_bln_numeric_mode", "1");
            _engine.DefaultPageSegMode = PageSegMode.SingleChar;
        }
    }
    public DigitRecognitionResult Recognize(CellImage cell, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (cell.IsEmptyCandidate || cell.PngBytes.Length == 0) return new(cell.Row, cell.Column, null, cell.SegmentationConfidence, true, null);
        if (_engine is null) return new(cell.Row, cell.Column, null, 0, false, $"Tesseract data path '{_options.TessDataPath}' was not found.");
        using var pix = Pix.LoadFromMemory(cell.PngBytes); using var page = _engine.Process(pix, PageSegMode.SingleChar);
        var text = page.GetText().Trim(); var confidence = Math.Clamp(page.GetMeanConfidence(), 0, 1);
        var digit = ExtractDigit(text);
        if (digit is null) return new(cell.Row, cell.Column, null, confidence, false, $"No digit recognized from '{text}'.");
        if (confidence < _options.ConfidenceThreshold) return new(cell.Row, cell.Column, digit, confidence, false, "Recognition confidence below threshold.");
        return new(cell.Row, cell.Column, digit, confidence, false, null);
    }
    private static int? ExtractDigit(string text) { foreach (var ch in text) if (ch is >= '1' and <= '9') return ch - '0'; return null; }
    public void Dispose() => _engine?.Dispose();
}
