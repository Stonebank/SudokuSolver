package com.hk.stonebank.board;

import com.hk.stonebank.settings.Settings;

public class SudokuBoard {

    private final int[][] board;
    private final int size;

    public SudokuBoard(int[][] board) {
        this.board = board;
        this.size = Settings.SUDOKU_BOARD_SIZE;
    }

    public void displayBoard() {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                sb.append(" ").append(board[i][j]);
            }
            sb.append("\n");
        }
        sb.append("\n");
        System.out.print(sb);
    }

    public boolean canSolve() {
        int row = 0;
        int col = 0;
        int minValues = Integer.MAX_VALUE;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                if (board[i][j] == 0) {
                    int remainingValues = getRemainingValues(i, j);
                    if (remainingValues < minValues) {
                        row = i;
                        col = j;
                        minValues = remainingValues;
                    }
                }
            }
        }

        if (minValues == Integer.MAX_VALUE)
            return true;

        for (int num = 1; num <= size; num++) {
            if (isAccepted(row, col, num)) {
                board[row][col] = num;
                if (canSolve())
                    return true;
                else
                    board[row][col] = 0;
            }
        }
        return false;
    }

    private boolean inRow(int row, int num) {
        for (int col = 0; col < size; col++)
            if (board[row][col] == num)
                return true;
        return false;
    }

    private boolean inCol(int col, int num) {
        for (int row = 0; row < size; row++)
            if (board[row][col] == num)
                return true;
        return false;
    }

    private boolean inBox(int row, int col, int num) {
        int r = row - row % 3;
        int c = col - col % 3;
        for (int i = r; i < r + 3; i++) {
            for (int j = c; j < c + 3; j++) {
                if (board[i][j] == num)
                    return true;
            }
        }
        return false;
    }

    private boolean isAccepted(int row, int col, int num) {
        return !inRow(row, num) && !inCol(col, num) && !inBox(row, col, num);
    }

    private int getRemainingValues(int row, int col) {
        int count = 0;
        for (int num = 1; num <= size; num++) {
            if (isAccepted(row, col, num))
                count++;
        }
        return count;
    }

    public int[][] getBoard() {
        return board;
    }

}