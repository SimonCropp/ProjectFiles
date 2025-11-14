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

```csharp
using ProjectFiles;

// Access your files with IntelliSense!
var configPath = ProjectFiles.ProjectFiles.Config.AppsettingsJson;
var config = File.ReadAllText(configPath);

var dataPath = ProjectFiles.ProjectFiles.Data.UsersCsv;
var users = File.ReadAllLines(dataPath);
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
```csharp
var activeUsers = ProjectFiles.ProjectFiles.Data.Users.ActiveCsv;
var archivedUsers = ProjectFiles.ProjectFiles.Data.Users.ArchivedCsv;
var catalog = ProjectFiles.ProjectFiles.Data.Products.CatalogJson;
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

### In JetBrains Rider:
1. Right-click your project
2. Click "Generate Code"
3. Select "View Generated Source"
4. Navigate to `ProjectFilesGenerator` → `ProjectFiles.g.cs`

### In Visual Studio:
1. Solution Explorer → Show All Files
2. Expand "Dependencies" → "Analyzers" → "ProjectFilesGenerator"
3. Open `ProjectFiles.g.cs`

### Using File System:
Look in: `obj\Debug\net10.0\generated\ProjectFilesGenerator\ProjectFilesGenerator.ProjectFilesSourceGenerator\ProjectFiles.g.cs`

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
