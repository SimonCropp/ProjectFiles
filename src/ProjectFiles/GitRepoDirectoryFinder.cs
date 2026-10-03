#pragma warning disable RS1035

public static class GitRepoDirectoryFinder
{
    public static string? Find(string projectFile)
    {
        if (!File.Exists(projectFile))
        {
            return null;
        }

        var directory = Directory.GetParent(projectFile);

        while (directory != null)
        {
            var path = directory.FullName;
            var git = Path.Combine(path, ".git");

            // .git is a file in worktrees and submodules
            if (Directory.Exists(git) || File.Exists(git))
            {
                return path;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
