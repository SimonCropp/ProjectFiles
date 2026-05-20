namespace ProjectFilesGenerator;

partial class ProjectDirectory
{
    /// <summary>
    /// Recursively enumerates all files in this directory and subdirectories.
    /// </summary>
    public IEnumerable<string> EnumerateFilesRecursively(string searchPattern = "*") =>
        Directory.EnumerateFiles(Path, searchPattern, SearchOption.AllDirectories);

    /// <summary>
    /// Combines this directory path with additional path segments.
    /// </summary>
    public string Combine(params string[] paths) =>
        System.IO.Path.Combine([Path, .. paths]);
}