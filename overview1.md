# ProjectFiles Source Generator - Project Overview

## 📁 Solution Structure

```
ProjectFilesGenerator.sln
├── ProjectFilesGenerator/              (Source Generator Project)
│   ├── ProjectFilesGenerator.csproj    
│   └── ProjectFilesSourceGenerator.cs  (Main generator implementation)
│
├── ExampleApp/                         (Example Usage Project)
│   ├── ExampleApp.csproj               (Shows how to reference the generator)
│   ├── Program.cs                      (Usage examples)
│   ├── Config/
│   │   └── appsettings.json
│   ├── RecursiveDirectory/
│   │   ├── SomeFile.txt
│   │   └── SubDir/
│   │       └── NestedFile.txt
│   └── SpecificDirectory/
│       ├── File3.txt
│       ├── Dir1/
│       │   ├── File1.txt
│       │   └── File2.txt
│       └── Dir2/
│           └── File4.txt
│
├── README.md                           (Full documentation)
├── QUICKSTART.md                       (5-minute getting started guide)
├── ExampleGeneratedOutput.cs           (Sample of what gets generated)
└── .gitignore                          (Git ignore rules)
```

## 🚀 What This Does

This source generator automatically creates a strongly-typed API for accessing files that are marked with `CopyToOutputDirectory="PreserveNewest"` in your `.csproj` file.

### Before (Traditional Approach)
```csharp
// ❌ Error-prone magic strings
var config = File.ReadAllText("Config/appsettings.json");
var users = File.ReadAllText("Data/users.csv");

// ❌ No compile-time checking
// ❌ No IntelliSense
// ❌ Breaks when files are moved/renamed
```

### After (With This Generator)
```csharp
// ✅ Strongly-typed, refactor-safe
var config = File.ReadAllText(ProjectFiles.ProjectFiles.Config.AppsettingsJson);
var users = File.ReadAllText(ProjectFiles.ProjectFiles.Data.UsersCsv);

// ✅ Full IntelliSense support
// ✅ Compile-time checking
// ✅ Automatically updates when files change
```

## 📚 Documentation Files

### [QUICKSTART.md](QUICKSTART.md) - Start Here!
**5-minute setup guide** with common patterns and examples. Perfect if you want to get up and running quickly.

### [README.md](README.md) - Complete Documentation  
**Comprehensive guide** covering:
- Installation steps
- How it works internally
- Generated code structure
- Naming conventions
- Glob pattern support
- Troubleshooting
- Benefits and use cases

### [ExampleGeneratedOutput.cs](ExampleGeneratedOutput.cs)
**Sample of generated code** showing exactly what the generator produces for the example project.

## 🎯 Key Features

1. **Incremental Generation** - Fast, efficient builds
2. **Glob Pattern Support** - Handles `**\*.*` and other wildcards
3. **Hierarchical API** - Nested classes mirror directory structure
4. **XML Documentation** - Every property has helpful docs
5. **Name Sanitization** - Converts filenames to valid C# identifiers
6. **Keyword Escaping** - Handles reserved words like `class`, `namespace`, etc.

## 🛠️ Requirements

- **.NET 5.0+** (Source generators requirement)
- **C# 9.0+** (For init-only properties and records)
- **JetBrains Rider** / Visual Studio 2019+ / VS Code with C# extension

## 📖 Usage Example

Given this `.csproj` configuration:

```xml
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileFullPath)" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\ProjectFilesGenerator\ProjectFilesGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>

<ItemGroup>
  <None Update="Assets\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

You can access files like this:

```csharp
using ProjectFiles;

// Direct access with IntelliSense
string logoPath = ProjectFiles.ProjectFiles.Assets.Images.LogoPng;
string iconPath = ProjectFiles.ProjectFiles.Assets.Icons.FaviconIco;
string dataPath = ProjectFiles.ProjectFiles.Assets.Data.ConfigJson;

