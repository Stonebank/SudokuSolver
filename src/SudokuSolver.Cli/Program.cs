using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SudokuSolver.Application;
using SudokuSolver.Automation;
using SudokuSolver.Domain;
using SudokuSolver.Ocr;
using SudokuSolver.Vision;

var parsed = CliParser.Parse(args);
if (!parsed.Success || parsed.Options is null) { Console.Error.WriteLine(parsed.Message); return (int)PipelineExitCode.UnexpectedFailure; }
var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<VisionOptions>(builder.Configuration.GetSection("Vision"));
builder.Services.Configure<OcrOptions>(builder.Configuration.GetSection("Ocr"));
builder.Services.AddSingleton<ISudokuSolver, BitMaskSudokuSolver>();
builder.Services.AddSingleton<IBoardDetector, OpenCvBoardDetector>();
builder.Services.AddSingleton<IDigitRecognizer, TesseractDigitRecognizer>();
builder.Services.AddSingleton<IDebugOutputWriter, JsonDebugOutputWriter>();
builder.Services.AddSingleton<IBrowserAutomation, PlaywrightBrowserAutomation>();
builder.Services.AddSingleton<SudokuPipeline>();
await using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SudokuSolver.Cli");
try
{
    await host.StartAsync().ConfigureAwait(false);
    var pipeline = host.Services.GetRequiredService<SudokuPipeline>();
    var result = await pipeline.SolveAsync(parsed.Options, CancellationToken.None).ConfigureAwait(false);
    Console.WriteLine(result.Message);
    if (result.Solution is not null) Console.WriteLine(result.Solution);
    foreach (var t in result.Timings) logger.LogInformation("Stage={Stage} ElapsedMs={ElapsedMs:0.0}", t.Stage, t.ElapsedMs);
    await host.StopAsync().ConfigureAwait(false);
    return (int)result.ExitCode;
}
catch (Exception ex) { logger.LogError(ex, "Unexpected command failure"); return (int)PipelineExitCode.UnexpectedFailure; }

internal static class CliParser
{
    public static (bool Success, SolveCommandOptions? Options, string Message) Parse(string[] args)
    {
        if (args.Length == 0 || args[0] != "solve") return (false, null, "Usage: SudokuSolver.Cli solve --url <url> [--headed|--headless] [--fill|--no-fill]");
        var url = new Uri("https://www.sudoku.com"); var headless = true; SudokuDifficulty? difficulty = null; string? debug = null; var timeout = 60; var ocr=0.70; var board=0.70; var fill=false; var width=1440; var height=1000; var browser=BrowserKind.Chromium;
        for (var i=1;i<args.Length;i++)
        {
            string Need() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i])
            {
                case "--url": url = new Uri(Need(), UriKind.Absolute); break;
                case "--headed": headless = false; break;
                case "--headless": headless = true; break;
                case "--difficulty": difficulty = Enum.Parse<SudokuDifficulty>(Need(), true); break;
                case "--debug-output": debug = Need(); break;
                case "--timeout-seconds": timeout = int.Parse(Need(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--ocr-confidence-threshold": ocr = double.Parse(Need(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--board-confidence-threshold": board = double.Parse(Need(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--fill": fill = true; break;
                case "--no-fill": fill = false; break;
                case "--viewport-width": width = int.Parse(Need(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--viewport-height": height = int.Parse(Need(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--browser": browser = Enum.Parse<BrowserKind>(Need(), true); break;
                default: return (false, null, $"Unknown option '{args[i]}'.");
            }
        }
        return (true, new SolveCommandOptions(url, headless, difficulty, debug, timeout, ocr, board, fill, width, height, browser), "OK");
    }
}
