using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ProjectFiles;

[MemoryDiagnoser]
public class GeneratorBenchmarks
{
    [Params(100, 10000)]
    public int FileCount { get; set; }

    List<AdditionalText> texts = null!;
    OptionsProvider options = null!;
    CSharpCompilation compilation = null!;

    [GlobalSetup]
    public void Setup()
    {
        texts = [];
        var metadata = new Dictionary<AdditionalText, string>();
        for (var index = 0; index < FileCount; index++)
        {
            // mixed case and punctuation so the ordering of members has work to do
            var path = $"Fixtures/group-{index % 50}/Sub_{index % 7}/Sample{index}.verified.txt";
            var text = new Text(path);
            texts.Add(text);
            metadata.Add(text, path);
        }

        options = new(metadata);
        compilation = CSharpCompilation.Create(
            "Benchmark",
            [],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    // A cold run of the whole generator: conflict detection, tree building and code generation
    [Benchmark]
    public GeneratorDriver RunGenerator() =>
        CSharpGeneratorDriver
            .Create([new Generator().AsSourceGenerator()], texts, optionsProvider: options)
            .RunGenerators(compilation);

    class Text(string path) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancel = default) =>
            SourceText.From("content", Encoding.UTF8);
    }

    class OptionsProvider(Dictionary<AdditionalText, string> files) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(null);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
            GlobalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            new Options(files[textFile]);
    }

    class Options(string? relativePath) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            if (relativePath != null &&
                key == "build_metadata.AdditionalFiles.ProjectFilesGenerator")
            {
                value = relativePath;
                return true;
            }

            value = null!;
            return false;
        }
    }
}