// Use in your code
var logo = Image.Load(ProjectFiles.ProjectFiles.Assets.Images.LogoPng);
var config = JsonSerializer.Deserialize<Config>(
    File.ReadAllText(ProjectFiles.ProjectFiles.Assets.Data.ConfigJson)
);
```

## 🔍 How It Works

1. **Build-Time Analysis** - Generator runs during compilation
2. **Project File Parsing** - Reads your `.csproj` to find marked files
3. **Glob Expansion** - Scans filesystem to resolve patterns like `**\*.*`
4. **Tree Building** - Creates hierarchical structure of directories/files
5. **Code Generation** - Produces nested static classes with properties
6. **Incremental Updates** - Only regenerates when files or patterns change

## 📦 Project Files

### ProjectFilesGenerator.csproj
```xml
- Target: netstandard2.0 (required for analyzers)
- Packages: Microsoft.CodeAnalysis.CSharp 4.8.0
- Purpose: Analyzer/generator that runs at build time
```

### ExampleApp.csproj  
```xml
- Target: net10.0 (your target framework)
- Reference: ProjectFilesGenerator as analyzer
- Purpose: Demonstrates generator usage
```

## 🎓 Learning Path

1. **Quick Start** → Read [QUICKSTART.md](QUICKSTART.md) (5 min)
2. **Try It Out** → Open ExampleApp and build it
3. **View Generated Code** → Check generated `ProjectFiles.g.cs`
4. **Deep Dive** → Read full [README.md](README.md) when needed

## 💡 Pro Tips

### Tip 1: Organize by Purpose
```xml
<None Update="Config\**\*.*" CopyToOutputDirectory="PreserveNewest" />
<None Update="Assets\**\*.*" CopyToOutputDirectory="PreserveNewest" />
<None Update="Templates\**\*.*" CopyToOutputDirectory="PreserveNewest" />
```
Results in clean API: `ProjectFiles.Config.*`, `ProjectFiles.Assets.*`, `ProjectFiles.Templates.*`

### Tip 2: Use Specific Patterns
```xml
<!-- Only copy JSON files -->
<None Update="**\*.json" CopyToOutputDirectory="PreserveNewest" />
```

### Tip 3: Combine with Path.Combine for Subfolders
```csharp
// If you need to build paths dynamically
var basePath = Path.GetDirectoryName(ProjectFiles.ProjectFiles.Assets.Images.LogoPng);
var dynamicPath = Path.Combine(basePath, "runtime-generated.png");
```

### Tip 4: Check Existence Before Use
```csharp
var path = ProjectFiles.ProjectFiles.Config.OptionalJson;
if (File.Exists(path))
{
    var config = File.ReadAllText(path);
}
```

## 🐛 Common Issues

**Issue**: Generator not running  
**Fix**: Clean solution, rebuild, restart IDE

**Issue**: Files not appearing  
**Fix**: Check glob pattern, verify files exist, rebuild

**Issue**: Can't see generated code  
**Fix**: Check `obj\Debug\net10.0\generated\` folder

**Issue**: Namespace conflicts  
**Fix**: Generated code is in `ProjectFiles` namespace - adjust usings if needed

## 🤝 Contributing Ideas

Some ideas for extending this generator:

- Support for embedded resources
- Option to customize namespace
- Generate file hash/checksum constants
- Support for localized resources
- Integration with file watchers
- MSBuild task for validation

## 📞 Support

For issues with the generator:
1. Check [QUICKSTART.md](QUICKSTART.md) troubleshooting section
2. Review [README.md](README.md) for detailed docs
3. Examine the generated code in `obj\Debug\...\generated\`
4. Clean and rebuild solution

## 🎉 Success Criteria

You'll know it's working when:
- ✅ Build succeeds without errors
- ✅ IntelliSense shows `ProjectFiles.ProjectFiles.*` 
- ✅ Generated code appears in `obj\Debug\...\generated\`
- ✅ You can navigate to generated properties (F12 in Rider/VS)
- ✅ Changing .csproj files triggers regeneration

## 📄 License

MIT License - Use freely in your projects!

---

**Ready to get started?** Open [QUICKSTART.md](QUICKSTART.md) and be up and running in 5 minutes! 🚀
