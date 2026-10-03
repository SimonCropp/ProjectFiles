using BenchmarkDotNet.Attributes;
using ProjectFilesGenerator;

[MemoryDiagnoser]
public class ReadAllBytesBenchmarks
{
    [Params("small.txt", "large.txt")]
    public string Resource { get; set; } = null!;

    EmbeddedResource resource = null!;

    [GlobalSetup]
    public void Setup() =>
        resource = new(Resource);

    // The implementation before the single allocation change, kept as the baseline
    [Benchmark(Baseline = true)]
    public byte[] ViaMemoryStream()
    {
        using var stream = resource.OpenRead();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    [Benchmark]
    public byte[] ReadAllBytes() =>
        resource.ReadAllBytes();
}
