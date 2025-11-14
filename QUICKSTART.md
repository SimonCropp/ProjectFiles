# Quick Start Guide

## Getting Started in 5 Minutes

### Step 1: Add the Generator to Your Project

In your application's `.csproj`, add these two blocks:

```xml
<!-- Reference the generator -->
<ItemGroup>
  <ProjectReference Include="..\ProjectFilesGenerator\ProjectFilesGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>

<!-- Allow the generator to read your .csproj -->
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileFullPath)" />
</ItemGroup>
```

### Step 2: Mark Files to Copy

Add files you want to access with `CopyToOutputDirectory`:

```xml
<ItemGroup>
  <!-- Copy all files from Data directory -->
  <None Update="Data\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  
  <!-- Or copy specific files -->
  <None Update="Config\appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Step 3: Build Your Project

```bash
dotnet build
```

### Step 4: Use the Generated API

**Traditional Dot-Access API:**
```csharp
using ProjectFiles;

// Access your files with IntelliSense!
var configPath = ProjectFiles.ProjectFiles.Config.AppsettingsJson;
var config = File.ReadAllText(configPath);

var dataPath = ProjectFiles.ProjectFiles.Data.UsersCsv;
var users = File.ReadAllLines(dataPath);
```

**New Slash-Operator API (Recommended):**
```csharp
using static GeneratedPaths;

// Compose paths naturally using the / operator
var configPath = Config / Appsettings.Json;
var config = File.ReadAllText(configPath);

var dataPath = Data / Users.Csv;
var users = File.ReadAllLines(dataPath);

// Compose directory paths
var nestedPath = SpecificDirectory / Dir2 / File4.Txt;
// Result: "SpecificDirectory/Dir2/File4.txt"

// Implicit conversion to string works automatically
string path = Assets / Images / Logo.Png;
```

## Common Patterns

### Copy All Files from Multiple Directories

```xml
<ItemGroup>
  <None Update="Assets\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Templates\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Config\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

Usage:
```csharp
var logo = ProjectFiles.ProjectFiles.Assets.Images.LogoPng;
var template = ProjectFiles.ProjectFiles.Templates.EmailWelcomeHtml;
var settings = ProjectFiles.ProjectFiles.Config.AppsettingsJson;
```

### Copy Specific File Types Only

```xml
<ItemGroup>
  <None Update="**\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="**\*.xml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Accessing Nested Files

For this structure:
```
Data/
  Users/
    active.csv
    archived.csv
  Products/
    catalog.json
```

With this config:
```xml
<None Update="Data\**\*.*">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

Access like this:

**Dot-Access API:**
```csharp
var activeUsers = ProjectFiles.ProjectFiles.Data.Users.ActiveCsv;
var archivedUsers = ProjectFiles.ProjectFiles.Data.Users.ArchivedCsv;
var catalog = ProjectFiles.ProjectFiles.Data.Products.CatalogJson;
```

**Slash-Operator API:**
```csharp
using static GeneratedPaths;

var activeUsers = Data / Users / Active.Csv;
var archivedUsers = Data / Users / Archived.Csv;
var catalog = Data / Products / Catalog.Json;
```

## Slash Operator API

The generator creates two APIs for accessing files:

1. **Traditional Dot-Access API** - Backwards compatible nested static classes
2. **Slash-Operator API** - New composition-based approach using the `/` operator

### Benefits of the Slash-Operator API

✅ **More intuitive** - Paths look like actual file paths  
✅ **Concise** - No repetitive class names  
✅ **Composable** - Build paths dynamically  
✅ **Type-safe** - Still strongly-typed with IntelliSense  
✅ **Forward-slash paths** - Uses `/` separator regardless of OS

### How It Works

The generator creates:
- **PathNode struct** - A value type representing a path segment
- **GeneratedPaths class** - Static class with PathNode properties for directories and files
- **File extension classes** - Static classes grouping files by name with extension properties

Example structure:
```csharp
// Directory PathNodes
public static readonly PathNode Config = new PathNode("Config");
public static readonly PathNode Data = new PathNode("Data");

// File extension classes
public static class Users {
    public static PathNode Csv => new PathNode("Users.csv");
}
```

### Using the Slash-Operator API

Add this using statement to your file:
```csharp
using static GeneratedPaths;
```

Then compose paths naturally:
```csharp
// Simple file access
var config = File.ReadAllText(Config / Appsettings.Json);

// Nested directories
var path = Assets / Images / Icons / Logo.Png;

// Assign to string (implicit conversion)
string filePath = Data / Users.Csv;

// Use in any method expecting a string
Console.WriteLine($"Loading: {Config / Settings.Xml}");
```

## Tips

### ✅ DO

- Use the generated properties instead of hardcoded strings
- Organize files into logical directories
- Use glob patterns (`**\*.*`) for entire directories
- Check `File.Exists()` before reading if files might be missing

### ❌ DON'T

- Don't hardcode file paths anymore - use the generated API
- Don't forget to rebuild after adding new files
- Don't use the generator for files that don't need to be copied to output

## Viewing Generated Code

The generator creates two files:
- `ProjectFiles.g.cs` - Traditional dot-access API
- `GeneratedPaths.g.cs` - Slash-operator API with PathNode

### In JetBrains Rider:
1. Right-click your project
2. Click "Generate Code"
3. Select "View Generated Source"
4. Navigate to `ProjectFilesGenerator` → `ProjectFiles.g.cs` or `GeneratedPaths.g.cs`

### In Visual Studio:
1. Solution Explorer → Show All Files
2. Expand "Dependencies" → "Analyzers" → "ProjectFilesGenerator"
3. Open `ProjectFiles.g.cs` or `GeneratedPaths.g.cs`

### Using File System:
Look in: `obj\Debug\net10.0\generated\ProjectFilesGenerator\ProjectFilesGenerator.ProjectFilesSourceGenerator\`
- `ProjectFiles.g.cs` - Dot-access API
- `GeneratedPaths.g.cs` - Slash-operator API

## Troubleshooting

**Q: The generated code isn't updating after I added new files**  
A: Clean and rebuild your solution, or restart your IDE.

**Q: I get a build error "ProjectFiles doesn't exist"**  
A: Ensure you've added both the `<ProjectReference>` and `<AdditionalFiles>` to your .csproj.

**Q: Some files aren't showing up**  
A: Verify the files exist on disk and match your glob pattern. Rebuild to regenerate.

**Q: File names with spaces or special characters?**  
A: They're converted to valid C# identifiers (e.g., `my-file.txt` → `MyFileTxt`).

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Check out [ExampleApp](ExampleApp/) for a working example
- View [ExampleGeneratedOutput.cs](ExampleGeneratedOutput.cs) to see what gets generated

Happy coding! 🚀
