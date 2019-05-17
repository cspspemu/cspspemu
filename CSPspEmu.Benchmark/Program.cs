using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace CSPspEmu.Benchmark
{
    namespace MyBenchmarks
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                BenchmarkRunner.Run<RasterizerBenchmark>();
                //var config = new ManualConfig();
                //config.Add(MemoryDiagnoser.Default);                    
                //var summary = BenchmarkRunner.Run<Md5VsSha256>(config);
                //var summary = BenchmarkRunner.Run<Md5VsSha256>();
            }
        }

        /*
        public class RasterizerTest
        {
        }

        [MemoryDiagnoser] // we need to enable it in explicit way
        public class Md5VsSha256
        {
            private const int N = 10000;
            private readonly byte[] data;
            
            private readonly byte[] cache = new byte[16];

            private readonly SHA256 sha256 = SHA256.Create();
            private readonly MD5 md5 = MD5.Create();

            public Md5VsSha256()
            {
                data = new byte[N];
                new Random(42).NextBytes(data);
            }

            [Benchmark]
            public byte[] Byte0() => new byte[0];

            [Benchmark]
            public byte[] Byte16() => new byte[16];

            [Benchmark]
            public byte[] Byte16Cache() => cache;

            [Benchmark]
            public byte[] Sha256() => sha256.ComputeHash(data);

            [Benchmark]
            public byte[] Md5() => md5.ComputeHash(data);
        }
        */
    }
}