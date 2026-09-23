using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Numerics;
using PivotGrowth.Core.Pivoting;
using PivotGrowth.Core.Solvers;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class FixtureTests
{
    [Fact]
    public void GeneratorHasStableKnownBitSequence()
    {
        var random = new DeterministicRng(0);
        Assert.Equal(0xE220A8397B1DCDAFUL, random.NextUInt64());
        Assert.Equal(0x6E789E6AA1B965F4UL, random.NextUInt64());
    }

    [Fact]
    public void FixturesAreDeterministicAndDeriveRightHandSides()
    {
        LinearSystemFixture[] fixtures =
        [
            DiagonallyDominantFixture.Create(6), HilbertFixture.Create(6), WilkinsonFixture.Create(6),
            NearDependentFixture.Create(6), RandomWellConditionedFixture.Create(6)
        ];
        foreach (LinearSystemFixture fixture in fixtures)
        {
            Assert.Equal(MatrixMath.Multiply(fixture.Matrix, fixture.TrueSolution.ToArray()), fixture.RightHandSide);
        }
        Assert.Equal(DiagonallyDominantFixture.Create(8, 7).Matrix.Values.ToArray(),
            DiagonallyDominantFixture.Create(8, 7).Matrix.Values.ToArray());
        Assert.NotEqual(DiagonallyDominantFixture.Create(8, 7).Matrix.Values.ToArray(),
            DiagonallyDominantFixture.Create(8, 8).Matrix.Values.ToArray());
    }

    [Theory]
    [InlineData(4)]
    [InlineData(12)]
    [InlineData(40)]
    public void WilkinsonHasExpectedExponentialGrowth(int size)
    {
        LinearSystemFixture fixture = WilkinsonFixture.Create(size);
        foreach (IPivotStrategy strategy in new IPivotStrategy[] { new NoPivotStrategy(), new PartialPivotStrategy() })
        {
            SolveResult result = new GaussianEliminationSolver(strategy).Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
            Assert.Equal(SolveStatus.Success, result.Status);
            Assert.Equal(Math.Pow(2, size - 1), result.Metrics.GrowthFactor);
            Assert.Equal(0, result.Metrics.RowSwaps);
        }
        SolveResult complete = new GaussianEliminationSolver(new CompletePivotStrategy())
            .Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
        Assert.Equal(SolveStatus.Success, complete.Status);
        Assert.Equal(2, complete.Metrics.GrowthFactor);
    }

    [Fact]
    public void HilbertSeparatesResidualFromForwardError()
    {
        LinearSystemFixture fixture = HilbertFixture.Create(12);
        SolveResult result = new GaussianEliminationSolver(new PartialPivotStrategy())
            .Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
        Assert.Equal(SolveStatus.Success, result.Status);
        Assert.True(result.Metrics.RelativeResidual < 1e-14);
        Assert.True(VectorMath.RelativeForwardError(result.Solution!.ToArray(), fixture.TrueSolution.ToArray()) > 1e-5);
    }

    [Fact]
    public void NearDependenceRespondsToEpsilon()
    {
        LinearSystemFixture loose = NearDependentFixture.Create(8, 1e-2);
        LinearSystemFixture tight = NearDependentFixture.Create(8, 1e-12);
        var solver = new GaussianEliminationSolver(new PartialPivotStrategy());
        SolveResult first = solver.Solve(loose.Matrix, loose.RightHandSide.ToArray());
        SolveResult second = solver.Solve(tight.Matrix, tight.RightHandSide.ToArray());
        Assert.Equal(SolveStatus.Success, second.Status);
        Assert.True(second.Metrics.MinimumAbsolutePivot < first.Metrics.MinimumAbsolutePivot * 1e-8);
        LinearSystemFixture singular = NearDependentFixture.Create(8, 0);
        Assert.Equal(SolveStatus.Singular, solver.Solve(singular.Matrix, singular.RightHandSide.ToArray()).Status);
    }

    [Theory]
    [MemberData(nameof(SolverTests.Strategies), MemberType = typeof(SolverTests))]
    public void SeededBaselinesSolveAccurately(IPivotStrategy strategy)
    {
        foreach (LinearSystemFixture fixture in new[] { DiagonallyDominantFixture.Create(16), RandomWellConditionedFixture.Create(16) })
        {
            SolveResult result = new GaussianEliminationSolver(strategy).Solve(fixture.Matrix, fixture.RightHandSide.ToArray());
            Assert.Equal(SolveStatus.Success, result.Status);
            Assert.True(VectorMath.RelativeForwardError(result.Solution!.ToArray(), fixture.TrueSolution.ToArray()) < 1e-13);
        }
    }
}
