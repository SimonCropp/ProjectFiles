using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ProjectFiles;

[MemoryDiagnoser]
public class GeneratorBenchmarks
{
    [Params(100, 10000)]
    public int FileCount { get; set; }

    AdditionalText[] texts = null!;
    CSharpCompilation compilation = null!;

    [GlobalSetup]
    public void Setup()
    {
        // the manifest ProjectFiles.props writes: one line per copied file
        var manifest = new StringBuilder();
        for (var index = 0; index < FileCount; index++)
        {
            // mixed case and punctuation so the ordering of members has work to do
            manifest.AppendLine($"File|Fixtures/group-{index % 50}/Sub_{index % 7}/Sample{index}.verified.txt");
        }

        texts = [new Text("obj/ProjectFiles.manifest.txt", manifest.ToString())];
        compilation = CSharpCompilation.Create(
            "Benchmark",
            [],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    // A cold run of the whole generator: manifest parsing, conflict detection, tree building and code generation
    [Benchmark]
    public GeneratorDriver RunGenerator() =>
        CSharpGeneratorDriver
            .Create([new Generator().AsSourceGenerator()], texts)
            .RunGenerators(compilation);

    class Text(string path, string content) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancel = default) =>
            SourceText.From(content, Encoding.UTF8);
    }
}
