using ProjectFilesGenerator;

[TestFixture]
public class ComsumeTests
{
    [Test]
    public void Recursive() =>
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SomeFile_txt));

    [Test]
    public void Specific()
    {
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File1_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File2_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir2.File4_txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.File3_txt));
    }

    [Test]
    public void Config() =>
        IsTrue(File.Exists(ProjectFiles.Config.Appsettings_json));
    [Test]
    public void LowerCase() =>
        IsTrue(File.Exists(ProjectFiles.LowerCase.LowerCase_json));

    [Test]
    public void Nested()
    {
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SubDir.NestedFile_txt));
        IsTrue(File.Exists(ProjectFiles.Config.Appsettings_json));
    }
}