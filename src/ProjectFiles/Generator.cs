#pragma warning disable RS1035
namespace ProjectFiles;

[Generator]
public class Generator : IIncrementalGenerator
{
    static SourceText projectFileContent;
    static SourceText projectDirectoryContent;
    static SourceText embeddedResourceContent;
    SourceText globalUsing = SourceText.From("global using ProjectFilesGenerator;\n", Encoding.UTF8);

    static Generator()
    {
        projectFileContent = ReadResouce("ProjectFile");
        projectDirectoryContent = ReadResouce("ProjectDirectory");
        embeddedResourceContent = ReadResouce("EmbeddedResource");
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

        // Get all additional files with CopyToOutputDirectory or EmbeddedResource metadata
        var files = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(pair =>
            {
                var (text, config) = pair;

                var options = config.GetOptions(text);

                // CopyToOutputDirectory files
                if (options.TryGetValue("build_metadata.AdditionalFiles.ProjectFilesGenerator", out var relativePath) &&
                    !string.IsNullOrWhiteSpace(relativePath))
                {
                    return new ProjectItem(NormalizeSeparators(relativePath), IsEmbeddedResource: false, ResourceName: null);
                }

                // Embedded resources
                if (options.TryGetValue("build_metadata.AdditionalFiles.ProjectFilesEmbeddedResource", out var resourcePath) &&
                    !string.IsNullOrWhiteSpace(resourcePath) &&
                    options.TryGetValue("build_metadata.AdditionalFiles.ProjectFilesEmbeddedResourceName", out var resourceName) &&
                    !string.IsNullOrWhiteSpace(resourceName))
                {
                    return new ProjectItem(NormalizeSeparators(resourcePath), IsEmbeddedResource: true, resourceName);
                }

                return null;
            })
            .Where(_ => _ is not null)
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

                var conflictingFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var conflict in FindReservedNameConflicts(fileList))
                {
                    conflictingFiles.Add(conflict.FilePath);
                    var descriptor = conflict.IsDirectory ? Diagnostics.ReservedDirectoryNameConflict : Diagnostics.ReservedFileNameConflict;
                    var diagnostic = Diagnostic.Create(
                        descriptor,
                        Location.None,
                        conflict.FilePath,
                        conflict.PropertyName);
                    context.ReportDiagnostic(diagnostic);
                }

                foreach (var conflict in FindDuplicatePropertyNames(fileList))
                {
                    conflictingFiles.Add(conflict.File1);
                    conflictingFiles.Add(conflict.File2);
                    var diagnostic = Diagnostic.Create(
                        Diagnostics.DuplicatePropertyName,
                        Location.None,
                        conflict.File1,
                        conflict.File2,
                        conflict.PropertyName);
                    context.ReportDiagnostic(diagnostic);
                }

                var conflictingDirectories = new List<string>();

                foreach (var conflict in FindMemberNameConflicts(fileList))
                {
                    foreach (var member in new[] {conflict.Existing, conflict.Added})
                    {
                        if (member == null)
                        {
                            continue;
                        }

                        if (member.IsFile)
                        {
                            conflictingFiles.Add(member.Path);
                        }
                        else
                        {
                            conflictingDirectories.Add($"{member.Path}/");
                        }
                    }

                    var diagnostic = Diagnostic.Create(
                        Diagnostics.DuplicateMemberName,
                        Location.None,
                        conflict.Description,
                        conflict.Added.Path,
                        conflict.Name);
                    context.ReportDiagnostic(diagnostic);
                }

                // Filter out conflicting files before generating source
                var filteredFiles = fileList
                    .Where(_ => !conflictingFiles.Contains(_.Path) &&
                                !conflictingDirectories.Any(directory => _.Path.StartsWith(directory, StringComparison.Ordinal)))
                    .ToList();

                var source = GenerateSource(filteredFiles, props, context.CancellationToken);
                context.AddSource("ProjectFiles.g.cs", SourceText.From(source, Encoding.UTF8));
                context.AddSource("ProjectFiles.ProjectDirectory.g.cs", projectDirectoryContent);
                context.AddSource("ProjectFiles.ProjectFile.g.cs", projectFileContent);

                // Only emit the EmbeddedResource base class when at least one is in use
                if (filteredFiles.Any(_ => _.IsEmbeddedResource))
                {
                    context.AddSource("ProjectFiles.EmbeddedResource.g.cs", embeddedResourceContent);
                }

