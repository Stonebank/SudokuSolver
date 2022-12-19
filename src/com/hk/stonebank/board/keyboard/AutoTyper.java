package com.hk.stonebank.board.keyboard;

import com.hk.stonebank.board.SudokuBoard;
import com.hk.stonebank.settings.Settings;

import java.awt.*;
import java.awt.event.KeyEvent;

public class AutoTyper {

    private final Robot robot;

    private final SudokuBoard board;

    private final int[][] keys = new int[Settings.SUDOKU_BOARD_SIZE][Settings.SUDOKU_BOARD_SIZE];

    public AutoTyper(SudokuBoard board) throws AWTException {
        this.robot = new Robot();
        this.board = board;
    }

    public void start() {

        if (!board.canSolve()) {
            System.err.println("This sudoku board cannot be solved.");
            return;
        }

        convertKeys();

        for (int i = 0; i < keys.length; i++) {
            for (int j = 0; j < keys.length; j++) {
                if (i > 0 && j % 9 == 0) {
                    pressKey(Settings.DOWN_KEY);
                    for (int k = 1; k <= 9; k++)
                        pressKey(Settings.LEFT_KEY);
                }
                if (j != 0)
                    pressKey(Settings.RIGHT_KEY);
                pressKey(keys[i][j]);
            }
        }

        board.displayBoard();

    }

    private void convertKeys() {
        for (int i = 0; i < board.getBoard().length; i++) {
            for (int j = 0; j < board.getBoard().length; j++) {
                try {
                    keys[i][j] = KeyEvent.class.getField("VK_" + board.getBoard()[i][j]).getInt(null);
                } catch (NoSuchFieldException | IllegalAccessException e) {
                    System.err.println("Fault converting " + board.getBoard()[i][j] + ".");
                    e.printStackTrace();
                }
            }
        }
        System.out.println("Board digits has been converted to keys");
    }

    private void pressKey(int key) {
        robot.keyPress(key);
        robot.keyRelease(key);
        robot.delay(Settings.TYPE_DELAY);
        if (Settings.DEBUG_MODE)
            System.out.println("AutoTyper pressed key: " + KeyEvent.getKeyText(key));
    }

}
