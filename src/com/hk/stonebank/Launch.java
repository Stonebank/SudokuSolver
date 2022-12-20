package com.hk.stonebank;

import com.hk.stonebank.board.keyboard.AutoTyper;
import com.hk.stonebank.image.BoardDetection;
import com.hk.stonebank.image.DigitRecognition;
import com.hk.stonebank.notification.Notification;
import com.hk.stonebank.settings.Settings;
import org.opencv.core.Core;

import java.awt.*;

public class Launch {

    public static void main(String[] args) throws AWTException {

        Notification.send(new Notification("Welcome to SudokuSolver! Please keep browser in focus for best results and do not touch the board as the board is detected."));

        long start = System.currentTimeMillis();

        System.loadLibrary(Core.NATIVE_LIBRARY_NAME);

        // Board detection. This will attempt to detect the sudoku board in the image and crop the board and the cells of the image.
        BoardDetection boardDetection = new BoardDetection(Settings.BOARD_IMAGE);
        boardDetection.openBrowser(Settings.SUDOKU_URL);
        boardDetection.takeScreenshot();
        boardDetection.detect();

        // Digit recognition. This will attempt to recognize the digits in the cells of the image.
        DigitRecognition digitRecognition = new DigitRecognition(Settings.BOARD_CELL_IMAGE_OUTPUT);
        digitRecognition.doOCR();

        // AutoTyper. This will attempt to type the digits into the sudoku board.
        AutoTyper autoTyper = new AutoTyper(digitRecognition.getBoard());
        autoTyper.start();

        double finish = (System.currentTimeMillis() - start) / 1000.0;
        double without_screenshot_delay = Math.max(finish, Settings.SCREENSHOT_DELAY) - Math.min(finish, Settings.SCREENSHOT_DELAY);

        Notification.send(new Notification("The board has been solved! Execution time: " + finish + " ms"));

        System.out.println("Finished execution in " + finish + " ms");
        System.out.println("Execution time without screenshot delay: " + without_screenshot_delay + " ms");


    }

}
