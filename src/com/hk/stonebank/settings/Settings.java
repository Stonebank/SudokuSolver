package com.hk.stonebank.settings;

import java.io.File;

public class Settings {

    public static final boolean DEBUG_MODE = true;

    public static final int SUDOKU_BOARD_SIZE = 9;

    public static final File BOARD_IMAGE = new File("./resources/board/board.png");
    public static final File BOARD_IMAGE_OUTPUT = new File("./resources/board/board_output.png");
    public static final File BOARD_IMAGE_OUTPUT_CROPPED = new File("./resources/board/board_output_cropped.png");
    public static final File BOARD_CELL_IMAGE_OUTPUT = new File("./resources/board/cells/");

    public static final File TESSERACT_TRAINED_DATA = new File("./resources/tesseract/model/digits");
    public static final String TESSERACT_DPI = "300";

}
