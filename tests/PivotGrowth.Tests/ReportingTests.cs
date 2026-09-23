using System.Globalization;
using PivotGrowth.Cli.Cli;
using PivotGrowth.Cli.Experiments;
using PivotGrowth.Cli.Reporting;
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Pivoting;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class ReportingTests
{
    [Fact]
    public void CsvIsInvariantUnderCommaDecimalCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var records = ExperimentRunner.Run([HilbertFixture.Create(4)], [new PartialPivotStrategy()]);
            string csv = ReportWriter.Csv(records);
            string[] lines = csv.Trim().Split('\n');
            Assert.Equal(2, lines.Length);
            Assert.Equal(22, lines[1].Split(',').Length);
            Assert.Equal("0.25", ReportWriter.Number(0.25));
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Equal(csv, ReportWriter.Csv(records));
            Assert.DoesNotContain("\r", csv, StringComparison.Ordinal);
            Assert.Contains("hilbert", ReportWriter.Markdown(records), StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("0:8:1")]
    [InlineData("8:4:1")]
    [InlineData("1:8:0")]
    [InlineData("1:8")]
    [InlineData("4097")]
    [InlineData("1,,2")]
    public void RejectsInvalidSizeArguments(string sizes)
    {
        Assert.ThrowsAny<Exception>(() => CliOptions.Parse(["run", "--sizes", sizes]));
    }

    [Fact]
    public void ParsesRangeAndRejectsUnknownOrRepeatedOptions()
    {
        CliOptions options = CliOptions.Parse(["run", "--sizes", "4:12:4"]);
        Assert.Equal([4, 8, 12], options.Sizes);
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["run", "--unknown", "value"]));
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["run", "--seed", "1", "--seed", "2"]));
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["run", "--pivot", "partial,partial"]));
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["suite", "--fixture", "hilbert"]));
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["run", "--epsilon", "1e-4"]));
        Assert.Throws<ArgumentException>(() => CliOptions.Parse(["run", "--fixture", "near-dependent", "--epsilon", "NaN"]));
    }

    [Fact]
    public void FailedSolvesHaveEmptyMetricsInsteadOfInfinity()
    {
        var records = ExperimentRunner.Run([NearDependentFixture.Create(4, 0)], [new CompletePivotStrategy()]);
        string[] fields = ReportWriter.Csv(records).Trim().Split('\n')[1].Split(',');
        Assert.Equal("Singular", fields[6]);
        Assert.Equal("", fields[7]);
        Assert.Equal("", fields[19]);
        Assert.DoesNotContain("NaN", ReportWriter.Csv(records), StringComparison.Ordinal);
    }
}
