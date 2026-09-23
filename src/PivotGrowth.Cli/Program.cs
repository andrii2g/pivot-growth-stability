using System.Globalization;
using PivotGrowth.Cli.Cli;
using PivotGrowth.Cli.Experiments;
using PivotGrowth.Cli.Reporting;
using PivotGrowth.Core.Fixtures;
using PivotGrowth.Core.Solvers;

namespace PivotGrowth.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || (args.Length == 1 && args[0] is "--help" or "-h" or "help"))
        {
            Console.WriteLine("""
                Pivot Growth Stability Lab
                run   --fixture NAME --sizes 4:40:4 --pivot none,partial,complete --out artifacts/run
                suite --preset smoke|readme|performance --out artifacts/suite

                Fixtures: wilkinson, hilbert, near-dependent, diagonally-dominant, well-conditioned
                Sizes: comma-separated integers or start:end:step (inclusive), 1..4096
                Shared options: --seed 42 --repetitions 1 --pivot none,partial,complete --out DIRECTORY
                Near-dependent option: --epsilon 1e-2,1e-6,1e-12 (size must be >= 2)
                Defaults: run wilkinson size 8; suite smoke; all strategies; seed 42; one repetition.
                Outputs: results.csv, results.md, and SVG charts for each fixture and metric.
                Exit codes: 0 completed; 1 a solve failed; 2 invalid arguments or output error.
                """);
            return 0;
        }
        try
        {
            CliOptions options = CliOptions.Parse(args);
            IEnumerable<LinearSystemFixture> fixtures = options.Command == "suite"
                ? ExperimentPresets.Create(options.Preset, options.Seed) : CreateFixtures(options);
            var strategies = options.Pivots.Select(ExperimentRunner.CreateStrategy).ToArray();
            IReadOnlyList<ExperimentRecord> records = ExperimentRunner.Run(fixtures, strategies, options.Repetitions);
            ReportWriter.Write(options.OutputDirectory, records);
            SvgChartWriter.Write(options.OutputDirectory, records);
            Console.Write(ReportWriter.Markdown(records));
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"\nWrote {records.Count} records to {Path.GetFullPath(options.OutputDirectory)}"));
            return records.Any(record => record.Status != SolveStatus.Success) ? 1 : 0;
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            Console.Error.WriteLine("Run with --help for usage.");
            return 2;
        }
    }

    private static IEnumerable<LinearSystemFixture> CreateFixtures(CliOptions options)
    {
        foreach (int size in options.Sizes)
        {
            if (options.Fixture == "near-dependent")
            {
                foreach (double epsilon in options.Epsilons)
                {
                    yield return FixtureFactory.Create(options.Fixture, size, options.Seed, epsilon);
                }
            }
            else
            {
                yield return FixtureFactory.Create(options.Fixture, size, options.Seed);
            }
        }
    }
}
