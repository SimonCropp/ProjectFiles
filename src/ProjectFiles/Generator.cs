#pragma warning disable RS1035
namespace ProjectFiles;

[Generator]
public class Generator : IIncrementalGenerator
{
    static SourceText projectFileContent;
    static SourceText projectDirectoryContent;
    SourceText globalUsing = SourceText.From("global using ProjectFilesGenerator;\n", Encoding.UTF8);

    static Generator()
    {
        projectFileContent = ReadResouce("ProjectFile");
        projectDirectoryContent = ReadResouce("ProjectDirectory");
    }

    static Assembly assembly = typeof(Generator).Assembly;

    static SourceText ReadResouce(string name)
    {
        using var stream = assembly.GetManifestResourceStream($"ProjectFiles.{name}.cs")!;
        using var reader = new StreamReader(stream);
        return SourceText.From(reader.ReadToEnd(), Encoding.UTF8);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get MSBuild properties
        var msbuildProperties = context
            .AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
            {
                var options = provider.GlobalOptions;
                var projectFile = options.GetValue("build_property.MSBuildProjectFullPath");
                var solutionFile = options.GetValue("build_property.SolutionPath");
                var implicitUsings = options.GetValue("build_property.ImplicitUsings");

                return new MsBuildProperties(
                    projectFile,
                    solutionFile,
                    string.Equals(implicitUsings, "enable", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(implicitUsings, "true", StringComparison.OrdinalIgnoreCase)
                );
            });

        // Get all additional files with CopyToOutputDirectory metadata
        var files = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(pair =>
            {
                var (text, config) = pair;

                var options = config.GetOptions(text);
                if (options.TryGetValue("build_metadata.AdditionalFiles.ProjectFilesGenerator", out var relativePath))
                {
                    return relativePath;
                }

                return null;
            })
            .Where(_ => !string.IsNullOrWhiteSpace(_))
            .Select(_ => _!)
            .Collect();

        var langVersion = context.ParseOptionsProvider
            .Select((p, _) => ((CSharpParseOptions)p).LanguageVersion);

        // Combine files, properties and langversion
        var combined = files.Combine(msbuildProperties.Combine(langVersion));

        // Generate the source
        context.RegisterSourceOutput(
            combined,
            (context, data) =>
            {
                var (fileList, (props, langVersion)) = data;

                if (langVersion < LanguageVersion.CSharp14)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MinLangVersion, Location.None));
                    return;
                }

                // Check for conflicts and report diagnostics
                var reservedConflicts = FindReservedNameConflicts(fileList);

