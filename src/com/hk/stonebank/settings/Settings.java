package com.hk.stonebank.settings;

import com.hk.stonebank.board.mode.GameMode;

import java.io.File;

public class Settings {

    public static GameMode GAME_MODE = null;

    public static final boolean DEBUG_MODE = false;

    public static final String SUDOKU_URL = "https://www.sudoku.com";

    public static final int SUDOKU_BOARD_SIZE = 9;

    public static final File BOARD_IMAGE = new File("./resources/board/board.png");
    public static final File BOARD_IMAGE_OUTPUT = new File("./resources/board/board_output.png");
    public static final File BOARD_IMAGE_OUTPUT_CROPPED = new File("./resources/board/board_output_cropped.png");
    public static final File BOARD_IMAGE_OUTPUT_SOLUTION = new File("./resources/board/board_output_solution.png");
    public static final File BOARD_CELL_IMAGE_OUTPUT = new File("./resources/board/cells/");

    public static final File PERFORMANCE_GRAPH_OUTPUT = new File("./resources/performance_graph.png");

    public static final File TESSERACT_TRAINED_DATA = new File("./resources/tesseract/model/digits");
    public static final String TESSERACT_DPI = "300";

    public static final int TYPE_DELAY = 0;
    public static final int LEFT_KEY = 37;
    public static final int RIGHT_KEY = 39;
    public static final int DOWN_KEY = 40;

    public static final long SCREENSHOT_DELAY = 3500;

}
