package com.hk.stonebank.image;

import com.hk.stonebank.settings.Settings;
import net.sourceforge.tess4j.ITessAPI;
import net.sourceforge.tess4j.Tesseract;
import net.sourceforge.tess4j.TesseractException;

import java.io.File;
import java.util.Objects;

public class DigitRecognition {

    private final File directory;

    private final Tesseract tesseract;

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
        this.tesseract.setLanguage("./resources/tesseract/model/digits");

    }

    public void doOCR() {
        for (File cell : Objects.requireNonNull(directory.listFiles())) {
            if (cell == null)
                continue;
            if (!cell.getName().endsWith(".png")) {
                System.err.println(cell.getName() + " is not a png image file, skipped.");
                continue;
            }
            try {
                var result = tesseract.doOCR(cell);
                if (result.isEmpty() || result.isBlank()) {
                    continue;
                }
                if (result.trim().toCharArray().length > 1) {
                    System.out.println(cell.getName() + " is not recognized as single digit, best guess: " + result.toCharArray()[0]);
                    continue;
                }
                System.out.println("Result for " + cell.getName() + ": " + result);
            } catch (TesseractException e) {
                System.err.println("Tesseract error: " + e);
                e.printStackTrace();
            }
        }
    }

}
