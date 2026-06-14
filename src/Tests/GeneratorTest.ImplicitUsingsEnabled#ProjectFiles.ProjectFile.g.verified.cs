//HintName: ProjectFiles.ProjectFile.g.cs
namespace ProjectFilesGenerator;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

partial class ProjectFile(string path)
{
    public string Path { get; } = path;

    public string FullPath => System.IO.Path.GetFullPath(Path);

    public override string ToString() => Path;

    public static implicit operator string(ProjectFile temp) =>
        temp.Path;

    public static implicit operator FileInfo(ProjectFile temp) =>
        new(temp.Path);

    public static string operator +(ProjectFile file, string suffix) =>
        ProjectDirectory.JoinPaths(file.Path, suffix);

    public static ProjectFile operator +(string prefix, ProjectFile file) =>
        new(ProjectDirectory.JoinPaths(prefix, file.Path));

    public static ProjectFile operator +(ProjectFile prefix, ProjectFile suffix) =>
        new(ProjectDirectory.JoinPaths(prefix.Path, suffix.Path));

    public FileStream OpenRead() =>
        File.OpenRead(Path);

    public StreamReader OpenText() =>
        File.OpenText(Path);

    public string ReadAllText() =>
        File.ReadAllText(Path);

    public string ReadAllText(Encoding encoding) =>
        File.ReadAllText(Path, encoding);

    public FileInfo Info => new(Path);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public Task<string> ReadAllTextAsync(CancellationToken cancel = default) =>
        File.ReadAllTextAsync(Path, cancel);

    public Task<string> ReadAllTextAsync(Encoding encoding, CancellationToken cancel = default) =>
        File.ReadAllTextAsync(Path, encoding,cancel);
#else
    public Task<string> ReadAllTextAsync(CancellationToken cancel = default) =>
        Task.FromResult(File.ReadAllText(Path));

    public Task<string> ReadAllTextAsync(Encoding encoding, CancellationToken cancel = default) =>
        Task.FromResult(File.ReadAllText(Path, encoding));
#endif
}