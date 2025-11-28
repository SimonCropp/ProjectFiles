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

        var options = new MockOptionsProvider([]);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task AllMsBuildProperties()
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

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp",
            ["build_property.MSBuildProjectFullPath"] = "C:/Projects/MyApp/MyApp.csproj",
            ["build_property.SolutionDir"] = "C:/Projects/",
            ["build_property.SolutionPath"] = "C:/Projects/MySolution.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task OnlyProjectProperties()
    {
        var additionalFiles = Array.Empty<AdditionalText>();

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Dev/WebApi",
            ["build_property.MSBuildProjectFullPath"] = "C:/Dev/WebApi/WebApi.csproj"
        };

        var options = new MockOptionsProvider([], globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task OnlySolutionProperties()
    {
        var additionalFiles = Array.Empty<AdditionalText>();

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.SolutionDir"] = "C:/Source/",
            ["build_property.SolutionPath"] = "C:/Source/MyProduct.sln"
        };

        var options = new MockOptionsProvider([], globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MSBuildPropertiesWithFiles()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("appsettings.json", "content"),
            CreateAdditionalText("Config/database.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["Config/database.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/database.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp",
            ["build_property.MSBuildProjectFullPath"] = "C:/Projects/MyApp/MyApp.csproj",
            ["build_property.SolutionDir"] = "C:/Projects/",
            ["build_property.SolutionPath"] = "C:/Projects/MySolution.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task PartialMSBuildProperties()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("readme.md", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["readme.md"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "readme.md"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Work/Library",
            // ProjectFile missing
            // SolutionDir missing
            ["build_property.SolutionPath"] = "C:/Work/Library.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MSBuildPropertiesWithUnixPaths()
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

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "/home/user/projects/myapp",
            ["build_property.MSBuildProjectFullPath"] = "/home/user/projects/myapp/myapp.csproj",
            ["build_property.SolutionDir"] = "/home/user/projects/",
            ["build_property.SolutionPath"] = "/home/user/projects/mysolution.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictWithProjectDirectory()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectDirectory.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectDirectory.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectDirectory.txt"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictWithProjectFile()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectFile.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectFile.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectFile.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectFullPath"] = "C:/Projects/MyApp/MyApp.csproj"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictWithSolutionDirectory()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("SolutionDirectory/config.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["SolutionDirectory/config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "SolutionDirectory/config.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.SolutionDir"] = "C:/Projects/"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MixedFileAndDirectoryConflicts()
    {
        // Test both file and directory conflicts together
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectFile.txt", "content"), // File conflict
            CreateAdditionalText("SolutionDirectory/config.json", "content"), // Directory conflict
            CreateAdditionalText("appsettings.json", "content") // Valid file
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectFile.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectFile.txt"
            },
            ["SolutionDirectory/config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "SolutionDirectory/config.json"
            },
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectFullPath"] = "C:/Projects/MyApp/MyApp.csproj",
            ["build_property.SolutionDir"] = "C:/Projects/"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictWithSolutionFile()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("SolutionFile.xml", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["SolutionFile.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "SolutionFile.xml"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.SolutionPath"] = "C:/Projects/MySolution.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task MultipleConflicts()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectDirectory.txt", "content"),
            CreateAdditionalText("SolutionFile.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectDirectory.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectDirectory.txt"
            },
            ["SolutionFile.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "SolutionFile.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp",
            ["build_property.SolutionPath"] = "C:/Projects/MySolution.sln"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task NoConflictWhenPropertyNotSet()
    {
        // File named ProjectDirectory is now always reserved, even if MSBuildProjectDirectory isn't set
        // This test should report a conflict
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectDirectory.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectDirectory.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectDirectory.json"
            }
        };

        var options = new MockOptionsProvider(metadata);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task NoConflictInSubdirectory()
    {
        // File named ProjectDirectory in a subdirectory should be fine
        var additionalFiles = new[]
        {
            CreateAdditionalText("Config/ProjectDirectory.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Config/ProjectDirectory.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/ProjectDirectory.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictCaseInsensitive()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("projectdirectory.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["projectdirectory.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "projectdirectory.txt"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
            .RunGenerators(CreateCompilation());

        return Verify(driver);
    }

    [Test]
    public Task ConflictWithOtherValidFiles()
    {
        // When there's a conflict, the conflicting file should be excluded but other files should still generate
        var additionalFiles = new[]
        {
            CreateAdditionalText("ProjectDirectory.txt", "content"),
            CreateAdditionalText("appsettings.json", "content"),
            CreateAdditionalText("Config/database.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["ProjectDirectory.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "ProjectDirectory.txt"
            },
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["Config/database.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/database.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Projects/MyApp"
        };

        var options = new MockOptionsProvider(metadata, globalOptions);

        var driver = CSharpGeneratorDriver
            .Create(new Generator())
            .AddAdditionalTexts(additionalFiles)
            .WithUpdatedAnalyzerConfigOptions(options)
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
            new(OutputKind.DynamicallyLinkedLibrary));
}
