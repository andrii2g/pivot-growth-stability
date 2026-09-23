using System.Globalization;
using System.Xml.Linq;
using PivotGrowth.Cli.Experiments;
using PivotGrowth.Cli.Reporting;
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Pivoting;
using Xunit;

namespace PivotGrowth.Tests;

public sealed class SvgTests
{
    [Fact]
    public void ChartIsValidDeterministicAndLocaleIndependent()
    {
        var records = ExperimentRunner.Run([WilkinsonFixture.Create(4), WilkinsonFixture.Create(8)],
            [new NoPivotStrategy(), new PartialPivotStrategy(), new CompletePivotStrategy()]);
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
            string first = SvgChartWriter.Create(records, "Growth <&>", record => record.Metrics.GrowthFactor);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Equal(first, SvgChartWriter.Create(records, "Growth <&>", record => record.Metrics.GrowthFactor));
            XDocument document = XDocument.Parse(first);
            Assert.Contains("Growth &lt;&amp;&gt;", first, StringComparison.Ordinal);
            Assert.Equal(6, document.Descendants(XName.Get("circle", "http://www.w3.org/2000/svg")).Count());
            AssertFiniteCoordinates(document);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1e-300)]
    public void HandlesZeroMissingAndSinglePointData(double metric)
    {
        var records = ExperimentRunner.Run([HilbertFixture.Create(2)], [new PartialPivotStrategy()]);
        string chart = SvgChartWriter.Create(records, "Edge cases", _ => metric);
        AssertFiniteCoordinates(XDocument.Parse(chart));
        Assert.DoesNotContain("NaN", chart, StringComparison.Ordinal);
        Assert.DoesNotContain("Infinity", chart, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyChartsAndZeroEpsilonHaveFiniteCoordinates()
    {
        AssertFiniteCoordinates(XDocument.Parse(SvgChartWriter.Create([], "Empty", _ => null)));
        var records = ExperimentRunner.Run([NearDependentFixture.Create(2, 0)], [new PartialPivotStrategy()]);
        AssertFiniteCoordinates(XDocument.Parse(SvgChartWriter.Create(records, "Zero epsilon", _ => 1, epsilonAxis: true)));
    }

    private static void AssertFiniteCoordinates(XDocument document)
    {
        foreach (XAttribute attribute in document.Descendants().Attributes())
        {
            if (attribute.Name.LocalName is "x" or "y" or "x1" or "x2" or "y1" or "y2" or "cx" or "cy")
            {
                Assert.True(double.IsFinite(double.Parse(attribute.Value, CultureInfo.InvariantCulture)));
            }
            if (attribute.Name.LocalName == "points")
            {
                foreach (string number in attribute.Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
                {
                    Assert.True(double.IsFinite(double.Parse(number, CultureInfo.InvariantCulture)));
                }
            }
        }
    }
}
