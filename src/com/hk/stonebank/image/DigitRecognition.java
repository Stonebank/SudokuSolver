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
        for (File cell : Objects.requireNonNull(directory.listFiles())) {
            if (cell == null)
                continue;
            if (!cell.getName().endsWith(".png")) {
                System.err.println(cell.getName() + " is not a png image file, skipped.");
                continue;
            }
        }
    }

}
