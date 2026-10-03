// The manifest written by ProjectFiles.props. One line per entry:
//   Project|<full path of the project file>
//   Solution|<full path of the solution file>
//   File|<path relative to the output directory>
//   Resource|<project relative path>|<manifest resource name>
record Manifest(
    string? ProjectFile,
    string? SolutionFile,
    ImmutableArray<ProjectItem> Items)
{
    public const string FileName = "ProjectFiles.manifest.txt";

    public static bool IsManifest(string path) =>
        path.EndsWith(FileName, StringComparison.OrdinalIgnoreCase);

    public static Manifest Parse(IEnumerable<string> contents)
    {
        string? projectFile = null;
        string? solutionFile = null;
        var files = new List<ProjectItem>();
        var resources = new List<ProjectItem>();

        foreach (var content in contents)
        {
            foreach (var rawLine in content.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                var separator = line.IndexOf('|');
                if (separator == -1)
                {
                    continue;
                }

                var kind = line.Substring(0, separator);
                var value = line.Substring(separator + 1);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                switch (kind)
                {
                    case "Project":
                        projectFile = value;
                        break;
                    case "Solution":
                        solutionFile = value;
                        break;
                    case "File":
                        files.Add(new(NormalizeSeparators(value), IsEmbeddedResource: false, ResourceName: null));
                        break;
                    case "Resource":
                        // the resource name is the last segment, so a path can contain the separator
                        var nameSeparator = value.LastIndexOf('|');
                        if (nameSeparator > 0 &&
                            nameSeparator < value.Length - 1)
                        {
                            var path = NormalizeSeparators(value.Substring(0, nameSeparator));
                            resources.Add(new(path, IsEmbeddedResource: true, value.Substring(nameSeparator + 1)));
                        }

                        break;
                }
            }
        }

        // A file can be both copied to the output and embedded. It is exposed as the resource.
        var resourcePaths = new HashSet<string>(resources.Select(_ => _.Path));
        var items = files
            .Where(_ => !resourcePaths.Contains(_.Path))
            .Concat(resources)
            .Distinct()
            .ToImmutableArray();

        return new(projectFile, solutionFile, items);
    }

    // Outside Windows a backslash is not a directory separator, and MSBuild can hand over a
    // Link with backslashes unchanged. Normalize so every platform produces the same tree.
    static string NormalizeSeparators(string path) =>
        path.Replace('\\', '/');
}
