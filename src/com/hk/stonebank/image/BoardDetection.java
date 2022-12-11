package com.hk.stonebank.image;

import com.hk.stonebank.settings.Settings;
import org.opencv.core.*;
import org.opencv.imgcodecs.Imgcodecs;
import org.opencv.imgproc.Imgproc;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

public class BoardDetection {

    private final File image;

    public BoardDetection(File image) {
        this.image = image;
        if (!image.exists()) {
            throw new IllegalArgumentException("Image does not exist");
        }
        if (!image.getName().endsWith(".png")) {
            throw new IllegalArgumentException("Image is not a png");
        }
    }

    public void detect() {

        System.out.println("Detecting board...");

        var image = Imgcodecs.imread(this.image.getAbsolutePath());

        var gray = new Mat();
        Imgproc.cvtColor(image, gray, Imgproc.COLOR_BGR2GRAY);

        var blur = new Mat();
        Imgproc.blur(gray, blur, new Size(5, 5));

        var edges = new Mat();
        Imgproc.Canny(blur, edges, 75, 200);

        List<MatOfPoint> contours = new ArrayList<>();
        Imgproc.findContours(edges, contours, new Mat(), Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

        MatOfPoint largestContour = null;
        double largestArea = 0;
        for (MatOfPoint contour : contours) {
            double area = Imgproc.contourArea(contour);
            if (area > largestArea) {
                largestArea = area;
                largestContour = contour;
            }
        }

        if (largestContour == null || largestContour.empty()) {
            throw new IllegalStateException("No board found");
        }

        var output = new Mat();

        var boundingRect = Imgproc.boundingRect(largestContour);

        int margin = 10;

        var adjustedX = boundingRect.x + margin;
        var adjustedY = boundingRect.y + margin;

        var adjustedWidth = boundingRect.width - 2 * margin;
        var adjustedHeight = boundingRect.height - 2 * margin;

        var adjustedBoundingRect = new Rect(adjustedX, adjustedY, adjustedWidth, adjustedHeight);

        output = new Mat(image, adjustedBoundingRect);

        Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT.getAbsolutePath(), output);
        System.out.println("Board detected, output is located at " + Settings.BOARD_IMAGE_OUTPUT.getAbsolutePath());

        detectAndCropCells(output);

    }

    public void detectAndCropCells(Mat output) {

        if (output == null || output.empty()) {
            throw new IllegalArgumentException("Output is null or empty");
        }

        System.out.println("Detecting cells...");

        int cellWidth = output.cols() / Settings.SUDOKU_BOARD_SIZE;
        int cellHeight = output.rows() / Settings.SUDOKU_BOARD_SIZE;

// Calculate the margin and padding values for each cell
        int cellMarginX = cellWidth / Settings.SUDOKU_BOARD_SIZE;
        int cellPaddingX = cellMarginX / 2;
        int cellMarginY = cellHeight / Settings.SUDOKU_BOARD_SIZE;
        int cellPaddingY = cellMarginY / 2;

        for (int x = 0; x < Settings.SUDOKU_BOARD_SIZE; x++) {
            for (int y = 0; y < Settings.SUDOKU_BOARD_SIZE; y++) {

                // Adjust the position of the cell to center the digit within it
                var cellRect = new Rect(
                        x * cellWidth + cellMarginX + cellPaddingX,
                        y * cellHeight + cellMarginY + cellPaddingY,
                        cellWidth - cellMarginX - cellPaddingX,
                        cellHeight - cellMarginY - cellPaddingY
                );

                var cell = new Mat(output, cellRect);

                String fileName = String.format("cell_%d_%d.png", x, y);
                Imgcodecs.imwrite(Settings.BOARD_CELL_IMAGE_OUTPUT + "/" + fileName, cell);

            }

        }
    }
}
