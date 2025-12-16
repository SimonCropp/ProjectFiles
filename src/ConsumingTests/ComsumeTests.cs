[TestFixture]
public class ComsumeTests
{
    [Test]
    public void DefaultProperties()
    {
        IsTrue(Directory.Exists(ProjectFiles.SolutionDirectory), ProjectFiles.SolutionDirectory);
        IsTrue(Directory.Exists(ProjectFiles.ProjectDirectory), ProjectFiles.ProjectDirectory);
        IsTrue(File.Exists(ProjectFiles.SolutionFile), ProjectFiles.SolutionFile);
        IsTrue(File.Exists(ProjectFiles.ProjectFile), ProjectFiles.ProjectFile);
    }

    [Test]
    public void Recursive() =>
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SomeFile_txt));

    [Test]
    public void AtRoot()
    {
        IsTrue(File.Exists(ProjectFiles.fileAtRoot_txt));
        IsTrue(File.Exists(ProjectFiles.globFileAtRoot_txt));
    }

    [Test]
    public void FileIncludeAtRoot() =>
        IsTrue(File.Exists(ProjectFiles.fileIncludeAtRoot_txt));

    [Test]
    public void Content() =>
        IsTrue(File.Exists(ProjectFiles.contentAtRoot_txt));

    [Test]
    public void ContentUpdate() =>
        IsTrue(File.Exists(ProjectFiles.contentUpdateAtRoot_txt));

    [Test]
    public void Specific()
    {
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File1_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File2_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir2.File4_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.File3_txt));
    }

    [Test]
    public void SameNamedDescendants()
    {
        IsTrue(File.Exists(ProjectFiles.SameNamedDescendants.SubDir1.NestedFile_txt));
        IsTrue(File.Exists(ProjectFiles.SameNamedDescendants.SubDir2.NestedFile_txt));
    }

    [Test]
    public void Config() =>
        IsTrue(File.Exists(ProjectFiles.Config.appsettings_json));

    [Test]
    public void LowerCase() =>
        IsTrue(File.Exists(ProjectFiles.lower_case.lower_case_json));

    [Test]
    public void Nested()
    {
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SubDir.NestedFile_txt));
        IsTrue(File.Exists(ProjectFiles.Config.appsettings_json));
    }
}