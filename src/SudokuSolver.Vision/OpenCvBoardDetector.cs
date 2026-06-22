using Microsoft.Extensions.Options;
using OpenCvSharp;
using SudokuSolver.Application;

namespace SudokuSolver.Vision;

public sealed class OpenCvBoardDetector(IOptions<VisionOptions> options) : IBoardDetector
{
    private readonly VisionOptions _options = options.Value;
    public BoardDetectionResult Detect(byte[] screenshotPng, string? debugOutputPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DirectoryInfo? root = string.IsNullOrWhiteSpace(debugOutputPath) ? null : Directory.CreateDirectory(debugOutputPath);
        using var src = Cv2.ImDecode(screenshotPng, ImreadModes.Color); if (src.Empty()) return Fail("Screenshot could not be decoded.");
        Save(root, "000-original-screenshot.png", src);
        using var gray = new Mat(); Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY); Save(root, "010-grayscale.png", gray);
        using var blur = new Mat(); Cv2.GaussianBlur(gray, blur, new Size(5,5), 0);
        using var threshold = new Mat(); Cv2.AdaptiveThreshold(blur, threshold, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 15, 2); Save(root, "020-threshold.png", threshold);
        Cv2.FindContours(threshold, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        var best = FindBest(src.Size(), contours);
        if (best.Points.Length != 4) return Fail("No quadrilateral Sudoku candidate was found.");
        using var candidate = src.Clone(); Cv2.Polylines(candidate, [best.Points], true, Scalar.Lime, 3); Save(root, "040-board-candidate.png", candidate);
        var ordered = Order(best.Points); var size = _options.NormalizedBoardSize;
        using var perspective = new Mat(); using var transform = Cv2.GetPerspectiveTransform(ordered, [new Point2f(0,0), new Point2f(size-1,0), new Point2f(size-1,size-1), new Point2f(0,size-1)]); Cv2.WarpPerspective(src, perspective, transform, new Size(size,size)); Save(root, "050-perspective-board.png", perspective);
        var gridConfidence = ScoreGrid(perspective); if (gridConfidence < _options.MinGridConfidence) return Fail($"Grid confidence {gridConfidence:0.00} is below {_options.MinGridConfidence:0.00}.");
        using var overlay = perspective.Clone(); DrawGrid(overlay); Save(root, "060-grid-overlay.png", overlay);
        var cells = ExtractCells(perspective, root, cancellationToken);
        var rect = Cv2.BoundingRect(best.Points); var mapper = new BoardCoordinateMapper(rect.X, rect.Y, rect.Width, rect.Height);
        return new(true, cells, best.Score, gridConfidence, mapper, null);
        BoardDetectionResult Fail(string reason) => new(false, [], 0, 0, new BoardCoordinateMapper(0,0,0,0), reason);
    }
    private Candidate FindBest(Size imageSize, Point[][] contours)
    {
        var imageArea = imageSize.Width * imageSize.Height; Candidate best = new([], 0);
        foreach (var contour in contours)
        {
            var area = Math.Abs(Cv2.ContourArea(contour)); if (area < imageArea * _options.MinContourAreaRatio) continue;
            var peri = Cv2.ArcLength(contour, true); var approx = Cv2.ApproxPolyDP(contour, 0.02 * peri, true); if (approx.Length != 4) continue;
            var rect = Cv2.BoundingRect(approx); var aspect = rect.Width / (double)Math.Max(1, rect.Height); var aspectScore = 1.0 - Math.Min(1.0, Math.Abs(1.0 - aspect)); var areaScore = Math.Min(1.0, area / (imageArea * 0.35)); var score = Math.Clamp((aspectScore * 0.45) + (areaScore * 0.55), 0, 1);
            if (score > best.Score) best = new(approx, score);
        }
        return best;
    }
    private IReadOnlyList<CellImage> ExtractCells(Mat board, DirectoryInfo? root, CancellationToken ct)
    {
        var cells = new List<CellImage>(81); var cellSize = board.Width / 9; var inset = Math.Max(2, cellSize * _options.CellInnerCropPercent / 100); var cellDir = root is null ? null : Directory.CreateDirectory(Path.Combine(root.FullName,"cells")); var digitDir = root is null ? null : Directory.CreateDirectory(Path.Combine(root.FullName,"digits"));
        for (var r=0;r<9;r++) for (var c=0;c<9;c++) { ct.ThrowIfCancellationRequested(); var rect = new Rect(c*cellSize, r*cellSize, cellSize, cellSize); using var original = new Mat(board, rect); Save(cellDir,$"r{r+1}c{c+1}-original.png", original); var innerRect = new Rect(inset,inset,cellSize-(2*inset),cellSize-(2*inset)); using var inner = new Mat(original, innerRect); Save(cellDir,$"r{r+1}c{c+1}-inner.png", inner); using var mask = SegmentDigit(inner, out var empty, out var conf); Save(cellDir,$"r{r+1}c{c+1}-mask.png", mask); byte[] png = empty ? [] : mask.ToBytes(".png"); if (!empty) Save(digitDir,$"r{r+1}c{c+1}-digit.png", mask); cells.Add(new CellImage(r,c,png,empty,conf)); }
        return cells;
    }
    private static Mat SegmentDigit(Mat inner, out bool empty, out double confidence)
    {
        using var gray = new Mat(); Cv2.CvtColor(inner, gray, ColorConversionCodes.BGR2GRAY); using var binary = new Mat(); Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu); Cv2.FindContours(binary, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        Rect? box = null; var areaSum=0.0; foreach (var contour in contours) { var area=Cv2.ContourArea(contour); if (area < inner.Width*inner.Height*0.015) continue; var r=Cv2.BoundingRect(contour); if (r.Height < inner.Height*0.25 || r.Width > inner.Width*0.85) continue; areaSum += area; box = box is null ? r : Union(box.Value, r); }
        empty = box is null || areaSum < inner.Width*inner.Height*0.025; confidence = empty ? 1 : Math.Clamp(areaSum/(inner.Width*inner.Height*0.20),0,1); if (empty) return Mat.Zeros(inner.Size(), MatType.CV_8UC1);
        var padded = Pad(box.Value, inner.Width, inner.Height, 4); using var crop = new Mat(binary, padded); var output = new Mat(); Cv2.Resize(crop, output, new Size(48,48), 0,0, InterpolationFlags.Area); return output;
    }
    private static Rect Union(Rect a, Rect b) { var x=Math.Min(a.X,b.X); var y=Math.Min(a.Y,b.Y); var right=Math.Max(a.Right,b.Right); var bottom=Math.Max(a.Bottom,b.Bottom); return new Rect(x,y,right-x,bottom-y); }
    private static Rect Pad(Rect r, int w, int h, int p) { var x=Math.Max(0,r.X-p); var y=Math.Max(0,r.Y-p); var right=Math.Min(w,r.Right+p); var bottom=Math.Min(h,r.Bottom+p); return new Rect(x,y,right-x,bottom-y); }
    private static double ScoreGrid(Mat board) { using var gray=new Mat(); Cv2.CvtColor(board,gray,ColorConversionCodes.BGR2GRAY); using var edges=new Mat(); Cv2.Canny(gray,edges,50,150); var samples=0; var hits=0; for (var i=1;i<9;i++){ var p=i*board.Width/9; hits += Cv2.CountNonZero(edges.Col(p)); hits += Cv2.CountNonZero(edges.Row(p)); samples += board.Width + board.Height; } return Math.Clamp(hits/(samples*0.18),0,1); }
    private static void DrawGrid(Mat image) { var cell=image.Width/9; for (var i=1;i<9;i++) { Cv2.Line(image,new Point(i*cell,0),new Point(i*cell,image.Height),Scalar.Lime, i%3==0?3:1); Cv2.Line(image,new Point(0,i*cell),new Point(image.Width,i*cell),Scalar.Lime, i%3==0?3:1); } }
    private static Point2f[] Order(Point[] points) { var ordered = new Point2f[4]; var sorted = points.OrderBy(p => p.X + p.Y).ToArray(); ordered[0]=sorted[0]; ordered[2]=sorted[3]; ordered[1]=points.OrderBy(p => p.Y - p.X).First(); ordered[3]=points.OrderByDescending(p => p.Y - p.X).First(); return ordered; }
    private static void Save(DirectoryInfo? dir, string file, Mat mat) { if (dir is not null) Cv2.ImWrite(Path.Combine(dir.FullName,file), mat); }
    private readonly record struct Candidate(Point[] Points, double Score);
}
