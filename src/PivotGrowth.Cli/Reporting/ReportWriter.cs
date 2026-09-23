using System.Globalization;
using System.Text;
using PivotGrowth.Cli.Experiments;
using PivotGrowth.Core.Metrics;

namespace PivotGrowth.Cli.Reporting;

public static class ReportWriter
{
    /// <summary>Missing or non-finite metrics are empty in CSV and shown as unavailable in Markdown.</summary>
    public static string Number(double? value) =>
        value.HasValue && double.IsFinite(value.Value) ? value.Value.ToString("G17", CultureInfo.InvariantCulture) : "";

    public static string Csv(IEnumerable<ExperimentRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        var output = new StringBuilder();
        output.AppendLine("Fixture,Size,Seed,Epsilon,Pivot,Repetition,Status,RelativeForwardError,PivotComparisons,RowSwaps,ColumnSwaps,EliminationMultipliers,MatrixUpdates,Divisions,OriginalMaxAbs,MaxObservedAbs,GrowthFactor,MinimumAbsolutePivot,TinyPivotCount,RelativeResidual,EncounteredNonFiniteValue,ElapsedMilliseconds");
        foreach (ExperimentRecord record in records)
        {
            SolveMetrics metrics = record.Metrics;
            string[] fields =
            [
                Escape(record.Fixture), Integer(record.Size), record.Seed.ToString(CultureInfo.InvariantCulture),
                Number(record.Epsilon), Escape(record.Pivot), Integer(record.Repetition), record.Status.ToString(),
                Number(record.RelativeForwardError), Integer(metrics.PivotComparisons), Integer(metrics.RowSwaps),
                Integer(metrics.ColumnSwaps), Integer(metrics.EliminationMultipliers), Integer(metrics.MatrixUpdates),
                Integer(metrics.Divisions), Number(metrics.OriginalMaxAbs), Number(metrics.MaxObservedAbs),
                Number(metrics.GrowthFactor), Number(metrics.MinimumAbsolutePivot), Integer(metrics.TinyPivotCount),
                Number(metrics.RelativeResidual), metrics.EncounteredNonFiniteValue ? "true" : "false", Number(metrics.Elapsed.TotalMilliseconds)
            ];
            output.AppendJoin(',', fields).AppendLine();
        }
        return output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    public static string Markdown(IEnumerable<ExperimentRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        var output = new StringBuilder("# Experiment results\n\n");
        output.AppendLine("Times include validation, cloning, elimination, back substitution and residual calculation. Single-run timings are diagnostic; use BenchmarkDotNet for performance comparisons. Missing or non-finite values appear as —.");
        output.AppendLine();
        output.AppendLine("| Fixture | n | ε | Pivot | Run | Status | Growth | Residual | Forward error | Comparisons | Row / column swaps | Time (ms) |");
        output.AppendLine("|---|---:|---:|---|---:|---|---:|---:|---:|---:|---:|---:|");
        foreach (ExperimentRecord record in records)
        {
            SolveMetrics metrics = record.Metrics;
            output.AppendLine(CultureInfo.InvariantCulture,
                $"| {MarkdownText(record.Fixture)} | {record.Size} | {Display(record.Epsilon)} | {MarkdownText(record.Pivot)} | {record.Repetition} | {record.Status} | {Display(metrics.GrowthFactor)} | {Display(metrics.RelativeResidual)} | {Display(record.RelativeForwardError)} | {metrics.PivotComparisons} | {metrics.RowSwaps} / {metrics.ColumnSwaps} | {Display(metrics.Elapsed.TotalMilliseconds)} |");
        }
        return output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    public static void Write(string directory, IReadOnlyList<ExperimentRecord> records)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(records);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "results.csv"), Csv(records));
        File.WriteAllText(Path.Combine(directory, "results.md"), Markdown(records));
    }

    private static string Integer(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Display(double? value) =>
        value.HasValue && double.IsFinite(value.Value) ? value.Value.ToString("G6", CultureInfo.InvariantCulture) : "—";
    private static string Escape(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    private static string MarkdownText(string value) => value.Replace("|", "\\|", StringComparison.Ordinal)
        .Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
}
