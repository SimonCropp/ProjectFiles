using ProjectFiles;

public static class TestHelper
{
    public static GeneratorDriverRunResult Run(string projectContent, string[] filePaths)
    {
        using var tempDir = new TempDirectory();

        // Write the project file
        var projectPath = Path.Combine(tempDir, "TestProject.csproj");
        File.WriteAllText(projectPath, projectContent);

        // Create all the files referenced in the project
        foreach (var filePath in filePaths)
        {
            var normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(tempDir, normalizedPath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, $"Test content for {Path.GetFileName(fullPath)}");
        }

        // Set up the compilation
        var source = """
                     public class Program
                     {
                         public static void Main()
                         {
                         }
                     }
                     """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestProject",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new(OutputKind.ConsoleApplication));

        // Create the source generator
        var generator = new Generator();

        // Create additional text for the project file
        var additionalText = new TestAdditionalText(projectPath, projectContent);
        
        // Read template files from Templates folder
        var templateDir = Path.Combine(Path.GetDirectoryName(typeof(TestHelper).Assembly.Location)!, "..", "..", "..", "..", "Templates");
        var projectFileTemplatePath = Path.Combine(templateDir, "ProjectFile.cs");
        var projectDirectoryTemplatePath = Path.Combine(templateDir, "ProjectDirectory.cs");
        
        var projectFileTemplateText = File.ReadAllText(projectFileTemplatePath);
        var projectDirectoryTemplateText = File.ReadAllText(projectDirectoryTemplatePath);
        
        var projectFileTemplate = new TestAdditionalText(projectFileTemplatePath, projectFileTemplateText);
        var projectDirectoryTemplate = new TestAdditionalText(projectDirectoryTemplatePath, projectDirectoryTemplateText);

        // Run the generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.AddAdditionalTexts([additionalText, projectFileTemplate, projectDirectoryTemplate]);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        // Get the results
        var runResult = driver.GetRunResult();
        return runResult;
    }

    class TestAdditionalText(string path, string text) :
        AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(Cancel cancel = default) =>
            SourceText.From(text);
    }
}