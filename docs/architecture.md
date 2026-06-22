# Architecture

The solution uses clean dependency direction: CLI -> Application -> Domain plus infrastructure implementations in Vision, OCR, and Automation. Domain has no OpenCV, Tesseract, Playwright, file-system, logging, or DI dependencies, so the solver is deterministic and benchmarkable.

The console app uses the .NET Generic Host because this is a command-line automation workflow that benefits from DI, configuration, structured logging, and graceful shutdown without exposing HTTP endpoints. Application orchestration coordinates browser capture, board detection, OCR, validation, solving, optional filling, debug output, and timings.

Vision owns OpenCV `Mat` lifetimes and emits disposable debug artifacts. OCR owns Tesseract engine lifetime and returns confidence-bearing digit results. Automation owns Playwright browser/context/page lifetime and readiness polling; it captures screenshots repeatedly with `PeriodicTimer` until board geometry and givens are stable across consecutive captures.

Performance decisions include bit masks for candidate tracking, MRV cell selection, arrays for 81-cell hot paths, no LINQ in recursive solving, isolated OCR timing, and BenchmarkDotNet coverage for solver workloads.
