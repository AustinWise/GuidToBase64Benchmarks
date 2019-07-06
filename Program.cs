using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace EfficientGuids.Performance
{
    internal class Program
    {
        private static void Main(string[] args) => _ = BenchmarkRunner.Run<GuidExtensionsBenchmarks>();
        //private static void Main(string[] args)
        //{
        //    var guid = Guid.NewGuid();
        //    var v1 = Convert.ToBase64String(guid.ToByteArray()).Replace("/", "-").Replace("+", "_").Replace("=", "");
        //    var v3 = GuidExtensions3.EncodeBase64String(guid);
        //    Console.WriteLine(v1.Equals(v3));
        //    Console.WriteLine(v1);
        //    Console.WriteLine(v3);

        //}
    }

    [MemoryDiagnoser]
    public class GuidExtensionsBenchmarks
    {
        private Guid _guid;

        [GlobalSetup]
        public void Setup() => _guid = Guid.NewGuid();

        [Benchmark(Baseline = true)]
        public string Base64EncodedGuidOriginal() => Convert.ToBase64String(_guid.ToByteArray())
          .Replace("/", "-").Replace("+", "_").Replace("=", "");


        [Benchmark]
        public string Base64EncodedGuid() => GuidExtensions.EncodeBase64String(_guid);


        [Benchmark]
        public string Base64EncodedGuid2() => GuidExtensions2.EncodeBase64String(_guid);

        [Benchmark]
        public string Base64EncodedGuid3() => GuidExtensions2.EncodeBase64String(_guid);
    }
}
