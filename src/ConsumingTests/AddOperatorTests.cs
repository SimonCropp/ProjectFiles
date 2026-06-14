[TestFixture]
public class AddOperatorTests
{
    // begin-snippet: AddOperators
    [Test]
    public void Combinations()
    {
        // ProjectDirectory + string
        var fileInDirectory = ProjectFiles.RecursiveDirectory + "SomeFile.txt";
        AreEqual("RecursiveDirectory/SomeFile.txt", fileInDirectory);
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

        // ProjectFile + string
        var suffixedFile = ProjectFiles.RecursiveDirectory.SomeFile_txt + "Suffix.txt";
        AreEqual("RecursiveDirectory/SomeFile.txt/Suffix.txt", suffixedFile);

        // string + ProjectFile
        var prefixedFile = "Prefix" + ProjectFiles.RecursiveDirectory.SomeFile_txt;
        AreEqual("Prefix/RecursiveDirectory/SomeFile.txt", prefixedFile.Path);

        // ProjectFile + ProjectDirectory
        var fileThenDirectory = ProjectFiles.fileAtRoot_txt + ProjectFiles.RecursiveDirectory;
        AreEqual("fileAtRoot.txt/RecursiveDirectory", fileThenDirectory.Path);

        // ProjectFile + ProjectFile
        var fileThenFile = ProjectFiles.fileAtRoot_txt + ProjectFiles.globFileAtRoot_txt;
        AreEqual("fileAtRoot.txt/globFileAtRoot.txt", fileThenFile.Path);
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
