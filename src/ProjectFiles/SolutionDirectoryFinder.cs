#pragma warning disable RS1035

public static class SolutionDirectoryFinder
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

            if (Directory.Exists(Path.Combine(path, ".git")))
            {
                break;
            }

            var solution = Directory.EnumerateFiles(path, "*.sln*")
                .OrderByDescending(_ => _.Length)
                .FirstOrDefault(_ => _.EndsWith(".slnx") || _.EndsWith(".sln"));

            if (solution != null)
            {
                return solution;
            }

            directory = directory.Parent;
        }

        return null;
    }
}