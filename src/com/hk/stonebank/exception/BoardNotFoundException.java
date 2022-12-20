package com.hk.stonebank.exception;

public class BoardNotFoundException extends RuntimeException {

    public BoardNotFoundException() {
        super("The board was not detected.");
    }

}
