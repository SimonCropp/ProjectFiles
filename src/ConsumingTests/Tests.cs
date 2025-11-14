[TestFixture]
public class Tests
{
    [Test]
    public void Recursive() =>
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SomeFileTxt));

    [Test]
    public void Specific()
    {
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File1Txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir1.File2Txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.Dir2.File4Txt));
        IsTrue(File.Exists(ProjectFiles.SpecificDirectory.File3Txt));
    }

    [Test]
    public void Config() =>
        IsTrue(File.Exists(ProjectFiles.Config.AppsettingsJson));

    [Test]
    public void Nested()
    {
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SubDir.NestedFileTxt));
        IsTrue(File.Exists(ProjectFiles.Config.AppsettingsJson));
    }
}