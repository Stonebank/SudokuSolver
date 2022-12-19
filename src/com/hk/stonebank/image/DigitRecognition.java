package com.hk.stonebank.image;

import com.hk.stonebank.board.SudokuBoard;
import com.hk.stonebank.settings.Settings;
import net.sourceforge.tess4j.ITessAPI;
import net.sourceforge.tess4j.Tesseract;
import net.sourceforge.tess4j.TesseractException;

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
                if (result.isBlank() || result.isEmpty())
                    result = "0";
                if (result.length() > 1)
                    result = result.substring(0, 1);
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
        if (!getBoard().canSolve()) {
            System.err.println("Not solvable. The OCR may be incorrect.");
            return;
        }
        System.out.println("OCR successful! Initiating AutoTyper...");
    }

    public SudokuBoard getBoard() {
        return new SudokuBoard(board);
    }

}
