using PivotGrowth.Cli.Experiments;
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Pivoting;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class ExperimentTests
{
    [Fact]
    public void RunnerPreservesInputsAndOrdersRecordsDeterministically()
    {
        LinearSystemFixture fixture = WilkinsonFixture.Create(4);
        double[] original = fixture.Matrix.Values.ToArray();
        IPivotStrategy[] strategies = [new NoPivotStrategy(), new PartialPivotStrategy(), new CompletePivotStrategy()];
        var first = ExperimentRunner.Run([fixture], strategies, 2);
        var second = ExperimentRunner.Run([fixture], strategies, 2);
        Assert.Equal(6, first.Count);
        for (int index = 0; index < first.Count; index++)
        {
            Assert.Equal(first[index] with { Metrics = first[index].Metrics with { Elapsed = TimeSpan.Zero } },
                second[index] with { Metrics = second[index].Metrics with { Elapsed = TimeSpan.Zero } });
        }
        Assert.Equal(["none", "partial", "complete", "none", "partial", "complete"], first.Select(record => record.Pivot));
        Assert.Equal(original, fixture.Matrix.Values.ToArray());
    }

    [Fact]
    public void PresetsHaveCanonicalCoverage()
    {
        Assert.Equal(5, ExperimentPresets.Create("smoke").Count());
        Assert.Equal(36, ExperimentPresets.Create("readme").Count());
        Assert.Equal([64, 128, 256, 512], ExperimentPresets.Create("performance").Select(fixture => fixture.Matrix.Rows));
        Assert.Throws<ArgumentException>(() => ExperimentPresets.Create("missing").ToArray());
    }
}
