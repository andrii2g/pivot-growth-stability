namespace PivotGrowth.Core.Fixtures;

/// <summary>SplitMix64 with a fixed bit-to-binary64 mapping, independent of runtime version.</summary>
public sealed class DeterministicRng(ulong seed)
{
    private ulong state = seed;

    public ulong NextUInt64()
    {
        unchecked
        {
            ulong value = (state += 0x9E3779B97F4A7C15UL);
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }

    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
    public double NextSignedDouble() => 2 * NextDouble() - 1;
}
