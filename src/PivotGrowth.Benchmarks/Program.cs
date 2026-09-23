using BenchmarkDotNet.Running;
using PivotGrowth.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(GaussianSolverBenchmarks).Assembly).Run(args);
