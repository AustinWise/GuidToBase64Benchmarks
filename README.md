# Converting GUIDs to a Base64-like representation

[Steve Gordon][SteveGordon] posted an
[article about converting GUIDs into a base64-like string][OriginalBlogPost]. Later
[Mark Rendle tweeted][MarkRendleTweet] a faster version. My version improves throughput by:

* Less copying of string bytes by directly writing the output in the characters of the string
  using `string.Create`. This is the most interesting part of my improved version, in my opinion.
* Directly encoding to the modified base64 format instead of fixing up the base64 string afterwards.
* Not encoding to UTF-8 and decoding from UTF-8.

The `Base64EncodedGuidStringCreate` method in the table below is my version. `CreateNullString`
is measures how long `new string('\0', 22)` takes, which represents the upper bound on performance
for solving this problem.

## Benchmark Results

``` ini
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-3930K CPU 3.20GHz (Ivy Bridge), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.2.300
  [Host]     : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.5 (CoreCLR 4.6.27617.05, CoreFX 4.6.27618.01), 64bit RyuJIT
```

|                        Method |       Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |-----------:|----------:|----------:|------:|-------:|------:|------:|----------:|
|     Base64EncodedGuidOriginal | 306.172 ns | 0.4677 ns | 0.4375 ns |  1.00 | 0.0429 |     - |     - |     272 B |
|             Base64EncodedGuid | 137.219 ns | 0.2388 ns | 0.2117 ns |  0.45 | 0.0112 |     - |     - |      72 B |
|         Base64EncodedGuidCast | 111.244 ns | 0.1127 ns | 0.0999 ns |  0.36 | 0.0113 |     - |     - |      72 B |
| Base64EncodedGuidStringCreate |  34.306 ns | 0.0823 ns | 0.0687 ns |  0.11 | 0.0114 |     - |     - |      72 B |
|              CreateNullString |   8.873 ns | 0.0260 ns | 0.0231 ns |  0.03 | 0.0114 |     - |     - |      72 B |

[SteveGordon]: https://twitter.com/stevejgordon
[OriginalBlogPost]: https://www.stevejgordon.co.uk/using-high-performance-dotnetcore-csharp-techniques-to-base64-encode-a-guid
[MarkRendleTweet]: https://twitter.com/markrendle/status/1141695153574486019