                // Generate global using if ImplicitUsings is enabled
                if (props.ImplicitUsings)
                {
                    context.AddSource("ProjectFiles.GlobalUsings.g.cs", globalUsing);
                }
            });
    }

    // Outside Windows a backslash is not a directory separator, and MSBuild can hand over a
    // Link with backslashes unchanged. Normalize so every platform produces the same tree.
    static string NormalizeSeparators(string path) =>
        path.Replace('\\', '/');

    static HashSet<string> reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ProjectDirectory",
        "ProjectFile",
        "SolutionDirectory",
        "SolutionFile",
        "GitRepoDirectory"
    };

    static IEnumerable<ReservedNameConflict> FindReservedNameConflicts(ImmutableArray<ProjectItem> files)
    {
        foreach (var item in files)
        {
            var file = item.Path;
            var parts = file.Split('/');

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
            yield return new(file, propertyName, isDirectory);
        }
    }

    static IEnumerable<DuplicateProperty> FindDuplicatePropertyNames(ImmutableArray<ProjectItem> files)
    {
        // Group files by their directory (same scope)
        var filesByDirectory = new Dictionary<string, List<string>>();

        foreach (var item in files)
        {
            var file = item.Path;
            var directory = Path.GetDirectoryName(file)!;
            if (filesByDirectory.TryGetValue(directory, out var filesInDir))
            {
                filesInDir.Add(file);
            }
            else
            {
                filesByDirectory.Add(directory, [file]);
            }
        }

        // Check for duplicates within each directory
        foreach (var filesInDir in filesByDirectory.Values)
        {
            var propertyToFile = new Dictionary<string, string>();

            foreach (var file in filesInDir)
            {
                var propertyName = ToFilePropertyName(file);

                if (propertyToFile.TryGetValue(propertyName, out var existingFile))
                {
                    yield return new(existingFile, file, propertyName);
                }
                else
                {
                    propertyToFile[propertyName] = file;
                }
            }
        }
    }

    // Finds clashes that FindDuplicatePropertyNames does not cover: a directory against a file,
    // a directory against another directory, and any member against the type it is declared in.
    static IEnumerable<MemberNameConflict> FindMemberNameConflicts(ImmutableArray<ProjectItem> files)
    {
        // scope (directory path, empty for the root) => member name => what generates it
        var scopes = new Dictionary<string, Dictionary<string, GeneratedMember?>>();
        var reported = new HashSet<(string?, string)>();

        foreach (var item in files)
        {
            var parts = item.Path.Split('/');
            var scope = "";

            for (var index = 0; index < parts.Length; index++)
            {
                if (!scopes.TryGetValue(scope, out var members))
                {
                    members = [];
                    scopes.Add(scope, members);

                    // member names cannot be the same as their enclosing type
                    var enclosingType = index == 0 ? "ProjectFiles" : DirectoryTypeName(parts, index - 1);
                    members.Add(enclosingType, null);
                }

                var path = scope.Length == 0 ? parts[index] : $"{scope}/{parts[index]}";
                var isFile = index == parts.Length - 1;
                var member = new GeneratedMember(path, isFile);

                string[] names = isFile
                    ? [ToFilePropertyName(path)]
                    // a directory generates a property and a type
                    : [Identifier.Build(parts[index]), DirectoryTypeName(parts, index)];

                foreach (var name in names)
                {
                    // @class and class are the same identifier
                    var key = name.TrimStart('@');

                    if (!members.TryGetValue(key, out var existing))
                    {
                        members.Add(key, member);
                        continue;
                    }

                    // the same directory seen again, or two files (reported as PROJFILES004)
                    if (existing == member ||
                        existing is {IsFile: true} && isFile)
                    {
                        continue;
                    }

                    // two directories clash on both the property and the type; report the pair once
                    if (!reported.Add((existing?.Path, path)))
                    {
                        continue;
                    }

                    var description = existing?.Path ?? $"the enclosing type {key}";
                    yield return new(description, existing, member, key);
                }

                scope = path;
            }
        }
    }

    // The name of the type generated for the directory at parts[index]
    static string DirectoryTypeName(string[] parts, int index)
    {
        var name = Identifier.Build(parts[index]);

        // A directory with the same name as its parent gets a depth-based suffix
        if (index > 0 &&
            string.Equals(name, Identifier.Build(parts[index - 1]), StringComparison.OrdinalIgnoreCase))
        {
            name = $"{name}_Level{index}";
        }

        return $"{name}Type";
    }

    // Members every generated type inherits. A file or directory that generates one of these
    // names hides the inherited member, which needs the "new" modifier to avoid CS0108.
    static HashSet<string> objectMembers =
    [
        "Equals",
        "GetHashCode",
        "GetType",
        "ToString",
        "MemberwiseClone",
        "ReferenceEquals",
        "Finalize"
    ];

    static HashSet<string> directoryMembers =
    [
        ..objectMembers,
        "Path",
        "FullPath",
        "Info",
        "JoinPaths",
        "EnumerateDirectories",
        "EnumerateFiles",
        "GetFiles",
        "GetDirectories"
    ];

    static string NewModifier(HashSet<string> inheritedMembers, string name) =>
        inheritedMembers.Contains(name.TrimStart('@')) ? "new " : "";

    static string GenerateSource(IEnumerable<ProjectItem> files, MsBuildProperties properties, Cancel cancel)
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
        foreach (var item in rootFiles.OrderBy(_ => _.Path))
        {
            cancel.ThrowIfCancellationRequested();
            builder.AppendLine(FilePropertyDeclaration("        ", isStatic: true, item));
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
            AppendFile(builder, solutionFile, "Solution");
        }

        if (properties.ProjectFile != null)
        {
            var gitRepoDirectory = GitRepoDirectoryFinder.Find(properties.ProjectFile!);
            if (gitRepoDirectory != null)
            {
                var directoryCSharp = PathToCSharp($"{gitRepoDirectory}/");
                builder.AppendLine($$"""        public static ProjectDirectory GitRepoDirectory { get; } = new({{directoryCSharp}});""");
            }
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
            var newModifier = NewModifier(objectMembers, className);
            builder.AppendLine($"        public {newModifier}static {className}Type {className} {{ get; }} = new();");
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
            var newModifier = NewModifier(directoryMembers, baseClassName);
            builder.AppendLine($"{indent}public {newModifier}{className}Type {baseClassName} {{ get; }} = new();");

            // generate nested type definitions for subdirectory
            var pathString = PathToCSharp(childNode.Path);
            builder.AppendLine(
                $$"""
                  {{indent}}public partial class {{className}}Type() : ProjectDirectory({{pathString}})
                  {{indent}}{
                  """);

            GenerateDirectoryMembers(builder, childNode, indentCount + 1, cancel);

            builder.AppendLine($"{indent}}}");
            builder.AppendLine();
        }

        // Generate file properties
        foreach (var item in node.Files.OrderBy(_ => _.Path))
        {
            builder.AppendLine(FilePropertyDeclaration(indent, isStatic: false, item));
        }
    }

    static string FilePropertyDeclaration(string indent, bool isStatic, ProjectItem item)
    {
        var propertyName = ToFilePropertyName(item.Path);
        var staticModifier = isStatic ? "static " : "";
        // root files are static members of ProjectFiles, the rest are members of a ProjectDirectory
        staticModifier = NewModifier(isStatic ? objectMembers : directoryMembers, propertyName) + staticModifier;

        if (item.IsEmbeddedResource)
        {
            var name = StringToCSharp(item.ResourceName!);
            return $$"""{{indent}}public {{staticModifier}}EmbeddedResource {{propertyName}} { get; } = new({{name}});""";
        }

        var path = PathToCSharp(item.Path);
        return $$"""{{indent}}public {{staticModifier}}ProjectFile {{propertyName}} { get; } = new({{path}});""";
    }

    static string StringToCSharp(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
        return $"\"{escaped}\"";
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

        // Handle files that start with a dot (like ".txt")
        if (string.IsNullOrEmpty(nameWithoutExtension) && !string.IsNullOrEmpty(extension))
        {
            var fileName = Path.GetFileName(filePath);
            return Identifier.Build(fileName);
        }

        if (string.IsNullOrEmpty(extension))
        {
            return Identifier.Build(nameWithoutExtension);
        }

        // Remove the leading dot and make it lowercase.
        // Build from the combined text so the extension is sanitized too (eg "Dockerfile.linux-arm64").
        var extensionWithoutDot = extension.TrimStart('.').ToLowerInvariant();
        return Identifier.Build($"{nameWithoutExtension}_{extensionWithoutDot}");
    }

    static (IReadOnlyCollection<DirectoryNode> Directories, List<ProjectItem> RootFiles) BuildFileTree(IEnumerable<ProjectItem> files, Cancel cancel)
    {
        var topLevelDirectories = new Dictionary<string, DirectoryNode>();
        var rootFiles = new List<ProjectItem>();

        foreach (var item in files)
        {
            cancel.ThrowIfCancellationRequested();

            var file = item.Path;
            var parts = file.Split('/');

            // Handle files at the root of the project
            if (parts.Length < 2)
            {
                rootFiles.Add(item);
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
                currentPath = currentPath + '/' + part;

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
            current.Files.Add(item);
        }

        return (topLevelDirectories.Values, rootFiles);
    }
}
