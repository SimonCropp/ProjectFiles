[TestFixture]
public class AddOperatorTests
{
    // begin-snippet: AddOperators
    [Test]
    public void Combinations()
    {
        // ProjectDirectory + string
        var fileInDirectory = ProjectFiles.RecursiveDirectory + "SomeFile.txt";
        AreEqual("RecursiveDirectory/SomeFile.txt", fileInDirectory.Path);
        IsTrue(File.Exists(fileInDirectory));

        // string + ProjectDirectory
        var prefixedDirectory = "Prefix" + ProjectFiles.RecursiveDirectory;
        AreEqual("Prefix/RecursiveDirectory", prefixedDirectory.Path);

        // ProjectDirectory + ProjectDirectory
        var directoryInProject = ProjectFiles.ProjectDirectory + ProjectFiles.RecursiveDirectory;
        IsTrue(Directory.Exists(directoryInProject), directoryInProject.Path);

        // ProjectDirectory + ProjectFile
        var fileInProject = ProjectFiles.ProjectDirectory + ProjectFiles.fileAtRoot_txt;
        IsTrue(File.Exists(fileInProject), fileInProject.Path);

        // string + ProjectFile
        var prefixedFile = "Prefix" + ProjectFiles.RecursiveDirectory.SomeFile_txt;
        AreEqual("Prefix/RecursiveDirectory/SomeFile.txt", prefixedFile.Path);
    }
    // end-snippet

    [Test]
    public void SeparatorHandling()
    {
        AreEqual("a/b", ProjectDirectory.JoinPaths("a", "b"));
        AreEqual("a/b", ProjectDirectory.JoinPaths("a/", "b"));
        AreEqual("a/b", ProjectDirectory.JoinPaths("a", "/b"));
        AreEqual("a/b", ProjectDirectory.JoinPaths("a/", "/b"));
        AreEqual(@"a\b", ProjectDirectory.JoinPaths(@"a\", "b"));
        AreEqual(@"a\b", ProjectDirectory.JoinPaths("a", @"\b"));
        AreEqual("b", ProjectDirectory.JoinPaths("", "b"));
        AreEqual("a", ProjectDirectory.JoinPaths("a", ""));
    }
}
