# Troubleshooting

## Board detection failures

Enable `--debug-output ./debug` and inspect `000-original-screenshot.png`, `020-threshold.png`, `040-board-candidate.png`, and `060-grid-overlay.png`. Increase the viewport, try headed mode, or lower `--board-confidence-threshold` only after confirming the candidate is correct.

## OCR mistakes

Inspect `cells/*-mask.png`, `digits/*-digit.png`, and `recognition.json`. Grid lines recognized as `1` or `7` usually mean the inner crop is too small or the page theme has unusually thick lines. Adjust `Vision:CellInnerCropPercent` or use a custom digit classifier.

## Tesseract setup

Install Tesseract trained data locally and set `Ocr:TessDataPath`. The recognizer uses digit whitelist `123456789` and single-character page segmentation.

## Playwright setup

Run the Playwright install command after building the CLI. Normal unit tests do not require live network or browsers.

## sudoku.com UI changes

The tool does not rely on fixed board coordinates, but cookie banners, overlays, or major page redesigns can hide the board. Use headed mode and debug screenshots to confirm visibility.

## Benchmark caveats

Solver benchmarks are deterministic. Vision/OCR/full-pipeline benchmarks depend on sample images and native library availability; compare results only on the same hardware and runtime configuration.
