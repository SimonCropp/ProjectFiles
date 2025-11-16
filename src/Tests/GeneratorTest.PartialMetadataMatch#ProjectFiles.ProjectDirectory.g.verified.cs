//HintName: ProjectFiles.ProjectDirectory.g.cs
namespace ProjectFilesGenerator;

using System.IO;
using System.Collections.Generic;

abstract partial class ProjectDirectory(string path)
{
    public string Path { get; } = path;

    public override string ToString() => Path;

    public static implicit operator string(ProjectDirectory temp) =>
        temp.Path;

    public static implicit operator FileInfo(ProjectDirectory temp) =>
        new(temp.Path);

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