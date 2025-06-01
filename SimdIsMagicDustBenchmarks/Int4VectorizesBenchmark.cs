using BenchmarkDotNet.Attributes;
using SimdIsMagicDust;
using SimdIsMagicDust.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SimdIsMagicDustBenchmarks {
    // * Summary *
    // 
    // BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5487/22H2/2022Update)
    // AMD Ryzen 5 5500U with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
    // .NET SDK 9.0.200
    //   [Host]     : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
    //   DefaultJob : .NET 9.0.2 (9.0.225.6610), X64 RyuJIT AVX2
    // 
    // 
    // | Method             | Mean       | Error    | StdDev   | Code Size |
    // |--------------------|-----------:|---------:|---------:|----------:|
    // | Scalar             | 1,070.8 us | 21.26 us | 36.67 us |      85 B |
    // | Vectorized         |   647.8 us |  4.57 us |  3.82 us |      98 B |
    // | Numerics           |   467.7 us |  7.71 us |  9.47 us |     115 B |
    // | ScalarNoBounds     | 1,253.9 us |  9.35 us |  8.74 us |      51 B |
    // | VectorizedNoBounds |   602.7 us |  5.09 us |  3.97 us |      65 B |

    // Explanation for these numbers:
    // - Scalar uses 32-bit registers
    // - Vectorized uses 128-bit registers on most machines
    // - Numerics uses 256-bit registers on many machines, including my laptop.
    // In an ideal world, this would mean a "8 : 2 : 1" ratio in perf because
    // the math should dominate computation.
    // Of course, there's a bunch of overhead, giving "4 : 3 : 2"'y stats.
    // - ScalarNoBounds has no reason to be slower. Every decompiler I try has
    //   its assembly basically a subset of Scalar, but it just doesn't...
    //   do it?
    // - VectorizedNoBounds does actually get rid of bounds checks with no
    //   other side effects, apparently.
    [DisassemblyDiagnoser]
    public unsafe class Int4VectorizesBenchmark {

        const int size = 1_000_000;

        readonly int[] arr1;
        readonly int[] arr2;

        readonly int4[] vectorArr1;
        readonly int4[] vectorArr2;

        public Int4VectorizesBenchmark() {
            arr1 = new int[size];
            arr2 = new int[size];

            System.Random rng = new();

            for (int i = 0; i < size; i++) {
                arr1[i] = rng.Next();
                arr2[i] = rng.Next();
            }

            // yeh this is bad length is wrong idc
            vectorArr1 = Unsafe.As<int4[]>(arr1);
            vectorArr2 = Unsafe.As<int4[]>(arr2);
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Scalar() {
            for (int i = 0; i < size; i++)
                arr1[i] += arr1[i] * arr2[i];
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Vectorized() {
            for (int i = 0; i < size / 4; i++)
                vectorArr1[i] += vectorArr1[i] * vectorArr2[i];
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Numerics() {
            int limit = size / Vector<int>.Count;
            for (int i = 0; i < limit; i++) {
                Vector<int> v1 = new(arr1, i * Vector<int>.Count);
                Vector<int> v2 = new(arr1, i * Vector<int>.Count);
                v1 += v1 * v2;
                v1.CopyTo(arr1, i * Vector<int>.Count);
            }
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void ScalarNoBounds() {
            for (int i = 0; i < size; i++) {
                ref var v1 = ref arr1.UnsafeGet(i);
                var v2 = arr2.UnsafeGet(i);
                v1 += v1 * v2;
            }
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void VectorizedNoBounds() {
            for (int i = 0; i < size / 4; i++) {
                ref var v1 = ref vectorArr1.UnsafeGet(i);
                var v2 = vectorArr2.UnsafeGet(i);
                v1 += v1 * v2;
            }
        }
    }
}