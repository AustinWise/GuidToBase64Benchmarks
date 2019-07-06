using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace EfficientGuids.Performance
{
    internal class Program
    {
        private static void Main(string[] args) => _ = BenchmarkRunner.Run<GuidExtensionsBenchmarks>();
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
        public string Base64EncodedGuidImproved() => GuidExtensionsImproved.EncodeBase64String(_guid);

        [Benchmark]
        public string Base64EncodedGuidAustin() => GuidExtensionsAustin.EncodeBase64String(_guid);

        [Benchmark]
        public string CreateNullString() => new string('\0', 22);
    }
}
