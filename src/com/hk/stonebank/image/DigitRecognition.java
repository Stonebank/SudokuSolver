package com.hk.stonebank.image;

import com.hk.stonebank.board.SudokuBoard;
import com.hk.stonebank.exception.UnsolvableException;
import com.hk.stonebank.notification.Notification;
import com.hk.stonebank.settings.Settings;
import net.sourceforge.tess4j.ITessAPI;
import net.sourceforge.tess4j.Tesseract;
import net.sourceforge.tess4j.TesseractException;
import org.opencv.core.Mat;
import org.opencv.core.Point;
import org.opencv.core.Scalar;
import org.opencv.imgcodecs.Imgcodecs;
import org.opencv.imgproc.Imgproc;

import java.io.File;
import java.util.Arrays;
import java.util.Objects;

public class DigitRecognition {

    private final File directory;

    private final Tesseract tesseract;

    private final int[][] board = new int[Settings.SUDOKU_BOARD_SIZE][Settings.SUDOKU_BOARD_SIZE];

    public DigitRecognition(File directory) {
        if (!directory.exists() || directory.listFiles() == null) {
            throw new IllegalArgumentException("Directory does not exist.");
        }
        if (Objects.requireNonNull(directory.listFiles()).length == 0) {
            throw new IllegalArgumentException("Directory is empty.");
        }

        this.directory = directory;

        System.out.println("Initializing tesseract...");
        this.tesseract = new Tesseract();

        System.out.println("Defining tesseract configuration...");
        this.tesseract.setPageSegMode(ITessAPI.TessPageSegMode.PSM_SINGLE_BLOCK);
        this.tesseract.setTessVariable("user_defined_dpi", Settings.TESSERACT_DPI);
        this.tesseract.setTessVariable("tessedit_char_whitelist", "0123456789");
        this.tesseract.setLanguage(Settings.TESSERACT_TRAINED_DATA.getPath());

    }

    public void doOCR() {
        int row = 0, col = 0;
        for (File cell : Objects.requireNonNull(directory.listFiles())) {
            if (cell == null)
                continue;
            if (!cell.getName().endsWith(".png")) {
                System.err.println(cell.getName() + " is not a png image file, skipped.");
                continue;
            }
            try {
                String result = tesseract.doOCR(cell).replaceAll("[^\\d\\s]", "").trim();
                if (result.isBlank())
                    result = "0";
                if (result.length() > 1)
                    result = result.substring(0, 1);
                if (Settings.DEBUG_MODE)
                    System.out.println("Cell " + row + ", " + col + " = " + result);
                board[row][col] = Integer.parseInt(result);
                col++;
                if (col > 8) {
                    row++;
                    col = 0;
                }
            } catch (TesseractException e) {
                e.printStackTrace();
            }
        }

        var board_copy = Arrays.stream(board).map(int[]::clone).toArray(int[][]::new);

        if (!getBoard().canSolve()) {
            throw new UnsolvableException();
        }

        fillBoardWithSolution(board_copy);

        System.out.println("OCR successful! Initiating AutoTyper...");

        Notification.send(new Notification("Board solution is found! Do not touch the keyboard or mouse until the AutoTyper is done."));

    }

    public void fillBoardWithSolution(int[][] copy) {
        var image = Imgcodecs.imread(Settings.BOARD_IMAGE_OUTPUT_CROPPED.getAbsolutePath());
        int cellWidth = image.cols() / Settings.SUDOKU_BOARD_SIZE;
        int cellHeight = image.rows() / Settings.SUDOKU_BOARD_SIZE;
        for (int row = 0; row < board.length; row++) {
            for (int col = 0; col < board.length; col++) {
                if (board[row][col] == 0 || board[row][col] == copy[row][col]) {
                    System.out.println("Cell " + row + ", " + col + " is the same.");
                    continue;
                }
                var cellPosition = calculateCellPosition(row, col, cellWidth, cellHeight);
                drawSolutionDigit(image, board[row][col], cellWidth, cellHeight, cellPosition);
            }
        }
        Imgcodecs.imwrite(Settings.BOARD_IMAGE_OUTPUT_SOLUTION.getAbsolutePath(), image);
    }

    private void drawSolutionDigit(Mat image, int digit, int cellWidth, int cellHeight, Point position) {
        var digitText = String.valueOf(digit);
        var textSize = Imgproc.getTextSize(digitText, Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, 1, null);
        var textPosition = new Point(position.x + (cellWidth - textSize.width) / 2, position.y + (cellHeight + textSize.height) / 2);
        Imgproc.putText(image, digitText, textPosition, Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar(114, 227, 0, 255), 2);
    }

    private Point calculateCellPosition(int row, int col, int cellWidth, int cellHeight) {
        int padding = 5;
        int x = (col * cellWidth) + padding;
        int y = (row * cellHeight) + padding;
        return new Point(x, y);
    }

    public SudokuBoard getBoard() {
        return new SudokuBoard(board);
    }

}
