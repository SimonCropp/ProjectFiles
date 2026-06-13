//HintName: ProjectFiles.ProjectDirectory.g.cs
namespace ProjectFilesGenerator;

using System.IO;
using System.Collections.Generic;

partial class ProjectDirectory(string path)
{
    public string Path { get; } = path;

    public string FullPath => System.IO.Path.GetFullPath(Path);

    public override string ToString() => Path;

    public static implicit operator string(ProjectDirectory temp) =>
        temp.Path;

    public static implicit operator FileInfo(ProjectDirectory temp) =>
        new(temp.Path);

    public static string operator +(ProjectDirectory directory, string suffix) =>
        JoinPaths(directory.Path, suffix);

    public static ProjectDirectory operator +(string prefix, ProjectDirectory directory) =>
        new(JoinPaths(prefix, directory.Path));

    public static ProjectDirectory operator +(ProjectDirectory parent, ProjectDirectory child) =>
        new(JoinPaths(parent.Path, child.Path));

    public static ProjectFile operator +(ProjectDirectory directory, ProjectFile file) =>
        new(JoinPaths(directory.Path, file.Path));

    public static ProjectDirectory operator +(ProjectFile file, ProjectDirectory directory) =>
        new(JoinPaths(file.Path, directory.Path));

    internal static string JoinPaths(string left, string right)
    {
        if (left.Length == 0)
        {
            return right;
        }

        if (right.Length == 0)
        {
            return left;
        }

        var leftEndsWithSeparator = left[left.Length - 1] is '/' or '\\';
        var rightStartsWithSeparator = right[0] is '/' or '\\';

        if (leftEndsWithSeparator && rightStartsWithSeparator)
        {
            return left.TrimEnd('/', '\\') + right;
        }

        if (leftEndsWithSeparator || rightStartsWithSeparator)
        {
            return left + right;
        }

        return left + "/" + right;
    }

    public IEnumerable<string> EnumerateDirectories() =>
        Directory.EnumerateDirectories(Path);

    public IEnumerable<string> EnumerateFiles() =>
        Directory.EnumerateFiles(Path);

    public IEnumerable<string> GetFiles() =>
        Directory.GetFiles(Path);

    public IEnumerable<string> GetDirectories() =>
        Directory.GetDirectories(Path);

    public DirectoryInfo Info => new(Path);
}