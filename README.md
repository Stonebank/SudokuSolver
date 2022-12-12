# SudokuSolver

SudokuSolver is a successor from my previous [version](https://github.com/Stonebank/SudukoSolver-Old).
The newer version of the software will use [OpenCV](https://opencv.org/) and [Tesseract](https://en.wikipedia.org/wiki/Tesseract) with the digits trained dataset. 

The older software did not always work as intended. The objective is to make this software work flawlessly at any time. 

# Backtracking solving algorithm 

The backtracking solving algorithm is a brute-force search algorithm that tries all the possible combinations of numbers until a solution is found that satisfies the constraints of the puzzle.
Read more about it [here](https://www.geeksforgeeks.org/backtracking-algorithms/)

# OpenCV algorithm 

The objective of this algorithm is to identify and extract a Sudoku board from an image. 

This is the steps summarized for this algorithm
1. Image is converted to grayscale 
2. Blur filter is applied to the image to reduce noise
3. Canny edge detection algorithm to identify the contours and selects the largest contour
4. It adds a margin to remove any borderlines 
5. Finally the image is extracted by using the coordinates of the bounding rectangle

# Tesseract 

The objective of this algorithm is to perform OCR to identify the digit. The training dataset applied for this is specified for digit training. The solution is still experimental and therefore it is not optimal yet.

# Developer

This software is developed and designed by Hassan K.

