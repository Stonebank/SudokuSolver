package com.hk.stonebank.exception;

public class UnsolvableException extends RuntimeException {

    public UnsolvableException() {
        super("The board is unsolvable.");
    }

}
