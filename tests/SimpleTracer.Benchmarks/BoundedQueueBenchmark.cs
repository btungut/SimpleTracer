using Ayaz.Internal;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ayaz.Benchmarks
{
    [Config(typeof(Config))]
    [AllStatisticsColumn, IterationsColumn]
    public class BoundedQueueBenchmark
    {
        BoundedQueue<string> _queue;

        [ParamsSource(nameof(TotalCountAndDequeueCountValues))]
        public Tuple<int,int> TotalCountAndDequeueCount { get; set; }
        public static IEnumerable<Tuple<int, int>> TotalCountAndDequeueCountValues()
        {
            yield return new Tuple<int, int>(1000, 1000);
            yield return new Tuple<int, int>(2000, 1000);

            yield return new Tuple<int, int>(10000, 10000);
            yield return new Tuple<int, int>(20000, 10000);

            yield return new Tuple<int, int>(100000, 100000);
            yield return new Tuple<int, int>(200000, 100000);

            yield return new Tuple<int, int>(1000000, 1000000);
            yield return new Tuple<int, int>(2000000, 1000000);

            yield return new Tuple<int, int>(10000000, 10000000);
            yield return new Tuple<int, int>(20000000, 10000000);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            string[] strings = new string[TotalCountAndDequeueCount.Item1];
            Array.Fill(strings, Guid.NewGuid().ToString());
            _queue = new BoundedQueue<string>(strings);
        }

        [Benchmark]
        public void CopyAndClear()
        {
            _queue.Dequeue(TotalCountAndDequeueCount.Item2, BoundedQueueBenchmark<string>.Mode.CopyAndClear);
        }

        [Benchmark]
        public void CreateAndCopy()
        {
            _queue.Dequeue(TotalCountAndDequeueCount.Item2, BoundedQueue<string>.Mode.CreateAndCopy);
        }
    }

    internal class Config : ManualConfig
    {
        public Config()
        {
            AddJob(
                Job.Default
                .WithGcConcurrent(true)
                .WithGcServer(true)
                .WithWarmupCount(3)
                .WithIterationCount(100)
                .WithLaunchCount(1)
                .WithJit(BenchmarkDotNet.Environments.Jit.RyuJit)
                .WithPlatform(BenchmarkDotNet.Environments.Platform.X64)
                .WithRuntime(CoreRuntime.Core31)
                .WithStrategy(RunStrategy.Throughput)
                .WithPowerPlan(PowerPlan.UltimatePerformance)
                
                );

            AddDiagnoser(ThreadingDiagnoser.Default);
            AddDiagnoser(MemoryDiagnoser.Default);

        }
    }
}
