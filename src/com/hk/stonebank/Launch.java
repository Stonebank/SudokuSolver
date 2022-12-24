package com.hk.stonebank;

import com.google.common.base.Stopwatch;
import com.hk.stonebank.board.keyboard.AutoTyper;
import com.hk.stonebank.board.mode.GameMode;
import com.hk.stonebank.image.BoardDetection;
import com.hk.stonebank.image.DigitRecognition;
import com.hk.stonebank.notification.Notification;
import com.hk.stonebank.settings.Settings;
import org.opencv.core.Core;

import java.awt.*;
import java.util.Arrays;
import java.util.Scanner;
import java.util.concurrent.TimeUnit;

public class Launch {

    public static void main(String[] args) throws AWTException {

        System.loadLibrary(Core.NATIVE_LIBRARY_NAME);

        System.out.println("Choose difficulty:");
        Arrays.stream(GameMode.values()).forEach(gameMode -> System.out.println(gameMode.ordinal() + 1 + ". " + gameMode.name()));

        var scanner = new Scanner(System.in);
        while (Settings.GAME_MODE == null) {
            String input = scanner.nextLine();
            switch (input) {
                case "1" -> Settings.GAME_MODE = GameMode.EASY;
                case "2" -> Settings.GAME_MODE = GameMode.MEDIUM;
                case "3" -> Settings.GAME_MODE = GameMode.HARD;
                case "4" -> Settings.GAME_MODE = GameMode.EXPERT;
                case "5" -> Settings.GAME_MODE = GameMode.EVIl;
                case "6" -> Settings.GAME_MODE = GameMode.DAILY_CHALLENGE;
                default -> System.out.println("Choose a game mode from 1-6");
            }
        }

        Notification.send(new Notification("Welcome to SudokuSolver! Please keep browser in focus for best results and do not touch the board as the board is detected."));

        var stopwatch = Stopwatch.createStarted();

        // Board detection. This will attempt to detect the sudoku board in the image and crop the board and the cells of the image.
        BoardDetection boardDetection = new BoardDetection(Settings.BOARD_IMAGE);
        boardDetection.openBrowser(Settings.SUDOKU_URL + "/" + (Settings.GAME_MODE == GameMode.DAILY_CHALLENGE ? "challenges/daily-sudoku" : Settings.GAME_MODE.name().toLowerCase()));
        boardDetection.takeScreenshot();
        boardDetection.detect();

        // Digit recognition. This will attempt to recognize the digits in the cells of the image.
        DigitRecognition digitRecognition = new DigitRecognition(Settings.BOARD_CELL_IMAGE_OUTPUT);
        digitRecognition.doOCR();

        // AutoTyper. This will attempt to type the digits into the sudoku board.
        AutoTyper autoTyper = new AutoTyper(digitRecognition.getBoard());
        autoTyper.start();

        stopwatch.stop();
        var result = stopwatch.elapsed(TimeUnit.MILLISECONDS);

        Notification.send(new Notification("The board has been solved! Total execution time: " + result + " ms"));

        System.out.println("Finished execution in " + result + " ms");


    }

}
