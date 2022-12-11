package com.hk.stonebank;

import com.hk.stonebank.image.BoardDetection;
import com.hk.stonebank.image.DigitRecognition;
import com.hk.stonebank.settings.Settings;
import org.opencv.core.Core;

public class Launch {

    public static void main(String[] args) {

        System.loadLibrary(Core.NATIVE_LIBRARY_NAME);

        // Board detection. This will attempt to detect the sudoku board in the image and crop the board and the cells of the image.
        BoardDetection boardDetection = new BoardDetection(Settings.BOARD_IMAGE);
        boardDetection.detect();

        // Digit recognition. This will attempt to recognize the digits in the cells of the image.
        DigitRecognition digitRecognition = new DigitRecognition(Settings.BOARD_CELL_IMAGE_OUTPUT);
        digitRecognition.doOCR();

    }

}
