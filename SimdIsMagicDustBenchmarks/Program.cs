using BenchmarkDotNet.Running;
using System;

namespace SimdIsMagicDustBenchmarks {
    internal class Program {
        static void Main(string[] args) {
            // Extremely Proper Programming:
            // just swap this out with whatever you want to benchmark lmao
            Console.WriteLine(
                BenchmarkRunner.Run<Int4VectorizesBenchmark>()
            );
        }
    }
}
