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
    public Task MsBuildPropertiesWithFiles()
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
    public Task PartialMsBuildProperties()
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

    [Test]
    public Task ImplicitUsingsEnabled()
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
            ["build_property.ImplicitUsings"] = "enable"
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
    public Task ImplicitUsingsTrue()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("appsettings.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            }
        };

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.ImplicitUsings"] = "true"
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
    public Task ImplicitUsingsDisabled()
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
            ["build_property.ImplicitUsings"] = "disable"
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
    public Task ImplicitUsingsFalse()
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
            ["build_property.ImplicitUsings"] = "false"
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
    public Task ImplicitUsingsNotSet()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("data.xml", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["data.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "data.xml"
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
    public Task ImplicitUsingsWithMsBuildProperties()
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
            ["build_property.SolutionPath"] = "C:/Projects/MySolution.sln",
            ["build_property.ImplicitUsings"] = "enable"
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
    public Task ImplicitUsingsNoFiles()
    {
        var additionalFiles = Array.Empty<AdditionalText>();

        var globalOptions = new Dictionary<string, string>
        {
            ["build_property.MSBuildProjectDirectory"] = "C:/Dev/WebApi",
            ["build_property.ImplicitUsings"] = "enable"
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
    public Task ContentAtRoot()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("contentAtRoot.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["contentAtRoot.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "contentAtRoot.txt"
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
    public Task ContentInDirectory()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Data/config.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Data/config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Data/config.json"
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
    public Task MixedContentAndNoneItems()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("web.config", "content"),
            CreateAdditionalText("appsettings.json", "content"),
            CreateAdditionalText("Templates/email.html", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["web.config"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "web.config"
            },
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["Templates/email.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/email.html"
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
    public Task ContentWithAlwaysCopyToOutput()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("important.dat", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["important.dat"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "important.dat"
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
    public Task MultipleContentItemsInNestedFolders()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Assets/Images/logo.png", "content"),
            CreateAdditionalText("Assets/Styles/theme.css", "content"),
            CreateAdditionalText("Assets/Scripts/app.js", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Assets/Images/logo.png"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Assets/Images/logo.png"
            },
            ["Assets/Styles/theme.css"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Assets/Styles/theme.css"
            },
            ["Assets/Scripts/app.js"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Assets/Scripts/app.js"
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
    public Task ContentWithoutCopyToOutputDirectoryShouldBeIgnored()
    {
        // Only files with the metadata are included by MSBuild target
        var additionalFiles = new[]
        {
            CreateAdditionalText("included.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["included.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "included.txt"
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
    public Task ContentWithVariousExtensions()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("data.xml", "content"),
            CreateAdditionalText("settings.yaml", "content"),
            CreateAdditionalText("readme.md", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["data.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "data.xml"
            },
            ["settings.yaml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "settings.yaml"
            },
            ["readme.md"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "readme.md"
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
    public Task ContentItemsGenerateCorrectPropertyNames()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("web.config", "content"),
            CreateAdditionalText("app.settings.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["web.config"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "web.config"
            },
            ["app.settings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "app.settings.json"
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
    public Task ContentInMultipleLevelsOfNesting()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Level1/Level2/Level3/deep.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Level1/Level2/Level3/deep.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Level1/Level2/Level3/deep.txt"
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
    public Task ContentAndNoneInSameDirectory()
    {
        var additionalFiles = new[]
        {
            CreateAdditionalText("Config/web.config", "content"),
            CreateAdditionalText("Config/app.json", "content"),
            CreateAdditionalText("Config/database.xml", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Config/web.config"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/web.config"
            },
            ["Config/app.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/app.json"
            },
            ["Config/database.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/database.xml"
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
    public Task ContentWithGlobbingPattern()
    {
        // When using globbing in MSBuild (e.g., Content Include="Templates/**/*.html"),
        // MSBuild expands it to individual files
        var additionalFiles = new[]
        {
            CreateAdditionalText("Templates/email.html", "content"),
            CreateAdditionalText("Templates/Invoice/invoice.html", "content"),
            CreateAdditionalText("Templates/Invoice/Styles/invoice.css", "content"),
            CreateAdditionalText("Templates/Receipt/receipt.html", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Templates/email.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/email.html"
            },
            ["Templates/Invoice/invoice.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/Invoice/invoice.html"
            },
            ["Templates/Invoice/Styles/invoice.css"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/Invoice/Styles/invoice.css"
            },
            ["Templates/Receipt/receipt.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/Receipt/receipt.html"
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
    public Task NestedContentItems()
    {
        // Content items nested within a specific subdirectory structure
        var additionalFiles = new[]
        {
            CreateAdditionalText("wwwroot/assets/images/logo.svg", "content"),
            CreateAdditionalText("wwwroot/assets/images/icons/home.svg", "content"),
            CreateAdditionalText("wwwroot/assets/images/icons/settings.svg", "content"),
            CreateAdditionalText("wwwroot/assets/styles/main.css", "content"),
            CreateAdditionalText("wwwroot/assets/scripts/app.js", "content"),
            CreateAdditionalText("wwwroot/index.html", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["wwwroot/assets/images/logo.svg"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/assets/images/logo.svg"
            },
            ["wwwroot/assets/images/icons/home.svg"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/assets/images/icons/home.svg"
            },
            ["wwwroot/assets/images/icons/settings.svg"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/assets/images/icons/settings.svg"
            },
            ["wwwroot/assets/styles/main.css"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/assets/styles/main.css"
            },
            ["wwwroot/assets/scripts/app.js"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/assets/scripts/app.js"
            },
            ["wwwroot/index.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "wwwroot/index.html"
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
    public Task ContentWithUpdateAttribute()
    {
        // Content items using Update attribute - this only works for files already in @(Content)
        // In web projects, SDK includes files like appsettings*.json in @(Content) by default
        // This test simulates that scenario
        var additionalFiles = new[]
        {
            CreateAdditionalText("appsettings.json", "content"),
            CreateAdditionalText("appsettings.Development.json", "content"),
            CreateAdditionalText("appsettings.Production.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["appsettings.Development.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.Development.json"
            },
            ["appsettings.Production.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.Production.json"
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
    public Task ContentWithIncludeAttribute()
    {
        // Content items using Include attribute - this adds new files to @(Content)
        var additionalFiles = new[]
        {
            CreateAdditionalText("content.txt", "content"),
            CreateAdditionalText("Templates/email.html", "content"),
            CreateAdditionalText("Data/seed.sql", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["content.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "content.txt"
            },
            ["Templates/email.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/email.html"
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
    public Task ContentMixedIncludeAndUpdate()
    {
        // Mix of Content Include (new files) and Update (SDK defaults in web projects)
        var additionalFiles = new[]
        {
            CreateAdditionalText("Templates/email.html", "content"),     // Include (new file)
            CreateAdditionalText("appsettings.json", "content"),         // Update (SDK default in web projects)
            CreateAdditionalText("Data/seed.sql", "content"),            // Include (new file)
            CreateAdditionalText("appsettings.Development.json", "content") // Update (SDK default in web projects)
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Templates/email.html"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Templates/email.html"
            },
            ["appsettings.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.json"
            },
            ["Data/seed.sql"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Data/seed.sql"
            },
            ["appsettings.Development.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "appsettings.Development.json"
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
    public Task NestedDirectoryWithSameName()
    {
        // Directory/Nested/Nested/File.txt - should generate Nested_Level1Type for inner Nested
        var additionalFiles = new[]
        {
            CreateAdditionalText("Directory/Nested/Nested/file.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Directory/Nested/Nested/file.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Directory/Nested/Nested/file.txt"
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
    public Task MultipleNestedDirectoriesWithSameName()
    {
        // Multiple files with nested directory conflicts - all should be generated with suffixes
        var additionalFiles = new[]
        {
            CreateAdditionalText("Data/Data/config.json", "content"),
            CreateAdditionalText("Config/Config/Config/settings.xml", "content"),
            CreateAdditionalText("Valid/Path/file.txt", "content") // This one doesn't need suffixes
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Data/Data/config.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Data/Data/config.json"
            },
            ["Config/Config/Config/settings.xml"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Config/Config/Config/settings.xml"
            },
            ["Valid/Path/file.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Valid/Path/file.txt"
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
    public Task NestedDirectoryConflictCaseInsensitive()
    {
        // Should detect conflict even with different casing and apply suffix
        var additionalFiles = new[]
        {
            CreateAdditionalText("Folder/folder/file.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Folder/folder/file.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Folder/folder/file.txt"
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
    public Task DeeplyNestedWithSameNameAtDifferentLevels()
    {
        // Root/Level1/Level1/Level2/file.txt - conflict at Level1, should get suffix
        var additionalFiles = new[]
        {
            CreateAdditionalText("Root/Level1/Level1/Level2/file.txt", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Root/Level1/Level1/Level2/file.txt"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Root/Level1/Level1/Level2/file.txt"
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
    public Task TriplyNestedSameDirectory()
    {
        // Test/Test/Test/file.txt - should generate Test_Level1Type and Test_Level2Type
        var additionalFiles = new[]
        {
            CreateAdditionalText("Test/Test/Test/data.json", "content")
        };

        var metadata = new Dictionary<string, Dictionary<string, string>>
        {
            ["Test/Test/Test/data.json"] = new()
            {
                ["build_metadata.AdditionalFiles.ProjectFilesGenerator"] = "Test/Test/Test/data.json"
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
    public Task PF0001_CSharp12Required()
    {
        var compilation = CreateCompilation();

        var driver = CSharpGeneratorDriver
            .Create([new Generator().AsSourceGenerator()],
            parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11))
            .RunGenerators(compilation);

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