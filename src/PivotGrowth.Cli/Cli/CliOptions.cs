using System.Globalization;
using PivotGrowth.Cli.Experiments;

namespace PivotGrowth.Cli.Cli;

public sealed record CliOptions(
    string Command, string Fixture, IReadOnlyList<int> Sizes, IReadOnlyList<string> Pivots,
    IReadOnlyList<double> Epsilons, ulong Seed, int Repetitions, string Preset, string OutputDirectory)
{
    public static CliOptions Parse(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Length == 0 || arguments[0] is not ("run" or "suite"))
        {
            throw new ArgumentException("Expected command 'run' or 'suite'.", nameof(arguments));
        }
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 1; index < arguments.Length; index += 2)
        {
            if (!arguments[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= arguments.Length)
            {
                throw new ArgumentException($"Expected an option and value near '{arguments[index]}'.", nameof(arguments));
            }
            string key = arguments[index][2..];
            bool shared = key is "pivot" or "seed" or "repetitions" or "out";
            bool specific = arguments[0] == "run" ? key is "fixture" or "sizes" or "epsilon" : key == "preset";
            if (!shared && !specific)
            {
                throw new ArgumentException($"Unknown option for {arguments[0]}: --{key}", nameof(arguments));
            }
            if (!options.TryAdd(key, arguments[index + 1]))
            {
                throw new ArgumentException($"Duplicate option: --{key}", nameof(arguments));
            }
        }
        string Get(string key, string fallback) => options.GetValueOrDefault(key, fallback);
        string fixture = Get("fixture", "wilkinson");
        if (fixture is not ("wilkinson" or "hilbert" or "near-dependent" or "diagonally-dominant" or "well-conditioned"))
        {
            throw new ArgumentException($"Unknown fixture: {fixture}", nameof(arguments));
        }
        int[] sizes = ParseSizes(Get("sizes", "8"));
        string[] pivots = Get("pivot", "none,partial,complete").Split(',', StringSplitOptions.TrimEntries);
        foreach (string pivot in pivots)
        {
            _ = ExperimentRunner.CreateStrategy(pivot);
        }
        if (pivots.Distinct(StringComparer.Ordinal).Count() != pivots.Length)
        {
            throw new ArgumentException("Pivot strategies must not be repeated.", nameof(arguments));
        }
        double[] epsilons = Get("epsilon", "1e-8").Split(',')
            .Select(value => double.Parse(value, CultureInfo.InvariantCulture)).ToArray();
        if (epsilons.Any(value => !double.IsFinite(value) || value < 0))
        {
            throw new ArgumentException("Epsilon values must be finite and nonnegative.", nameof(arguments));
        }
        if (arguments[0] == "run" && options.ContainsKey("epsilon") && fixture != "near-dependent")
        {
            throw new ArgumentException("--epsilon applies only to near-dependent fixtures.", nameof(arguments));
        }
        if (fixture == "near-dependent" && sizes.Any(size => size < 2))
        {
            throw new ArgumentException("Near-dependent fixtures require size >= 2.", nameof(arguments));
        }
        ulong seed = ulong.Parse(Get("seed", "42"), CultureInfo.InvariantCulture);
        int repetitions = int.Parse(Get("repetitions", "1"), CultureInfo.InvariantCulture);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repetitions);
        string preset = Get("preset", "smoke");
        if (preset is not ("smoke" or "readme" or "performance"))
        {
            throw new ArgumentException($"Unknown preset: {preset}", nameof(arguments));
        }
        string output = Get("out", $"artifacts/{(arguments[0] == "suite" ? preset : fixture)}");
        ArgumentException.ThrowIfNullOrWhiteSpace(output);
        return new CliOptions(arguments[0], fixture, sizes, pivots, epsilons, seed, repetitions, preset, output);
    }

    private static int[] ParseSizes(string text)
    {
        var sizes = new List<int>();
        if (text.Contains(':', StringComparison.Ordinal))
        {
            string[] parts = text.Split(':');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Size ranges use start:end:step.", nameof(text));
            }
            int start = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int end = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int increment = int.Parse(parts[2], CultureInfo.InvariantCulture);
            if (start < 1 || end < start || end > 4096 || increment < 1)
            {
                throw new ArgumentException("Sizes must be 1..4096 with an increasing range and positive step.", nameof(text));
            }
            for (long size = start; size <= end; size += increment)
            {
                sizes.Add((int)size);
            }
        }
        else
        {
            foreach (string part in text.Split(','))
            {
                int size = int.Parse(part, CultureInfo.InvariantCulture);
                if (size < 1 || size > 4096)
                {
                    throw new ArgumentException("Sizes must be between 1 and 4096.", nameof(text));
                }
                sizes.Add(size);
            }
        }
        return sizes.Distinct().Order().ToArray();
    }
}
