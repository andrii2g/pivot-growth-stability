using BenchmarkDotNet.Attributes;
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;

namespace PivotGrowth.Benchmarks;

/// <summary>Fixture construction is outside measurement; Solve includes cloning and instrumentation.</summary>
[MemoryDiagnoser]
public class GaussianSolverBenchmarks
{
    private LinearSystemFixture fixture = null!;
    private double[] rightHandSide = null!;
    private GaussianEliminationSolver solver = null!;

    [Params(64, 128, 256, 512)]
    public int Size { get; set; }

    [Params("none", "partial", "complete")]
    public string Pivot { get; set; } = "partial";

    [GlobalSetup]
    public void Setup()
    {
        fixture = RandomWellConditionedFixture.Create(Size, 42);
        rightHandSide = fixture.RightHandSide.ToArray();
        IPivotStrategy strategy = Pivot switch
        {
            "none" => new NoPivotStrategy(),
            "partial" => new PartialPivotStrategy(),
            "complete" => new CompletePivotStrategy(),
            _ => throw new InvalidOperationException("Unknown strategy.")
        };
        solver = new GaussianEliminationSolver(strategy);
    }

    [Benchmark]
    public SolveResult Solve() => solver.Solve(fixture.Matrix, rightHandSide);
}