                var conflictingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var (file, property, isDirectory) in reservedConflicts)
                {
                    conflictingFiles.Add(file);
                    var descriptor = isDirectory ? Diagnostics.ReservedDirectoryNameConflict : Diagnostics.ReservedFileNameConflict;
                    var diagnostic = Diagnostic.Create(
                        descriptor,
                        Location.None,
                        file,
                        property);
                    context.ReportDiagnostic(diagnostic);
                }

                // Filter out conflicting files before generating source
                var filteredFiles = fileList.Where(_ => !conflictingFiles.Contains(_)).ToImmutableArray();

                var source = GenerateSource(filteredFiles, props, context.CancellationToken);
                context.AddSource("ProjectFiles.g.cs", SourceText.From(source, Encoding.UTF8));
                context.AddSource("ProjectFiles.ProjectDirectory.g.cs", projectDirectoryContent);
                context.AddSource("ProjectFiles.ProjectFile.g.cs", projectFileContent);

                // Generate global using if ImplicitUsings is enabled
                if (props.ImplicitUsings)
                {
                    context.AddSource("ProjectFiles.GlobalUsings.g.cs", globalUsing);
                }
            });
    }

    static HashSet<string> reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ProjectDirectory",
        "ProjectFile",
        "SolutionDirectory",
        "SolutionFile"
    };

    static List<(string FilePath, string PropertyName, bool IsDirectory)> FindReservedNameConflicts(ImmutableArray<string> files)
    {
        var conflicts = new List<(string, string, bool)>();

        foreach (var file in files)
        {
            var parts = file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (parts.Length <= 0)
            {
                continue;
            }

            var rootName = parts[0];
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(rootName);
            var propertyName = Identifier.Build(nameWithoutExtension);

            if (!reservedNames.Contains(propertyName))
            {
                continue;
            }

            // It's a directory if there are more path parts (subdirectories or files within)
            var isDirectory = parts.Length > 1;
            conflicts.Add((file, propertyName, isDirectory));
        }

        return conflicts;
    }

    static string GenerateSource(ImmutableArray<string> files, MsBuildProperties properties, Cancel cancel)
    {
        var (tree, rootFiles) = BuildFileTree(files, cancel);
        var builder = new StringBuilder();

        builder.AppendLine(
            """
            // <auto-generated/>
            #nullable enable

            namespace ProjectFilesGenerator
            {
                using ProjectFilesGenerator.Types;

                /// <summary>Provides strongly-typed access to project files marked with CopyToOutputDirectory.</summary>
                static partial class ProjectFiles
                {
            """);

        // Generate default properties
        GenerateDefaultProperties(builder, properties);

        if ((rootFiles.Count > 0 || tree.Count > 0) &&
            HasAnyDefaultProperty(properties))
        {
            builder.AppendLine();
        }

        // Generate root-level file properties
        foreach (var filePath in rootFiles.OrderBy(_ => _))
        {
            cancel.ThrowIfCancellationRequested();
            var propertyName = ToFilePropertyName(filePath);
            var path = PathToCSharp(filePath);

            builder.AppendLine($$"""        public static ProjectFile {{propertyName}} { get; } = new({{path}});""");
        }

        if (rootFiles.Count > 0 &&
            tree.Count > 0)
        {
            builder.AppendLine();
        }

        GenerateRootProperties(builder, tree, cancel);

        builder.AppendLine(
            """
                }
            }

            namespace ProjectFilesGenerator.Types
            {
            """);

        GenerateTypeDefinitions(builder, tree, 0, cancel);

        builder.AppendLine("}");

        return builder.ToString();
    }

    static void GenerateDefaultProperties(StringBuilder builder, MsBuildProperties properties)
    {
        if (properties.ProjectFile != null)
        {
            AppendFile(builder, properties.ProjectFile!, "Project");
        }

        var solutionFile = properties.SolutionFile;

        if (solutionFile == null && properties.ProjectFile != null)
        {
            solutionFile = SolutionDirectoryFinder.Find(properties.ProjectFile!);
        }

        if (solutionFile != null)
        {
            AppendFile(builder, solutionFile!, "Solution");
        }
    }

    static void AppendFile(StringBuilder builder, string file, string prefix)
    {
        var directory = Directory.GetParent(file)!;
        var directoryCSharp = PathToCSharp($"{directory.FullName}/");
        builder.AppendLine($$"""        public static ProjectDirectory {{prefix}}Directory { get; } = new({{directoryCSharp}});""");
        var fileCSharp = PathToCSharp(file);
        builder.AppendLine($$"""        public static ProjectFile {{prefix}}File { get; } = new({{fileCSharp}});""");
    }

    static bool HasAnyDefaultProperty(MsBuildProperties properties) =>
        !string.IsNullOrWhiteSpace(properties.ProjectFile) ||
        !string.IsNullOrWhiteSpace(properties.SolutionFile);

    static void GenerateRootProperties(StringBuilder builder, IReadOnlyCollection<DirectoryNode> topLevelNodes, Cancel cancel)
    {
        foreach (var node in topLevelNodes.OrderBy(_ => _.Path))
        {
            cancel.ThrowIfCancellationRequested();

            var className = Identifier.Build(Path.GetFileName(node.Path));
            builder.AppendLine($"        public static {className}Type {className} {{ get; }} = new();");
        }
    }

    static void GenerateTypeDefinitions(StringBuilder builder, IReadOnlyCollection<DirectoryNode> topLevelNodes, int indentCount, Cancel cancel)
    {
        var indent = new string(' ', indentCount * 4);

        foreach (var node in topLevelNodes.OrderBy(_ => _.Path))
        {
            cancel.ThrowIfCancellationRequested();

            var className = Identifier.Build(Path.GetFileName(node.Path));
            var pathString = PathToCSharp(node.Path);
            builder.AppendLine(
                $$"""
                  {{indent}}partial class {{className}}Type() : ProjectDirectory({{pathString}})
                  {{indent}}{
                  """);

            GenerateDirectoryMembers(builder, node, indentCount + 1, cancel);

            builder.AppendLine($"{indent}}}");
        }
    }

    static void GenerateDirectoryMembers(StringBuilder builder, DirectoryNode node, int indentCount, Cancel cancel)
    {
        var indent = new string(' ', indentCount * 4);

        // Get the parent class name for conflict detection
        var parentClassName = Identifier.Build(Path.GetFileName(node.Path));

        // Generate subdirectory properties first
        foreach (var (name, childNode) in node.Directories.OrderBy(_ => _.Key))
        {
            cancel.ThrowIfCancellationRequested();

            var baseClassName = Identifier.Build(name);
            var className = baseClassName;

            // Check if this subdirectory name matches the parent directory name
            if (string.Equals(baseClassName, parentClassName, StringComparison.OrdinalIgnoreCase))
            {
                // Conflict detected - use depth-based suffix
                className = $"{baseClassName}_Level{childNode.Depth}";
            }

            // generate subdirectory property
            builder.AppendLine($"{indent}public {className}Type {baseClassName} {{ get; }} = new();");

            // generate nested type definitions for subdirectory
            builder.AppendLine(
                $$"""
                  {{indent}}public partial class {{className}}Type
                  {{indent}}{
                  """);

            GenerateDirectoryMembers(builder, childNode, indentCount + 1, cancel);

            builder.AppendLine($"{indent}}}");
            builder.AppendLine();
        }

        // Generate file properties
        foreach (var filePath in node.Files.OrderBy(_ => _))
        {
            var propertyName = ToFilePropertyName(filePath);
            var path = PathToCSharp(filePath);

            builder.AppendLine($$"""{{indent}}public ProjectFile {{propertyName}} { get; } = new({{path}});""");
        }
    }

    static string PathToCSharp(string filePath)
    {
        var path = filePath.Replace('\\', '/');
        return $"\"{path}\"";
    }

    static string ToFilePropertyName(string filePath)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        var propertyName = Identifier.Build(nameWithoutExtension);

        if (!string.IsNullOrEmpty(extension))
        {
            // Remove the leading dot and make it lowercase
            var extensionWithoutDot = extension.TrimStart('.');
            propertyName += "_" + extensionWithoutDot.ToLowerInvariant();
        }

        return propertyName;
    }

    static (IReadOnlyCollection<DirectoryNode> Directories, List<string> RootFiles) BuildFileTree(ImmutableArray<string> files, Cancel cancel)
    {
        var topLevelDirectories = new Dictionary<string, DirectoryNode>();
        var rootFiles = new List<string>();

        foreach (var file in files)
        {
            cancel.ThrowIfCancellationRequested();

            var parts = file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Handle files at the root of the project
            if (parts.Length < 2)
            {
                rootFiles.Add(file);
                continue;
            }

            // Get or create top-level directory
            var topLevelName = parts[0];
            if (!topLevelDirectories.TryGetValue(topLevelName, out var topLevelNode))
            {
                topLevelNode = new()
                {
                    Path = topLevelName,
                    Depth = 0
                };
                topLevelDirectories[topLevelName] = topLevelNode;
            }

            var current = topLevelNode;
            var currentPath = topLevelName;

            // Navigate through middle directories
            for (var i = 1; i < parts.Length - 1; i++)
            {
                cancel.ThrowIfCancellationRequested();
                var part = parts[i];
                currentPath = currentPath + Path.DirectorySeparatorChar + part;

                if (!current.Directories.TryGetValue(part, out var child))
                {
                    child = new()
                    {
                        Path = currentPath,
                        Depth = i
                    };
                    current.Directories[part] = child;
                }

                current = child;
            }

            // Add file to current directory
            current.Files.Add(file);
        }

        return (topLevelDirectories.Values, rootFiles);
    }
}