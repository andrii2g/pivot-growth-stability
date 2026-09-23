using PivotGrowth.Core.Fixtures;

namespace PivotGrowth.Cli.Experiments;

public static class FixtureFactory
{
    public static LinearSystemFixture Create(string name, int size, ulong seed = 42, double epsilon = 1e-8) => name switch
    {
        "wilkinson" => WilkinsonFixture.Create(size),
        "hilbert" => HilbertFixture.Create(size),
        "near-dependent" => NearDependentFixture.Create(size, epsilon, seed),
        "diagonally-dominant" => DiagonallyDominantFixture.Create(size, seed),
        "well-conditioned" => RandomWellConditionedFixture.Create(size, seed),
        _ => throw new ArgumentException($"Unknown fixture: {name}", nameof(name))
    };
}
