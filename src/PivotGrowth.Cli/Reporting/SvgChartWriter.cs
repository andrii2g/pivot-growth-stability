using System.Globalization;
using System.Xml.Linq;
using PivotGrowth.Cli.Experiments;

namespace PivotGrowth.Cli.Reporting;

/// <summary>Draws saved metrics only. Zero values are placed below the smallest positive log value; missing values are omitted.</summary>
public static class SvgChartWriter
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    public static string Create(IReadOnlyList<ExperimentRecord> records, string title,
        Func<ExperimentRecord, double?> metric, bool epsilonAxis = false)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(metric);
        var samples = records.Select(record => new
        {
            Record = record,
            X = epsilonAxis ? (record.Epsilon > 0 ? Math.Log10(record.Epsilon.Value) : double.NaN) : record.Size,
            Y = metric(record)
        }).Where(point => double.IsFinite(point.X) && point.Y.HasValue && double.IsFinite(point.Y.Value) && point.Y >= 0).ToArray();
        double[] positiveLogs = samples.Where(point => point.Y > 0).Select(point => Math.Log10(point.Y!.Value)).ToArray();
        double floor = positiveLogs.Length == 0 ? -1 : positiveLogs.Min() - 1;
        double minimumY = samples.Length == 0 ? -1 : samples.Min(point => point.Y == 0 ? floor : Math.Log10(point.Y!.Value));
        double maximumY = samples.Length == 0 ? 1 : samples.Max(point => point.Y == 0 ? floor : Math.Log10(point.Y!.Value));
        if (minimumY == maximumY)
        {
            minimumY -= 0.5;
            maximumY += 0.5;
        }
        double minimumX = samples.Length == 0 ? 0 : samples.Min(point => point.X);
        double maximumX = samples.Length == 0 ? 1 : samples.Max(point => point.X);
        if (minimumX == maximumX)
        {
            minimumX -= 0.5;
            maximumX += 0.5;
        }
        double X(double value) => 100 + (value - minimumX) / (maximumX - minimumX) * 650;
        double Y(double value) => 345 - ((value == 0 ? floor : Math.Log10(value)) - minimumY) / (maximumY - minimumY) * 245;
        var root = new XElement(Svg + "svg",
            new XAttribute("viewBox", "0 0 840 455"), new XAttribute("role", "img"),
            new XAttribute("aria-labelledby", "title description"),
            new XElement(Svg + "title", new XAttribute("id", "title"), title),
            new XElement(Svg + "desc", new XAttribute("id", "description"),
                "Comparison of pivot strategies. Vertical axis is logarithmic. Zero values appear at the bottom; unavailable values are omitted."),
            new XElement(Svg + "rect", new XAttribute("width", "840"), new XAttribute("height", "455"), new XAttribute("fill", "#ffffff")));
        root.Add(Text(30, 32, title, 19, "#172b4d"));
        root.Add(Text(30, 55, "Logarithmic metric scale · measured experiment records", 12, "#52616b"));
        string[] pivots = ["none", "partial", "complete"];
        for (int index = 0; index < pivots.Length; index++)
        {
            root.Add(Line(100 + index * 190, 78, 125 + index * 190, 78, Color(pivots[index])));
            root.Add(Text(132 + index * 190, 82, pivots[index], 12, "#172b4d"));
        }
        for (int index = 0; index <= 5; index++)
        {
            double fraction = index / 5.0;
            double y = 345 - fraction * 245;
            double logValue = minimumY + fraction * (maximumY - minimumY);
            root.Add(Line(100, y, 750, y, "#e3e8ef"));
            root.Add(Text(89, y + 4, $"10^{logValue.ToString("0.##", CultureInfo.InvariantCulture)}", 11, "#52616b", "end"));
            double xValue = minimumX + fraction * (maximumX - minimumX);
            string label = epsilonAxis ? $"10^{xValue.ToString("0.##", CultureInfo.InvariantCulture)}"
                : xValue.ToString("0.##", CultureInfo.InvariantCulture);
            root.Add(Text(X(xValue), 367, label, 11, "#52616b", "middle"));
        }
        root.Add(Line(100, 100, 100, 345, "#52616b"));
        root.Add(Line(100, 345, 750, 345, "#52616b"));
        foreach (var series in samples.GroupBy(point => (point.Record.Pivot, point.Record.Repetition)))
        {
            var ordered = series.OrderBy(point => point.X).ToArray();
            string points = string.Join(" ", ordered.Select(point => $"{Number(X(point.X))},{Number(Y(point.Y!.Value))}"));
            root.Add(new XElement(Svg + "polyline", new XAttribute("points", points),
                new XAttribute("fill", "none"), new XAttribute("stroke", Color(series.Key.Pivot)),
                new XAttribute("stroke-width", "2"), new XAttribute("stroke-opacity", "0.75"),
                new XAttribute("stroke-dasharray", series.Key.Pivot == "none" ? "7 4" : series.Key.Pivot == "partial" ? "2 3" : "none")));
            foreach (var point in ordered)
            {
                root.Add(new XElement(Svg + "circle", new XAttribute("cx", Number(X(point.X))),
                    new XAttribute("cy", Number(Y(point.Y!.Value))), new XAttribute("r", "3.5"),
                    new XAttribute("fill", Color(series.Key.Pivot)),
                    new XElement(Svg + "title", string.Create(CultureInfo.InvariantCulture,
                        $"{point.Record.Pivot}, n={point.Record.Size}, epsilon={point.Record.Epsilon}, run={point.Record.Repetition}: {point.Y:G17}"))));
            }
        }
        root.Add(Text(425, 392, epsilonAxis ? "Epsilon (logarithmic)" : "Matrix size n", 13, "#172b4d", "middle"));
        string note = samples.Length == 0 ? "No finite nonnegative data available."
            : $"Zero values use the plot floor; unavailable points omitted: {records.Count - samples.Length}.";
        root.Add(Text(30, 426, note, 12, "#52616b"));
        return new XDocument(root).ToString() + "\n";
    }

    public static void Write(string directory, IReadOnlyList<ExperimentRecord> records)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(records);
        Directory.CreateDirectory(directory);
        foreach (var group in records.GroupBy(record => (record.Fixture, Size: record.Fixture == "near-dependent" ? record.Size : 0)))
        {
            bool epsilonAxis = group.Key.Fixture == "near-dependent";
            string name = group.Key.Fixture + (epsilonAxis ? $"-n{group.Key.Size.ToString(CultureInfo.InvariantCulture)}" : "");
            ExperimentRecord[] data = group.ToArray();
            WriteMetric("growth", "Element growth factor", record => record.Metrics.GrowthFactor);
            WriteMetric("residual", "Relative residual", record => record.Metrics.RelativeResidual);
            WriteMetric("forward-error", "Relative forward error", record => record.RelativeForwardError);
            WriteMetric("comparisons", "Pivot comparisons", record => record.Metrics.PivotComparisons);
            WriteMetric("runtime", "Elapsed time (ms)", record => record.Metrics.Elapsed.TotalMilliseconds);

            void WriteMetric(string suffix, string label, Func<ExperimentRecord, double?> metric)
            {
                File.WriteAllText(Path.Combine(directory, $"{name}-{suffix}.svg"),
                    Create(data, $"{name}: {label}", metric, epsilonAxis));
            }
        }
    }

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string Color(string pivot) => pivot switch
    {
        "none" => "#b85414",
        "partial" => "#1765b5",
        "complete" => "#07806e",
        _ => "#4b5563"
    };

    private static XElement Line(double x1, double y1, double x2, double y2, string color) =>
        new(Svg + "line", new XAttribute("x1", Number(x1)), new XAttribute("y1", Number(y1)),
            new XAttribute("x2", Number(x2)), new XAttribute("y2", Number(y2)),
            new XAttribute("stroke", color), new XAttribute("stroke-width", "1"));

    private static XElement Text(double x, double y, string value, int size, string color, string anchor = "start") =>
        new(Svg + "text", new XAttribute("x", Number(x)), new XAttribute("y", Number(y)),
            new XAttribute("font-family", "system-ui, sans-serif"), new XAttribute("font-size", size),
            new XAttribute("fill", color), new XAttribute("text-anchor", anchor), value);
}
