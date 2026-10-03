#pragma warning disable RS1035

public static class SolutionDirectoryFinder
{
    public static string? Find(string projectFile)
    {
        try
        {
            return InnerFind(projectFile);
        }
        // An unreadable parent directory should not fail the generator
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    static string? InnerFind(string projectFile)
    {
        if (!File.Exists(projectFile))
        {
            return null;
        }

        var directory = Directory.GetParent(projectFile);

        while (directory != null)
        {
            var path = directory.FullName;

            var solution = FindSolution(path, ".slnx") ?? FindSolution(path, ".sln");

            if (solution != null)
            {
                return solution;
            }

            // Stop at the repository root, after it has been searched.
            // .git is a directory in a normal clone and a file in a worktree or submodule.
            var git = Path.Combine(path, ".git");
            if (Directory.Exists(git) || File.Exists(git))
            {
                break;
            }

            directory = directory.Parent;
        }

        return null;
    }

    static string? FindSolution(string directory, string extension) =>
        // No search pattern: outside Windows a pattern is matched case sensitively
        Directory.EnumerateFiles(directory)
            .Where(_ => _.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => _, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
}
