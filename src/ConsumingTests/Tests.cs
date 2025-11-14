using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0169 // Field is never used

[TestFixture]
[SuppressMessage("Performance", "CA1823:Avoid unused private fields")]
public class Tests
{
    static String prop;
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
    public void Number() =>
        IsTrue(File.Exists(ProjectFiles._1StartsWithNumber._1StartsWithNumber_txt));

    [Test]
    public void Keyword() =>
        IsTrue(File.Exists(ProjectFiles.Using.using_txt));
    [Test]
    public void Type() =>
        IsTrue(File.Exists(ProjectFiles.String.String_txt));

    [Test]
    public void With_underscore() =>
        IsTrue(File.Exists(ProjectFiles.With_underscore.With_underscore_txt));

    [Test]
    public void Nested()
    {
        IsTrue(File.Exists(ProjectFiles.RecursiveDirectory.SubDir.NestedFile_txt));
        IsTrue(File.Exists(ProjectFiles.Config.Appsettings_json));
    }
}