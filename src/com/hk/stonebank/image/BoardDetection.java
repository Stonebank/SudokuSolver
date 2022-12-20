package com.hk.stonebank.image;

import com.hk.stonebank.exception.BoardNotFoundException;
import com.hk.stonebank.settings.Settings;
import org.opencv.core.Point;
import org.opencv.core.*;
import org.opencv.imgcodecs.Imgcodecs;
import org.opencv.imgproc.Imgproc;

import javax.imageio.ImageIO;
import java.awt.*;
import java.io.File;
import java.io.IOException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

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
        if (Settings.BOARD_CELL_IMAGE_OUTPUT.exists()) {
            for (File cell : Objects.requireNonNull(Settings.BOARD_CELL_IMAGE_OUTPUT.listFiles())) {
                if (cell == null)
                    continue;
                if (!cell.delete()) {
                    System.err.println("Failed to delete " + cell.getName());
                }
            }
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
            throw new BoardNotFoundException();
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

        Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath(), output);
        System.out.println("Board detected, output is located at " + Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath());

        if (Settings.DEBUG_MODE) {
            var board_contours = drawContoursAroundBoard(image, largestContour);
            var board_lines = drawLinesAroundBoard(image, largestContour);
            Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT.getAbsolutePath(), board_contours);
            Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT.getAbsolutePath(), board_lines);
            System.out.println("Debug image with rectangle is located at " + Settings.BOARD_IMAGE_OUTPUT.getAbsolutePath());
        }

        processCroppedBoard();
        detectAndCropCells();

    }

    public void processCroppedBoard() {

        if (!Settings.BOARD_CELL_IMAGE_OUTPUT.exists()) {
            throw new IllegalStateException("Board image output does not exist");
        }

        System.out.println("Performing additional image processing techniques on cropped board...");

        var image = Imgcodecs.imread(Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath());

        Mat grayImage = new Mat();
        Imgproc.cvtColor(image, grayImage, Imgproc.COLOR_BGR2GRAY);
        Imgproc.GaussianBlur(grayImage, grayImage, new Size(5, 5), 0);

        Mat thresholdImage = new Mat();
        Imgproc.adaptiveThreshold(grayImage, thresholdImage, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY, 15, 30);

        Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath(), thresholdImage);

    }

    public void detectAndCropCells() {

        var output = Imgcodecs.imread(Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath());

        if (output.empty()) {
            throw new IllegalStateException("Board image output is empty");
        }

        System.out.println("Detecting cells...");

        int cellWidth = output.cols() / Settings.SUDOKU_BOARD_SIZE;
        int cellHeight = output.rows() / Settings.SUDOKU_BOARD_SIZE;

        for (int y = 0; y < Settings.SUDOKU_BOARD_SIZE; y++) {
            for (int x = 0; x < Settings.SUDOKU_BOARD_SIZE; x++) {

                var cellRect = new Rect(y * cellWidth, x * cellHeight, cellWidth, cellHeight);
                var cell = new Mat(output, cellRect);

                var gray = new Mat();
                Imgproc.cvtColor(cell, gray, Imgproc.COLOR_BGR2GRAY);

                Imgproc.threshold(gray, gray, 128, 255, Imgproc.THRESH_BINARY);

                List<MatOfPoint> contours = new ArrayList<>();
                Imgproc.findContours(gray, contours, new Mat(), Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

                if (contours.isEmpty()) {
                    throw new RuntimeException("No contours found");
                }

                MatOfPoint digitContour = null;
                for (MatOfPoint contour : contours) {
                    if (Imgproc.contourArea(contour) > 0) {
                        digitContour = contour;
                        break;
                    }
                }

                if (digitContour == null) {
                    throw new RuntimeException("No digit contour found");
                }

                var boundingRect = Imgproc.boundingRect(digitContour);
                var result = new Mat(cell, boundingRect);

                var fileName = String.format("cell_%d_%d.png", x, y);
                Imgcodecs.imwrite(Settings.BOARD_CELL_IMAGE_OUTPUT + "/" + fileName, result);

            }
        }

    }

    private Mat drawContoursAroundBoard(Mat image, MatOfPoint largestContour) {
        var rect = Imgproc.boundingRect(largestContour);
        Imgproc.rectangle(image, rect.tl(), rect.br(), new Scalar(0, 255, 0), 3);
        return image;
    }

    private Mat drawLinesAroundBoard(Mat image, MatOfPoint largestContour) {
        var rect = Imgproc.boundingRect(largestContour);
        int x = rect.x, y = rect.y, width = rect.width, height = rect.height;

        int cellWidth = width / 9;
        int cellHeight = height / 9;

        for (int i = 1; i < 9; i++) {
            Point p1 = new Point(x + i * cellWidth, y);
            Point p2 = new Point(x + i * cellWidth, y + height);
            Imgproc.line(image, p1, p2, new Scalar(0, 255, 0), 3);
        }

        for (int i = 1; i < 9; i++) {
            Point p1 = new Point(x, y + i * cellHeight);
            Point p2 = new Point(x + width, y + i * cellHeight);
            Imgproc.line(image, p1, p2, new Scalar(0, 255, 0), 3);
        }
        return image;
    }

    public void takeScreenshot() {

        System.out.println("Attempting to take a screenshot in " + Settings.SCREENSHOT_DELAY + " milliseconds...");

        try {

            Thread.sleep(Settings.SCREENSHOT_DELAY);

            var screenRect = new Rectangle(Toolkit.getDefaultToolkit().getScreenSize());
            ImageIO.write(new Robot().createScreenCapture(screenRect), "png", Settings.BOARD_IMAGE);

            System.out.println("Screenshot successfully taken");

        } catch (InterruptedException | AWTException | IOException e) {
            System.err.println("Screenshot was not taken.");
            e.printStackTrace();
        }

    }

    public void openBrowser(String url) {
        try {

            String os_name = System.getProperty("os.name").toLowerCase();

            if (canOpenBrowser()) {
                Desktop.getDesktop().browse(new URI(url));
                return;
            }

            if (os_name.startsWith("windows")) {
                Runtime.getRuntime().exec("rundll32 url.dll,FileProtocolHandler " + url);
                return;
            }

            if (os_name.startsWith("mac")) {
                Runtime.getRuntime().exec( new String[] {"open", url});
                return;
            }

            if (os_name.startsWith("linux")) {
                Runtime.getRuntime().exec( new String[] {"xdg-open", url});
                return;
            }

            System.err.println("Could not open browser");

        } catch (IOException | URISyntaxException e) {
            System.err.println("Error! Operating system not supported.");
            e.printStackTrace();
        }
    }

    private boolean canOpenBrowser() {
        return Desktop.isDesktopSupported() && Desktop.getDesktop().isSupported(Desktop.Action.BROWSE);
    }

}
