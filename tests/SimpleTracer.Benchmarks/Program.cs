using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace Ayaz.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
#endif
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
