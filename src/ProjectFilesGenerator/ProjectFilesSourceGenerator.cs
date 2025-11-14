namespace ProjectFilesGenerator;

[Generator]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
public class ProjectFilesSourceGenerator :
    IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all additional files that are .csproj files
        var projectFiles = context.AdditionalTextsProvider
            .Where(_ => _.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        // Parse the project file and extract file paths
        var filePaths = projectFiles
            .Select((file, cancel) =>
            {
                var text = file.GetText(cancel);
                if (text == null)
                {
                    return ImmutableArray<string>.Empty;
                }

                var projectDir = Path.GetDirectoryName(file.Path) ?? string.Empty;
                return ParseProjectFile(text.ToString(), projectDir);
            })
            .Where(_ => _.Length > 0);

        // Generate the source
        context.RegisterSourceOutput(filePaths, (spc, files) =>
        {
            var source = GenerateSource(files);
            spc.AddSource("ProjectFiles.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    static ImmutableArray<string> ParseProjectFile(string content, string projectDir)
    {
        var doc = XDocument.Parse(content);

        var files = new List<string>();

        // Find all None, Content, and other item types with CopyToOutputDirectory
        var itemGroups = doc.Descendants()
            .Where(_ => _.Name.LocalName == "ItemGroup");

        foreach (var itemGroup in itemGroups)
        {
            foreach (var item in itemGroup.Elements())
            {
                var copyToOutput = item.Elements()
                    .FirstOrDefault(_ => _.Name.LocalName == "CopyToOutputDirectory");

                if (copyToOutput?.Value is not ("PreserveNewest" or "Always"))
                {
                    continue;
                }

                var include = item.Attribute("Include")?.Value ?? item.Attribute("Update")?.Value;
                if (string.IsNullOrEmpty(include))
                {
                    continue;
                }

                // Expand glob patterns
                var expanded = ExpandGlobPattern(include!, projectDir);
                if (expanded != null)
                {
                    files.AddRange(expanded);
                }
            }
        }

        return files.Distinct().OrderBy(_ => _).ToImmutableArray();
    }

    static char separatorChar = Path.DirectorySeparatorChar;

    static IEnumerable<string>? ExpandGlobPattern(string pattern, string projectDir)
    {
        // Normalize path separators
        pattern = pattern.Replace('/', separatorChar);

        if (!Directory.Exists(projectDir))
        {
            return null;
        }

        // Check if pattern contains wildcards
        if (pattern.Contains('*') ||
            pattern.Contains('?'))
        {
            var parts = pattern.Split(separatorChar);
            var hasRecursive = parts.Contains("**");

            if (hasRecursive)
            {
                // Handle ** recursive pattern
                var beforeRecursive = string.Join(separatorChar, parts.TakeWhile(_ => _ != "**"));
                var afterRecursive = string.Join(separatorChar, parts.SkipWhile(_ => _ != "**").Skip(1));

                var searchDir = string.IsNullOrEmpty(beforeRecursive)
                    ? projectDir
                    : Path.Combine(projectDir, beforeRecursive);

                if (!Directory.Exists(searchDir))
                {
                    return null;
                }

                var searchPattern = string.IsNullOrEmpty(afterRecursive) ? "*.*" : afterRecursive;

                var found = Directory.GetFiles(searchDir, searchPattern, SearchOption.AllDirectories);
                return found.Select(file => GetRelativePath(projectDir, file));
            }
            else
            {
                // Handle single directory with wildcards
                var dirPart = Path.GetDirectoryName(pattern) ?? string.Empty;
                var filePart = Path.GetFileName(pattern);

                var searchDir = string.IsNullOrEmpty(dirPart)
                    ? projectDir
                    : Path.Combine(projectDir, dirPart);

                if (!Directory.Exists(searchDir))
                {
                    return null;
                }

                var found = Directory.GetFiles(searchDir, filePart);
                return found.Select(file => GetRelativePath(projectDir, file));
            }
        }

        // No wildcards - just return the file if it exists
        var fullPath = Path.Combine(projectDir, pattern);
        return File.Exists(fullPath) ? [pattern] : null;
    }

    static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(EnsureTrailingSlash(basePath));
        var fullUri = new Uri(fullPath);
        var relativeUri = baseUri.MakeRelativeUri(fullUri);
        return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', separatorChar);
    }

    static string EnsureTrailingSlash(string path)
    {
        if (path.EndsWith(separatorChar.ToString()))
        {
            return path;
        }

        return path + separatorChar;
    }

    static string GenerateSource(ImmutableArray<string> files)
    {
        var tree = BuildFileTree(files);
        var builder = new StringBuilder();

        builder.AppendLine(
            """
            // <auto-generated/>
            #nullable enable

            using ProjectFilesGenerator.Types;

            namespace ProjectFilesGenerator
            {
                /// <summary>Provides strongly-typed access to project files marked with CopyToOutputDirectory.</summary>
                public static partial class ProjectFiles
                {
            """);

        GenerateRootProperties(builder, tree);

        builder.AppendLine(
            """
                }
            }

            namespace ProjectFilesGenerator.Types
            {
            """);

        GenerateTypeDefinitions(builder, tree, 0);

        builder.AppendLine("}");

        return builder.ToString();
    }

    static void GenerateRootProperties(StringBuilder builder, FileTreeNode node)
    {
        foreach (var (name, childNode) in node.Children.OrderBy(_ => _.Key))
        {
            if (!childNode.IsDirectory)
            {
                continue;
            }

            var className = ToValidIdentifier(name);
            builder.AppendLine($"        public static {className} {className} {{ get; }} = new();");
        }
    }

    static void GenerateTypeDefinitions(StringBuilder builder, FileTreeNode node, int indentCount)
    {
        var indent = new string(' ', indentCount * 4);

        foreach (var (name, childNode) in node.Children.OrderBy(_ => _.Key))
        {
            if (!childNode.IsDirectory)
            {
                continue;
            }

            var className = ToValidIdentifier(name);

            builder.AppendLine(
                $$"""
                  {{indent}}public partial class {{className}}
                  {{indent}}{
                  """);

            // Generate file properties and subdirectory properties
            GenerateDirectoryMembers(builder, childNode, indentCount + 1);

            builder.AppendLine($"{indent}}}");
        }
    }

    static void GenerateDirectoryMembers(StringBuilder builder, FileTreeNode node, int indentCount)
    {
        var indent = new string(' ', indentCount * 4);

        foreach (var (name, childNode) in node.Children.OrderBy(_ => _.Key))
        {
            if (childNode.IsDirectory)
            {
                var className = ToValidIdentifier(name);
                // generate subdirectory property
                builder.AppendLine($"{indent}public {className}Type {className} {{ get; }} = new();");

                // generate nested type definitions for subdirectory
                builder.AppendLine(
                    $$"""
                      {{indent}}public partial class {{className}}Type
                      {{indent}}{
                      """);

                GenerateDirectoryMembers(builder, childNode, indentCount + 1);

                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }
            else
            {
                // generate file property
                var propertyName = ToFilePropertyName(name);
                var path = childNode.FullPath!.Replace("\\", @"\\");

                builder.AppendLine($"""{indent}public string {propertyName} => "{path}";""");
            }
        }
    }

    static string ToFilePropertyName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        var propertyName = ToValidIdentifier(nameWithoutExtension);

        if (!string.IsNullOrEmpty(extension))
        {
            // Remove the leading dot and make it lowercase
            var extensionWithoutDot = extension.TrimStart('.');
            propertyName += "_" + extensionWithoutDot.ToLowerInvariant();
        }

        return propertyName;
    }

    static FileTreeNode BuildFileTree(ImmutableArray<string> files)
    {
        var root = new FileTreeNode
        {
            IsDirectory = true,
            FullPath = null
        };

        foreach (var file in files)
        {
            var parts = file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var current = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (!current.Children.TryGetValue(part, out var child))
                {
                    var isLast = i == parts.Length - 1;
                    child = new()
                    {
                        IsDirectory = !isLast,
                        FullPath = isLast ? file : null
                    };
                    current.Children[part] = child;
                }

                current = child;
            }
        }

        return root;
    }

    static string ToValidIdentifier(string name)
    {
        var builder = new StringBuilder();
        var capitalizeNext = false;

        foreach (var ch in name)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(capitalizeNext ? char.ToUpperInvariant(ch) : ch);
                capitalizeNext = false;
            }
            else if (ch == '_')
            {
                builder.Append('_');
                capitalizeNext = false;
            }
            else
            {
                // Replace invalid characters with underscore and capitalize next
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    capitalizeNext = true;
                }
            }
        }

        var result = builder.ToString();

        // Ensure it starts with a letter or underscore
        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        // Handle C# keywords
        if (KeywordDetect.IsCSharpKeyword(result))
        {
            result = "@" + result;
        }

        // Capitalize first letter if it's a class/namespace
        if (result.Length > 0)
        {
            result = char.ToUpperInvariant(result[0]) + result[1..];
        }

        return string.IsNullOrEmpty(result) ? "_" : result;
    }

    class FileTreeNode
    {
        public required bool IsDirectory { get; init; }
        public required string? FullPath { get; init; }
        public Dictionary<string, FileTreeNode> Children { get; } = [];
    }
}
