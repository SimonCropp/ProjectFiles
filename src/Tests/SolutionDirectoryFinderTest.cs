[TestFixture]
public class SolutionDirectoryFinderTest
{
    string tempRoot = null!;

    [SetUp]
    public void Setup()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), $"SolutionFinderTest_{Guid.NewGuid()}");
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
    public void SolutionFoundInSameDirectory()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        Directory.CreateDirectory(solutionDir);

        var solutionPath = Path.Combine(solutionDir, "MySolution.sln");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(solutionDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void SolutionFoundOneDirectoryUp()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "src", "MyProject");
        Directory.CreateDirectory(projectDir);

        var solutionPath = Path.Combine(solutionDir, "MySolution.sln");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void Nested()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "level1", "level2", "level3");
        Directory.CreateDirectory(projectDir);

        var solutionPath = Path.Combine(solutionDir, "MySolution.sln");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void StopsAtGitDirectory()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var gitDir = Path.Combine(solutionDir, "repo");
        var projectDir = Path.Combine(gitDir, "src", "MyProject");
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(gitDir, ".git"));

        var solutionPath = Path.Combine(solutionDir, "MySolution.sln");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindsSlnxFile()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "src");
        Directory.CreateDirectory(projectDir);

        var solutionPath = Path.Combine(solutionDir, "MySolution.slnx");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void PrefersSlnxOverSln()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "src");
        Directory.CreateDirectory(projectDir);

        var slnPath = Path.Combine(solutionDir, "MySolution.slnx");
        File.WriteAllText(slnPath, "");

        var slnxPath = Path.Combine(solutionDir, "MySolution.sln");
        File.WriteAllText(slnxPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(slnPath));
    }

    [Test]
    public void NoSolutionFound()
    {
        var projectDir = Path.Combine(tempRoot, "Project", "src");
        Directory.CreateDirectory(projectDir);

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindsFirstSlnWhenMultipleExist()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "src");
        Directory.CreateDirectory(projectDir);

        var firstSolutionPath = Path.Combine(solutionDir, "AAA.sln");
        File.WriteAllText(firstSolutionPath, "");

        var secondSolutionPath = Path.Combine(solutionDir, "ZZZ.sln");
        File.WriteAllText(secondSolutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.EndWith(".sln"));
    }

    [Test]
    public void SolutionBesideGitDirectory()
    {
        var repoDir = Path.Combine(tempRoot, "repo");
        var projectDir = Path.Combine(repoDir, "src", "MyProject");
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));

        var solutionPath = Path.Combine(repoDir, "MySolution.sln");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void StopsAtGitFile()
    {
        // worktrees and submodules have a .git file instead of a directory
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var worktreeDir = Path.Combine(solutionDir, "worktree");
        var projectDir = Path.Combine(worktreeDir, "src");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(worktreeDir, ".git"), "gitdir: elsewhere");

        File.WriteAllText(Path.Combine(solutionDir, "Unrelated.sln"), "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void PrefersSlnxOverLongerNamedSln()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        var projectDir = Path.Combine(solutionDir, "src");
        Directory.CreateDirectory(projectDir);

        var slnxPath = Path.Combine(solutionDir, "A.slnx");
        File.WriteAllText(slnxPath, "");
        File.WriteAllText(Path.Combine(solutionDir, "LongerName.sln"), "");

        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(slnxPath));
    }

    [Test]
    public void FindsUpperCaseExtension()
    {
        var solutionDir = Path.Combine(tempRoot, "Solution");
        Directory.CreateDirectory(solutionDir);

        var solutionPath = Path.Combine(solutionDir, "APP.SLN");
        File.WriteAllText(solutionPath, "");

        var projectPath = Path.Combine(solutionDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.EqualTo(solutionPath));
    }

    [Test]
    public void ReturnsNullForNonExistentProjectFile()
    {
        var projectPath = Path.Combine(tempRoot, "NonExistent", "MyProject.csproj");

        var result = SolutionDirectoryFinder.Find(projectPath);

        Assert.That(result, Is.Null);
    }
}
