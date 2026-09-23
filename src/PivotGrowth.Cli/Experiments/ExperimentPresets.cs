using PivotGrowth.Core.Fixtures;

namespace PivotGrowth.Cli.Experiments;

public static class ExperimentPresets
{
    public static IEnumerable<LinearSystemFixture> Create(string name, ulong seed = 42)
    {
        if (name is not ("smoke" or "readme" or "performance"))
        {
            throw new ArgumentException($"Unknown preset: {name}", nameof(name));
        }
        if (name == "smoke")
        {
            yield return WilkinsonFixture.Create(8);
            yield return HilbertFixture.Create(8);
            yield return NearDependentFixture.Create(8, 1e-8, seed);
            yield return DiagonallyDominantFixture.Create(8, seed);
            yield return RandomWellConditionedFixture.Create(8, seed);
            yield break;
        }
        if (name == "readme")
        {
            for (int size = 4; size <= 40; size += 4)
            {
                yield return WilkinsonFixture.Create(size);
            }
            for (int size = 2; size <= 16; size++)
            {
                yield return HilbertFixture.Create(size);
            }
            double[] epsilons = [1e-2, 1e-4, 1e-6, 1e-8, 1e-10, 1e-12, 1e-14];
            foreach (double epsilon in epsilons)
            {
                yield return NearDependentFixture.Create(8, epsilon, seed);
            }
        }
        foreach (int size in new[] { 64, 128, 256, 512 })
        {
            yield return RandomWellConditionedFixture.Create(size, seed);
        }
    }
}
