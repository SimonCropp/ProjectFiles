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
            
            // Generate the PathNode and GeneratedPaths source
            var pathsSource = GeneratePathsSource(files);
            spc.AddSource("GeneratedPaths.g.cs", SourceText.From(pathsSource, Encoding.UTF8));
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
                files.AddRange(expanded);
            }
        }

        return files.Distinct().OrderBy(_ => _).ToImmutableArray();
    }

    static char separatorChar = Path.DirectorySeparatorChar;

    static IEnumerable<string> ExpandGlobPattern(string pattern, string projectDir)
    {
        // Normalize path separators
        pattern = pattern.Replace('/', separatorChar);

        if (!Directory.Exists(projectDir))
        {
            return [];
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
                    return [];
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
                    return [];
                }

                var foundFiles = Directory.GetFiles(searchDir, filePart);
                return foundFiles.Select(f => GetRelativePath(projectDir, f));
            }
        }

        // No wildcards - just return the file if it exists
        var fullPath = Path.Combine(projectDir, pattern);
        return File.Exists(fullPath) ? [pattern] : [];
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

            /// <summary>
            /// Provides strongly-typed access to project files marked with CopyToOutputDirectory.
            /// </summary>
            public static partial class ProjectFiles
            {
            """);

        GenerateTreeNode(builder, tree, 1);

        builder.AppendLine("}");

        return builder.ToString();
    }

    static string GeneratePathsSource(ImmutableArray<string> files)
    {
        var tree = BuildFileTree(files);
        var builder = new StringBuilder();

        builder.AppendLine(
            """
            // <auto-generated/>
            #nullable enable

            /// <summary>
            /// Represents a path segment that can be composed using the / operator.
            /// </summary>
            public readonly struct PathNode
            {
                /// <summary>
                /// Gets the path value.
                /// </summary>
                public string Value { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="PathNode"/> struct.
                /// </summary>
                /// <param name="value">The path value.</param>
                public PathNode(string value) => Value = value;

                /// <summary>
                /// Returns the string representation of this path node.
                /// </summary>
                public override string ToString() => Value;

                /// <summary>
                /// Implicitly converts a <see cref="PathNode"/> to a string.
                /// </summary>
                public static implicit operator string(PathNode p) => p.Value;

                /// <summary>
                /// Gets an empty path node.
                /// </summary>
                public static PathNode Empty => new PathNode("");

                /// <summary>
                /// Combines two path nodes using the / separator.
                /// </summary>
                public static PathNode operator /(PathNode left, PathNode right)
                    => new PathNode(string.IsNullOrEmpty(left.Value) ? right.Value : left.Value + "/" + right.Value);

                /// <summary>
                /// Combines a path node with a string using the / separator.
                /// </summary>
                public static PathNode operator /(PathNode left, string right)
                    => new PathNode(string.IsNullOrEmpty(left.Value) ? right : left.Value + "/" + right);
            }

            /// <summary>
            /// Provides path composition support using the / operator for project files.
            /// Use with 'using static GeneratedPaths;' to access directory and file path nodes.
            /// </summary>
            public static class GeneratedPaths
            {
            """);

        GeneratePathsContent(builder, tree, 1);

        builder.AppendLine("}");

        return builder.ToString();
    }

    static void GeneratePathsContent(StringBuilder builder, FileTreeNode node, int indentCount)
    {
        var indent = new string(' ', indentCount * 4);

        // Collect all unique directory names and file names
        var directories = new HashSet<string>();
        var filesByName = new Dictionary<string, List<FileTreeNode>>();

        CollectPathNodes(node, directories, filesByName);

        // Generate directory path nodes
        foreach (var dirName in directories.OrderBy(_ => _))
        {
            var identifier = ToValidIdentifier(dirName);
            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// PathNode for the '{dirName}' directory.");
            builder.AppendLine($"{indent}/// </summary>");
            builder.AppendLine($"{indent}public static readonly PathNode {identifier} = new PathNode(\"{dirName}\");");
            builder.AppendLine();
        }

        // Generate file static classes with extension properties
        foreach (var (fileNameWithoutExt, nodes) in filesByName.OrderBy(_ => _.Key))
        {
            var className = ToValidIdentifier(fileNameWithoutExt);
            
            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// File extensions for '{fileNameWithoutExt}'.");
            builder.AppendLine($"{indent}/// </summary>");
            builder.AppendLine($"{indent}public static class {className}");
            builder.AppendLine($"{indent}{{");

            // Group by extension in case there are multiple files with same name but different extensions
            var extensionGroups = nodes.GroupBy(n => Path.GetExtension(n.Name));
            
            foreach (var extGroup in extensionGroups.OrderBy(_ => _.Key))
            {
                var ext = extGroup.Key;
                if (string.IsNullOrEmpty(ext))
                {
                    // File without extension - shouldn't happen but handle it
                    continue;
                }
                
                var extId = ToValidIdentifier(ext);
                var fullFileName = fileNameWithoutExt + ext;

                var innerIndent = indent + "    ";
                builder.AppendLine($"{innerIndent}/// <summary>");
                builder.AppendLine($"{innerIndent}/// PathNode for '{fullFileName}'.");
                builder.AppendLine($"{innerIndent}/// </summary>");
                builder.AppendLine($"{innerIndent}public static PathNode {extId} => new PathNode(\"{fullFileName}\");");
                builder.AppendLine();
            }

            builder.AppendLine($"{indent}}}");
            builder.AppendLine();
        }
    }

    static void CollectPathNodes(FileTreeNode node, HashSet<string> directories, Dictionary<string, List<FileTreeNode>> filesByName)
    {
        foreach (var (name, childNode) in node.Children)
        {
            if (childNode.IsDirectory)
            {
                directories.Add(name);
                CollectPathNodes(childNode, directories, filesByName);
            }
            else
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
                if (!filesByName.TryGetValue(fileNameWithoutExt, out var list))
                {
                    list = [];
                    filesByName[fileNameWithoutExt] = list;
                }
                // Store the full node so we can access the original name later
                list.Add(childNode);
            }
        }
    }

    static FileTreeNode BuildFileTree(ImmutableArray<string> files)
    {
        var root = new FileTreeNode
        {
            Name = "Root",
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
                        Name = part,
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

    static void GenerateTreeNode(StringBuilder builder, FileTreeNode node, int indentCount)
    {
        var indent = new string(' ', indentCount * 4);

        foreach (var (name, childNode) in node.Children.OrderBy(_ => _.Key))
        {
            if (childNode.IsDirectory)
            {
                // Generate nested static class for directory
                var className = ToValidIdentifier(name);
                builder.AppendLine(
                    $$"""
                      {{indent}}/// <summary>
                      {{indent}}/// Files in the '{{name}}' directory.
                      {{indent}}/// </summary>
                      {{indent}}public static partial class {{className}}
                      {{indent}}{
                      """);

                GenerateTreeNode(builder, childNode, indentCount + 1);

                builder.AppendLine($"{indent}}}");
            }
            else
            {
                // Generate property for file
                var propertyName = ToValidIdentifier(Path.GetFileNameWithoutExtension(name));
                var extension = Path.GetExtension(name);
                if (!string.IsNullOrEmpty(extension))
                {
                    propertyName += ToValidIdentifier(extension);
                }

                var path = childNode.FullPath!.Replace("\\", @"\\");

                builder.AppendLine(
                    $"""
                     {indent}public static string {propertyName} => "{path}";
                     """);
            }

            builder.AppendLine();
        }
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
        public required string Name { get; init; }
        public required bool IsDirectory { get; init; }
        public required string? FullPath { get; init; }
        public Dictionary<string, FileTreeNode> Children { get; } = [];
    }
}