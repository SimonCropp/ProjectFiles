using Microsoft.CodeAnalysis.Diagnostics;

namespace ProjectFiles.Tests;

[TestFixture]
public class GeneratorTest
{
    [Test]
    public Task NoFiles()
    {
        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task NoFilesWithMetadata()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("file1.txt", "content"),
            CreateAdditionalText("file2.json", "content")
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(new Dictionary<string, Dictionary<string, string>>());

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(ImmutableArray.Create(additionalFiles))
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task SingleFileAtRoot()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("config.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "config.json"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(ImmutableArray.Create(additionalFiles))
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MultipleFilesAtRoot()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("appsettings.json", "content"),
            CreateAdditionalText("readme.md", "content"),
            CreateAdditionalText("license.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["readme.md"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "readme.md"
            },
            ["license.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "license.txt"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task FilesInSingleDirectory()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Config/appsettings.json", "content"),
            CreateAdditionalText("Config/database.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Config/appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/appsettings.json"
            },
            ["Config/database.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/database.json"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task FilesInNestedDirectories()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Config/appsettings.json", "content"),
            CreateAdditionalText("Config/Dev/appsettings.dev.json", "content"),
            CreateAdditionalText("Config/Prod/appsettings.prod.json", "content"),
            CreateAdditionalText("Data/seed.sql", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Config/appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/appsettings.json"
            },
            ["Config/Dev/appsettings.dev.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/Dev/appsettings.dev.json"
            },
            ["Config/Prod/appsettings.prod.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/Prod/appsettings.prod.json"
            },
            ["Data/seed.sql"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Data/seed.sql"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MixedRootAndDirectoryFiles()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("readme.md", "content"),
            CreateAdditionalText("Config/appsettings.json", "content"),
            CreateAdditionalText("Scripts/deploy.ps1", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["readme.md"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "readme.md"
            },
            ["Config/appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/appsettings.json"
            },
            ["Scripts/deploy.ps1"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Scripts/deploy.ps1"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task FilesWithSpecialCharacters()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("my-config.json", "content"),
            CreateAdditionalText("app.settings.dev.json", "content"),
            CreateAdditionalText("file_with_underscore.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["my-config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "my-config.json"
            },
            ["app.settings.dev.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "app.settings.dev.json"
            },
            ["file_with_underscore.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "file_with_underscore.txt"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task FilesWithKeywordNames()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("class.json", "content"),
            CreateAdditionalText("namespace.txt", "content"),
            CreateAdditionalText("Config/string.xml", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["class.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "class.json"
            },
            ["namespace.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "namespace.txt"
            },
            ["Config/string.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/string.xml"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task DeepNestedStructure()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Root/Level1/Level2/Level3/deep.txt", "content"),
            CreateAdditionalText("Root/Level1/file1.txt", "content"),
            CreateAdditionalText("Root/file2.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Root/Level1/Level2/Level3/deep.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Root/Level1/Level2/Level3/deep.txt"
            },
            ["Root/Level1/file1.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Root/Level1/file1.txt"
            },
            ["Root/file2.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Root/file2.txt"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task FileExtensionsInPropertyNames()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("config.json", "content"),
            CreateAdditionalText("data.xml", "content"),
            CreateAdditionalText("script.ps1", "content"),
            CreateAdditionalText("readme.md", "content"),
            CreateAdditionalText("file.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "config.json"
            },
            ["data.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "data.xml"
            },
            ["script.ps1"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "script.ps1"
            },
            ["readme.md"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "readme.md"
            },
            ["file.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "file.txt"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(ImmutableArray.Create(additionalFiles))
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task PartialMetadataMatch()
    {
        // Some files have metadata, others don't
        var additionalFiles = new[]
        {
            CreateAdditionalText("has-metadata.json", "content"),
            CreateAdditionalText("no-metadata.json", "content"),
            CreateAdditionalText("also-has-metadata.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["has-metadata.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "has-metadata.json"
            },
            ["no-metadata.json"] = new(), // Empty metadata
            ["also-has-metadata.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "also-has-metadata.txt"
            }
        };

        var optionsProvider = new MockAnalyzerConfigOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts([..additionalFiles])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    static AdditionalText CreateAdditionalText(string path, string content) =>
        new MockAdditionalText(path, content);

    static Compilation CreateCompilation() =>
        CSharpCompilation.Create(
            "TestAssembly",
            [],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    class MockAdditionalText(string path, string text) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(Cancel cancel = default) =>
            SourceText.From(text, Encoding.UTF8);
    }

    class MockAnalyzerConfigOptionsProvider(
        Dictionary<string, Dictionary<string, string>> fileMetadata) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new MockAnalyzerConfigOptions(new());

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
            new MockAnalyzerConfigOptions(new());

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            var options = fileMetadata.TryGetValue(textFile.Path, out var metadata)
                ? metadata
                : new Dictionary<string, string>();

            return new MockAnalyzerConfigOptions(options);
        }
    }

    class MockAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
            options.TryGetValue(key, out value);
    }
}
