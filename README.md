# SudokuSolver .NET Rewrite

This repository rewrites the original Java/OpenCV/Tesseract Sudoku automation tool as a modern .NET 10 console solution. The CLI opens a browser with Playwright, captures a Sudoku board, detects the grid with OpenCV, recognizes givens with Tesseract, solves with a bit-mask MRV solver, and optionally fills the page.

## Original Java design summary

The Java version used `Desktop`/`Robot` to open and screenshot the browser, OpenCV Canny/largest-contour board cropping, Tess4J OCR over cropped cells, a recursive backtracking solver with MRV, and `Robot` keyboard typing. Its README notes that the browser must remain focused and first-visit popups must be removed manually. The rewrite removes those assumptions by using Playwright screenshots/clicks, readiness polling, structured debug output, and explicit confidence thresholds.

## Architecture

- `SudokuSolver.Domain`: pure grid validation and bit-mask MRV solving.
- `SudokuSolver.Application`: pipeline orchestration, result mapping, timing, debug JSON.
- `SudokuSolver.Vision`: OpenCV board detection, perspective normalization, cell extraction, digit segmentation.
- `SudokuSolver.Ocr`: Tesseract digit-only recognition behind `IDigitRecognizer`.
- `SudokuSolver.Automation`: Playwright lifecycle, readiness checks, screenshots, filling.
- `SudokuSolver.Cli`: console app using `Host.CreateApplicationBuilder(args)`, DI, logging, options, and cancellation.

## Prerequisites

- .NET 10 SDK (`global.json` targets `10.0.100` with roll-forward).
- Playwright browsers: `pwsh src/SudokuSolver.Cli/bin/Debug/net10.0/playwright.ps1 install` after the first build.
- Tesseract trained data in `./tessdata` or configure `Ocr:TessDataPath`.
- OpenCV native runtime. The project references the Windows runtime package; Linux/macOS deployments should switch to the matching OpenCvSharp runtime package.

## Commands

```bash
dotnet build
dotnet test
dotnet run --project src/SudokuSolver.Cli -- solve --url https://www.sudoku.com --headed --debug-output ./debug --fill
dotnet run --project src/SudokuSolver.Cli -- solve --url https://www.sudoku.com --headless --no-fill
dotnet run --project benchmarks/SudokuSolver.Benchmarks -c Release
```

## CLI options

`solve` supports `--url`, `--headed`, `--headless`, `--difficulty easy|medium|hard|expert|evil`, `--debug-output`, `--timeout-seconds`, `--ocr-confidence-threshold`, `--board-confidence-threshold`, `--fill`, `--no-fill`, `--viewport-width`, `--viewport-height`, and `--browser chromium|firefox|webkit`.

## Debug output

When `--debug-output ./debug` is provided, the vision pipeline writes original screenshots, grayscale/threshold images, candidate overlays, normalized board images, grid overlays, per-cell crops/masks, digit crops, `recognition.json`, and `timings.json`.

## Exit codes

`0` success, `1` unexpected failure, `2` board detection failed, `3` OCR confidence too low, `4` recognized puzzle invalid, `5` no solution, `6` browser automation failed, `7` timeout/cancellation.

## Known limitations and extension path

Tesseract remains sensitive to font/theme changes; the OCR boundary is intentionally narrow so an ONNX digit classifier or template matcher can replace it. Live sudoku.com integration can break if the site markup or interaction model changes, but board detection uses screenshots rather than fixed DOM coordinates.
