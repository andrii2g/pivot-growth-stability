using System.Globalization;
using System.Xml.Linq;
using PivotGrowth.Cli;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class CliIntegrationTests
{
    [Fact]
    public void CommandWritesAllReportsAndCharts()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PivotGrowth.Tests", Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        try
        {
            int code = Program.Main(["run", "--fixture", "wilkinson", "--sizes", "4,8", "--out", directory]);
            Assert.Equal(0, code);
            Assert.Equal(7, File.ReadAllLines(Path.Combine(directory, "results.csv")).Length);
            Assert.True(File.Exists(Path.Combine(directory, "results.md")));
            string[] charts = Directory.GetFiles(directory, "*.svg");
            Assert.Equal(5, charts.Length);
            foreach (string chart in charts)
            {
                Assert.NotNull(XDocument.Load(chart).Root);
            }
            Assert.Equal(1, Program.Main(["run", "--fixture", "near-dependent", "--sizes", "4", "--epsilon", "0", "--out", directory]));
            Assert.Contains("Singular", File.ReadAllText(Path.Combine(directory, "results.csv")), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void InvalidCommandsReturnUsageError()
    {
        Assert.Equal(2, Program.Main(["unknown"]));
        Assert.Equal(2, Program.Main(["run", "--sizes", "0"]));
        Assert.Equal(0, Program.Main(["--help"]));
    }
}
