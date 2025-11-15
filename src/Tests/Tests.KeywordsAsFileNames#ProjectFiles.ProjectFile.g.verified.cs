//HintName: ProjectFiles.ProjectFile.g.cs
namespace ProjectFilesGenerator;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


partial class ProjectFile(string path)
{
    public string Path { get; } = path;

    public override string ToString() => Path;

    public static implicit operator string(ProjectFile temp) =>
        temp.Path;

    public static implicit operator FileInfo(ProjectFile temp) =>
        new(temp.Path);

    public FileStream OpenRead() =>
        File.OpenRead(Path);

    public StreamReader OpenText() =>
        File.OpenText(Path);

    public string ReadAllText() =>
        File.ReadAllText(Path);

    public string ReadAllText(Encoding encoding) =>
        File.ReadAllText(Path, encoding);

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
    public FileInfo Info => new(Path);
}