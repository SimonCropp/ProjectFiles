[TestFixture]
public class GitRepoDirectoryFinderTest
{
    string tempRoot = null!;

    [SetUp]
    public void Setup()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), $"GitRepoFinderTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempRoot);
    }

    [TearDown]
    public void Teardown()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Test]
    public void GitDirectoryInSameDirectory()
    {
        var repoDir = Path.Combine(tempRoot, "repo");
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));

        var projectPath = Path.Combine(repoDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = GitRepoDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(repoDir));
    }

    [Test]
    public void Nested()
    {
        var repoDir = Path.Combine(tempRoot, "repo");
        var projectDir = Path.Combine(repoDir, "src", "MyProject");
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = GitRepoDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(repoDir));
    }

    [Test]
    public void GitFile()
    {
        // worktrees and submodules have a .git file instead of a directory
        var repoDir = Path.Combine(tempRoot, "worktree");
        var projectDir = Path.Combine(repoDir, "src");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(repoDir, ".git"), "gitdir: ../repo/.git/worktrees/worktree");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = GitRepoDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(repoDir));
    }

    [Test]
    public void NearestWins()
    {
        var outerDir = Path.Combine(tempRoot, "outer");
        var innerDir = Path.Combine(outerDir, "inner");
        Directory.CreateDirectory(Path.Combine(outerDir, ".git"));
        Directory.CreateDirectory(Path.Combine(innerDir, ".git"));

        var projectPath = Path.Combine(innerDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = GitRepoDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(innerDir));
    }

    [Test]
    public void ReturnsNullForNonExistentProjectFile()
    {
        var projectPath = Path.Combine(tempRoot, "NonExistent", "MyProject.csproj");

        var result = GitRepoDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Null);
    }
}
