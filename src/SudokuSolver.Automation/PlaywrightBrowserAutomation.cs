using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using SudokuSolver.Application;
using SudokuSolver.Domain;

namespace SudokuSolver.Automation;

public sealed class PlaywrightBrowserAutomation(IBoardDetector detector, ILogger<PlaywrightBrowserAutomation> logger) : IBrowserAutomation
{
    private IPlaywright? _playwright; private IBrowser? _browser; private IBrowserContext? _context; private IPage? _page; private BrowserSessionOptions? _options;
    public async Task<ScreenshotCapture> CaptureSudokuScreenshotAsync(BrowserSessionOptions options, CancellationToken cancellationToken)
    {
        _options = options; _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false); var browserType = options.Browser switch { BrowserKind.Firefox => _playwright.Firefox, BrowserKind.Webkit => _playwright.Webkit, _ => _playwright.Chromium };
        _browser = await browserType.LaunchAsync(new() { Headless = options.Headless }).ConfigureAwait(false);
        _context = await _browser.NewContextAsync(new() { ViewportSize = new() { Width = options.ViewportWidth, Height = options.ViewportHeight } }).ConfigureAwait(false);
        _page = await _context.NewPageAsync().ConfigureAwait(false); _page.SetDefaultTimeout(options.TimeoutSeconds * 1000);
        var url = BuildUrl(options); await _page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = options.TimeoutSeconds * 1000 }).ConfigureAwait(false); await DismissPopupsAsync(_page).ConfigureAwait(false);
        var ready = await WaitForBoardReadyAsync(cancellationToken).ConfigureAwait(false); if (!ready.IsReady || ready.Screenshot is null) throw new InvalidOperationException(ready.Reason); return ready.Screenshot;
    }
    public async Task<BoardReadinessResult> WaitForBoardReadyAsync(CancellationToken cancellationToken)
    {
        if (_page is null) return new(false, null, "Page was not initialized.", 0);
        string? previousSignature = null; BoardDetectionResult? previousDetection = null; var stable=0; var attempts=0;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(300));
        do
        {
            attempts++; cancellationToken.ThrowIfCancellationRequested(); var bytes = await _page.ScreenshotAsync(new() { FullPage = false, Type = ScreenshotType.Png }).ConfigureAwait(false); var detection = detector.Detect(bytes, null, cancellationToken); var signature = detection.Success ? $"{Math.Round(detection.Mapper.OriginX)}:{Math.Round(detection.Mapper.OriginY)}:{Math.Round(detection.Mapper.Width)}:{Math.Round(detection.Mapper.Height)}:{detection.Cells.Count(c => !c.IsEmptyCandidate)}" : detection.FailureReason;
            if (detection.Success && signature == previousSignature) stable++; else stable = 1;
            logger.LogInformation("Board readiness attempt {Attempt} Success={Success} Confidence={Confidence:0.00} Reason={Reason}", attempts, detection.Success, detection.BoardConfidence, detection.FailureReason);
            if (stable >= 2 && detection.Success) return new(true, new ScreenshotCapture(bytes, detection.Mapper), "Board detection was stable across captures.", detection.BoardConfidence);
            previousSignature = signature; previousDetection = detection;
        } while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false));
        return new(false, null, previousDetection?.FailureReason ?? "Board was not ready.", previousDetection?.BoardConfidence ?? 0);
    }
    public async Task FillBoardAsync(SudokuGrid original, SudokuGrid solution, BoardCoordinateMapper mapper, CancellationToken cancellationToken)
    {
        if (_page is null) throw new InvalidOperationException("Page was not initialized.");
        for (var r=0;r<9;r++) for (var c=0;c<9;c++) if (original[r,c] == 0) { cancellationToken.ThrowIfCancellationRequested(); var (x,y)=mapper.CellCenter(r,c); await _page.Mouse.ClickAsync((float)x,(float)y).ConfigureAwait(false); await _page.Keyboard.TypeAsync(solution[r,c].ToString(System.Globalization.CultureInfo.InvariantCulture)).ConfigureAwait(false); }
    }
    private static string BuildUrl(BrowserSessionOptions options) => options.Difficulty is null ? options.Url.ToString() : new Uri(options.Url, options.Difficulty.Value.ToString().ToLowerInvariant()).ToString();
    private static async Task DismissPopupsAsync(IPage page)
    {
        string[] selectors = ["button:has-text('Accept')", "button:has-text('I agree')", "button:has-text('Continue')", "button[aria-label='Close']"];
        foreach (var selector in selectors) { var locator = page.Locator(selector).First; if (await locator.CountAsync().ConfigureAwait(false) > 0) { try { await locator.ClickAsync(new() { Timeout = 1000 }).ConfigureAwait(false); } catch (PlaywrightException) { } } }
    }
    public async ValueTask DisposeAsync()
    { if (_context is not null) await _context.DisposeAsync().ConfigureAwait(false); if (_browser is not null) await _browser.DisposeAsync().ConfigureAwait(false); _playwright?.Dispose(); }
}
