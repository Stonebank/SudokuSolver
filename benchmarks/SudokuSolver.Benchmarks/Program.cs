using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SudokuSolver.Domain;

BenchmarkRunner.Run<SolverBenchmarks>();

[MemoryDiagnoser]
public class SolverBenchmarks
{
    private readonly ISudokuSolver _solver = new BitMaskSudokuSolver();
    private SudokuGrid _easy = SudokuGrid.Parse("530070000600195000098000060800060003400803001700020006060000280000419005000080079");
    private SudokuGrid _evil = SudokuGrid.Parse("000000010400000000020000000000050407008000300001090000300400200050100000000806000");
    [Benchmark] public SudokuSolveStatus Easy() => _solver.Solve(_easy).Status;
    [Benchmark] public SudokuSolveStatus Evil() => _solver.Solve(_evil).Status;
}
