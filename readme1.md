# ProjectFiles Source Generator

An incremental C# source generator that creates a strongly-typed API for accessing files marked with `CopyToOutputDirectory="PreserveNewest"` in your `.csproj` file.

## Features

- 🎯 **Strongly-typed access** to project files
- 🌳 **Hierarchical structure** mirroring your directory layout
- 🔄 **Automatic glob expansion** for patterns like `**\*.*`
- 📝 **IntelliSense support** with XML documentation
- ⚡ **Incremental generation** for fast builds
- 🛡️ **Type-safe** - no magic strings!

## Installation

### 1. Add the Generator to Your Project

Add a project reference to the generator with special settings:

```xml
<ItemGroup>
  <ProjectReference Include="..\ProjectFilesGenerator\ProjectFilesGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. Include Your .csproj as Additional File

This allows the generator to read your project file:

```xml
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileFullPath)" />
</ItemGroup>
```

### 3. Mark Files for Copying

Add your files with `CopyToOutputDirectory`:

```xml
<ItemGroup>
  <!-- Copy all files from a directory recursively -->
  <None Update="Assets\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  
  <!-- Copy specific files -->
  <None Update="Config\appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  
  <None Update="Data\users.csv">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Usage

After building your project, you'll have access to the `ProjectFiles` class:

```csharp
using ProjectFiles;

// Access files with strongly-typed properties
string configPath = ProjectFiles.ProjectFiles.Config.AppsettingsJson;
string usersPath = ProjectFiles.ProjectFiles.Data.UsersCsv;

// Files in subdirectories create nested classes
string iconPath = ProjectFiles.ProjectFiles.Assets.Images.IconPng;

// Read file content
var config = File.ReadAllText(ProjectFiles.ProjectFiles.Config.AppsettingsJson);
```

## Generated Code Structure

For this `.csproj` configuration:

```xml
<ItemGroup>
  <None Update="Config\appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Assets\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

With this file structure:
```
Config/
  appsettings.json
Assets/
  Images/
    icon.png
    logo.svg
  Data/
    users.csv
```

The generator produces:

```csharp
namespace ProjectFiles;

public static partial class ProjectFiles
{
    public static partial class Config
    {
        /// <summary>
        /// Path to 'appsettings.json'.
        /// </summary>
        public static string AppsettingsJson => "Config\\appsettings.json";
    }

    public static partial class Assets
    {
        public static partial class Images
        {
            /// <summary>
            /// Path to 'icon.png'.
            /// </summary>
            public static string IconPng => "Assets\\Images\\icon.png";

            /// <summary>
            /// Path to 'logo.svg'.
            /// </summary>
            public static string LogoSvg => "Assets\\Images\\logo.svg";
        }

        public static partial class Data
        {
            /// <summary>
            /// Path to 'users.csv'.
            /// </summary>
            public static string UsersCsv => "Assets\\Data\\users.csv";
        }
    }
}
```

## Naming Conventions

The generator converts file and directory names to valid C# identifiers:

| File Name | Property Name |
|-----------|--------------|
| `appsettings.json` | `AppsettingsJson` |
| `user-data.csv` | `UserDataCsv` |
| `my.config.xml` | `MyConfigXml` |
| `1-file.txt` | `_1FileTxt` |
| `class.cs` | `@class` (keywords are escaped) |

## Supported Glob Patterns

The generator supports:

- `**\*.*` - All files recursively
- `**\*.json` - All JSON files recursively  
- `Assets\**\*.*` - All files under Assets recursively
- `*.txt` - All .txt files in the directory
- `Data\users-*.csv` - Wildcard matching

## How It Works

1. The generator runs during build and reads your `.csproj` file
2. It finds all items with `CopyToOutputDirectory="PreserveNewest"` or `CopyToOutputDirectory="Always"`
3. Glob patterns are expanded by scanning the file system
4. A tree structure is built representing your directory hierarchy
5. Nested static classes are generated for each directory
6. Properties are created for each file with the relative path as the value

## Benefits

- ✅ **No more magic strings** - refactor-safe file references
- ✅ **Compile-time checking** - missing files cause build errors
- ✅ **IntelliSense** - discover available files while typing
- ✅ **Maintainability** - automatically updates when files change
- ✅ **Documentation** - every property has XML docs with the original filename

## Requirements

- .NET SDK 5.0 or higher
- C# 9.0 or higher (for source generators)
- JetBrains Rider, Visual Studio 2019+, or VS Code with C# extension

## Troubleshooting

### Generator not running?

1. Clean and rebuild your solution
2. Ensure `<AdditionalFiles Include="$(MSBuildThisFileFullPath)" />` is in your `.csproj`
3. Check that the generator project reference has `OutputItemType="Analyzer"`
4. Restart your IDE to refresh the Roslyn workspace

### Files not appearing?

1. Verify files are marked with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`
2. Check that the files actually exist on disk
3. Rebuild the project to regenerate the source

### Want to see the generated code?

In Rider:
- Right-click on your project → Generate Code → View Generated Source

In Visual Studio:
- Project → Show All Files → Dependencies → Analyzers → ProjectFilesGenerator → ProjectFiles.g.cs

Or check: `obj\Debug\net10.0\generated\ProjectFilesGenerator\ProjectFilesGenerator.ProjectFilesSourceGenerator\ProjectFiles.g.cs`

## License

MIT License - feel free to use in your projects!
