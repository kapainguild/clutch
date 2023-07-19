using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Clutch.Benchmarks
{
    public class Guids
    {
        Guid g1 = Guid.Empty;
        Guid g2 = Guid.Empty;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public bool Equals1() => g1.Equals(g2);

        [Benchmark]
        public bool Equals2() => g1 == g2;
    }




    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ObjectMaterialization>(new Config());
            //BenchmarkRunner.Run<Guids>(new Config());
            BenchmarkRunner.Run<Serialization>(new Config());
            //BenchmarkRunner.Run<NullableComparison>(new Config());
            //BenchmarkRunner.Run<SerializationWithInheritance>(new Config());

            Console.ReadLine();
        }
    }
}