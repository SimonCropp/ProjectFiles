# ProjectFiles review findings

Review of the generator, the MSBuild props and the templates at commit `2494baa` (package `1.4.0`), done on 2026-10-03.

How each finding was checked is stated per item. Three methods were used:

- **Real build**: throwaway projects built with SDK 10.0.401 against the Release analyzer DLL and `buildTransitive/ProjectFiles.props`, or against the packed `nugets/ProjectFiles.1.4.0.nupkg`.
- **Generator harness**: a console app driving `Generator` through `CSharpGeneratorDriver`, then compiling the output and listing compiler diagnostics.
- **Inspection**: read from the code only. These are marked as such.

The throwaway projects lived in a temporary folder and are not part of the repo. The inputs are listed per finding so each case can be recreated. Nothing in the repo was changed. Suggested fixes are proposals and have not been tested.

## Summary

| Done | # | Finding | Kind | Checked by |
|---|---|---|---|---|
| [ ] | 1 | `LogicalName` is ignored for embedded resources | wrong output | real build |
| [ ] | 2 | Culture-suffixed embedded resources point at nothing | wrong output | real build |
| [ ] | 3 | `#` or `;` in a path truncates the value | wrong output | real build |
| [ ] | 4 | SDK default content (Web, Worker) is skipped | missing output | real build (Web), inspection (Worker) |
| [ ] | 5 | `IfDifferent`, `TargetPath` and absolute includes are mishandled | wrong or missing output | real build |
| [ ] | 6 | `SolutionDirectoryFinder` misses or picks the wrong solution | wrong output | generator harness |
| [ ] | 7 | Analyzer needs Roslyn 5.9 but ships as `roslyn5.0` / `roslyn5.3` | packaging | real build, part inferred |
| [ ] | 8 | File extension is not sanitised | compile break | generator harness |
| [ ] | 9 | Identifier collisions are not detected | compile break | generator harness, real build |
| [ ] | 10 | Nested directory types are not `ProjectDirectory` | API | generator harness |
| [ ] | 11 | `ProjectDirectory` converts implicitly to `FileInfo` | API | inspection |
| [ ] | 12 | Reserved-name check is broader than needed | diagnostics | generator harness |
| [ ] | 13 | Backslash `Link` produces a different API on Linux | wrong output | real build in Docker |
| [ ] | P1 | Editing a copied data file recompiles the project | perf | real build with control |
| [ ] | P2 | `EmbeddedResource.ReadAllBytes` buffers twice | perf | inspection |
| [ ] | P3 | `ReadAllTextAsync` is synchronous on older targets | perf | inspection |
| [ ] | P4 | Culture-sensitive ordering of generated members | perf, stability | generator harness |

## Wrong or missing output, no diagnostic

### 1. `LogicalName` is ignored for embedded resources

Where: [ProjectFiles.props line 82](src/ProjectFiles/buildTransitive/ProjectFiles.props#L82).

Input:

```xml
<EmbeddedResource Include="Resources\logical.txt" LogicalName="Custom.Name" />
```

Observed:

- The assembly contains one manifest resource, named `Custom.Name`.
- The generated property is `public EmbeddedResource logical_txt { get; } = new("App3.Resources.logical.txt");`.
- `ProjectFiles.Resources.logical_txt.ReadAllText()` throws `InvalidOperationException: Could not find embedded resource 'App3.Resources.logical.txt'.`

`%(ManifestResourceName)` does not reflect `LogicalName`; the compiler uses `LogicalName` when it is set. The readme (line 219) and the comment in the props both say `LogicalName` is honoured.

This matters for the adoption list in `todo.md`: several of the embedded-resource candidates there use `LogicalName`.

Suggested fix: `ProjectFilesEmbeddedResourceName="$([MSBuild]::ValueOrDefault('%(EmbeddedResource.LogicalName)', '%(EmbeddedResource.ManifestResourceName)'))"`.

### 2. Culture-suffixed embedded resources point at nothing

Where: [ProjectFiles.props lines 83 to 86](src/ProjectFiles/buildTransitive/ProjectFiles.props#L83).

Input:

```xml
<EmbeddedResource Include="Resources\text.fr.txt" />
```

Observed:

- MSBuild treats `fr` as a culture and puts the file into the satellite assembly `fr/App1.resources.dll`.
- The main assembly holds no resource for it.
- The generator still emits `public EmbeddedResource text_fr_txt { get; } = new("App1.Resources.text.txt");`, so `OpenRead()` cannot find it.

Any file name with a segment that parses as a culture is affected, for example `help.de.txt` or `data.id.json`.

Suggested fix: add `AND '%(EmbeddedResource.WithCulture)' != 'true'` to the condition. `PrepareResourceNames` has already run `SplitResourcesByCulture` at that point, so the metadata is set.

### 3. `#` or `;` in a path truncates the value

Where: every value the props pass through the generated `.editorconfig`: [ProjectFiles.props lines 27, 42, 81, 82](src/ProjectFiles/buildTransitive/ProjectFiles.props#L27) and [lines 108, 109](src/ProjectFiles/buildTransitive/ProjectFiles.props#L108). Read in [Generator.cs lines 36, 37, 58, 65](src/ProjectFiles/Generator.cs#L36).

Cause: MSBuild writes the full value into the generated `.editorconfig`, but the Roslyn editorconfig parser ends a value at the first `#` or `;` and treats the rest as a comment.

Observed:

| Input | Generated | Reality |
|---|---|---|
| `<None Update="Docs\C#\note.txt" CopyToOutputDirectory="PreserveNewest" />` | `ProjectFiles.Docs.C` with path `"Docs/C"` | file is copied to `Docs/C#/note.txt` |
| project at `...\msb\C#Repo\App2\App2.csproj` | `ProjectFile = new(".../msb/C")`, `ProjectDirectory = new(".../msb/")` | `File.Exists(ProjectFiles.ProjectFile)` is `False` at runtime |
| project at `...\msb\Semi;Colon\App6\App6.csproj` | `ProjectFile = new(".../msb/Semi")` | same mechanism |

A repo checked out under a folder named `C#` is the realistic case. `SolutionPath` goes through the same route (not built).

Suggested fix: either encode `#` and `;` in the props and decode in the generator, or move to the single manifest file described in P1, which avoids the editorconfig value parser for paths altogether.

### 4. SDK default content (Web, Worker) is skipped

Where: [ProjectFiles.props lines 35, 36](src/ProjectFiles/buildTransitive/ProjectFiles.props#L35) and [lines 50, 51](src/ProjectFiles/buildTransitive/ProjectFiles.props#L50).

The filter accepts an item only when `DefiningProjectFullPath` is the project file or ends with `Microsoft.NET.Sdk.DefaultItems.props`.

Input, in a `Microsoft.NET.Sdk.Web` project (this is the scenario the readme documents under "Include vs Update"):

```xml
<Content Update="appsettings.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
<None Update="notes.txt" CopyToOutputDirectory="PreserveNewest" />
```

Observed:

- Both files are copied to the output directory.
- Only `notes_txt` is generated. There is no property for `appsettings.json`.
- `dotnet msbuild -getItem:Content` reports the item's `DefiningProjectFullPath` as `...\Sdks\Microsoft.NET.Sdk.StaticWebAssets\Sdk\Sdk.StaticWebAssets.StaticAssets.ProjectSystem.props`. `Update` does not change the defining project.

By inspection, not built:

- The Worker SDK defines `**\*.json` and `**\*.config` content with `CopyToOutputDirectory="PreserveNewest"` in `Microsoft.NET.Sdk.Worker.props`, so it fails the same condition.
- Items declared in `Directory.Build.props`, `Directory.Build.targets` or any other imported file fail it too.

Suggested fix: accept items defined by any file under the SDK's `Sdks` folder instead of matching one file name. `$(MSBuildSDKsPath)` is one candidate under `dotnet build`; check what it resolves to under Visual Studio.

### 5. `IfDifferent`, `TargetPath` and absolute includes are mishandled

Where: [ProjectFiles.props lines 26 to 53](src/ProjectFiles/buildTransitive/ProjectFiles.props#L26).

| Input | Observed |
|---|---|
| `<None Update="different.txt" CopyToOutputDirectory="IfDifferent" />` | File is copied to the output. No property is generated. Only `PreserveNewest` and `Always` are matched (lines 30, 31, 45, 46). |
| `<None Update="configs\prod.json" CopyToOutputDirectory="PreserveNewest" TargetPath="appsettings.json" />` | File lands at `appsettings.json`. Generated `ProjectFiles.configs.prod_json` has path `"configs/prod.json"`, which does not exist in the output. |
| `<None Include="$(MSBuildProjectDirectory)\Abs\abs.txt" CopyToOutputDirectory="PreserveNewest" />` | File lands at `Abs/abs.txt`. Generated code has a root type `partial class _Type() : ProjectDirectory("C:")` with nested `Users`, `simon` and so on down to `Abs.abs_txt`, holding the full absolute path. |
| `both.json` as `None` with copy and also as `EmbeddedResource` | Only `EmbeddedResource both_json` is generated. The copied file gets no `ProjectFile`. |

By inspection: only `None` and `Content` are scanned for `CopyToOutputDirectory`. MSBuild also copies `EmbeddedResource` and `Compile` items that carry it, and those get no `ProjectFile`. One candidate in `todo.md` (`SetStartupProjects\src\Tests`) declares its fixtures that way.

Suggested fix: add `IfDifferent` to both conditions, and take the path from the `TargetPath` that `AssignTargetPaths` computes instead of rebuilding it from `Link` or `RelativeDir`.

### 6. `SolutionDirectoryFinder` misses or picks the wrong solution

Where: [SolutionDirectoryFinder.cs lines 18 to 25](src/ProjectFiles/SolutionDirectoryFinder.cs#L18).

This code only runs when MSBuild supplies no `SolutionPath`, for example `dotnet build path/to/Project.csproj`.

| Layout | Expected | `Find` returned |
|---|---|---|
| `repo/.git/`, `repo/App.sln`, `repo/src/App/App.csproj` | `repo/App.sln` | `null` |
| `three/Unrelated.sln`, `three/worktree/.git` (a file), `three/worktree/src/App.csproj` | `null` | `three/Unrelated.sln` |
| `A.slnx`, `LongerName.sln` in one folder | `A.slnx` | `LongerName.sln` |
| `APP.SLN` beside the project | `APP.SLN` | `null` |

Causes:

- The `.git` check (line 18) runs before the folder is searched, so the loop stops without looking in the git root. A solution at the repo root is the most common layout. The existing test `StopsAtGitDirectory` only covers a solution above the git root.
- `Directory.Exists` does not see a `.git` file, which is what worktrees and submodules have.
- `OrderByDescending(_ => _.Length)` (line 24) prefers the longest path, not `.slnx`. The test `PrefersSlnxOverSln` passes only because both files share the base name `MySolution`.
- `EndsWith(".sln")` (line 25) is case-sensitive and culture-sensitive.

Consequence: `SolutionDirectory` and `SolutionFile` are not generated on project-level builds of repos that keep the solution at the git root.

Suggested fix: search the folder first, then stop at `.git` (file or folder). Pick `.slnx` then `.sln` explicitly using `StringComparison.OrdinalIgnoreCase`. Wrap the file system calls so an inaccessible parent folder cannot fail the generator.

### 7. Analyzer needs Roslyn 5.9 but ships as `roslyn5.0` / `roslyn5.3`

Where: [Directory.Packages.props lines 11, 12](src/Directory.Packages.props#L11), [ProjectFiles.csproj lines 22, 23](src/ProjectFiles/ProjectFiles.csproj#L22), [ProjectFiles.props line 5](src/ProjectFiles/buildTransitive/ProjectFiles.props#L5).

Observed:

- Both analyzer builds reference `Microsoft.CodeAnalysis 5.9.0.0` and `Microsoft.CodeAnalysis.CSharp 5.9.0.0`.
- The package places them in `analyzers/dotnet/roslyn5.0/cs` (netstandard2.0) and `analyzers/dotnet/roslyn5.3/cs` (net10.0).
- The props and the readme state a minimum SDK of 10.0.100.
- Compiler shipped by each SDK on this machine: 8.0.425 has 4.11, 9.0.318 has 4.14, 10.0.401 has 5.9, 11.0.100-rc.1 has 5.11.
- Building with SDK 9.0.318 and the analyzer DLL gives: `warning CS9057: Analyzer assembly '...ProjectFiles.dll' cannot be used because it references version '5.9.0.0' of the compiler, which is newer than the currently running version '4.14.0.0'.` The analyzer is then dropped.

Inferred, not reproduced: SDK bands 10.0.1xx to 10.0.3xx ship a compiler older than 5.9 (none is installed here). They pass `CheckSdkVersion`, then drop the analyzer with CS9057, and every `ProjectFiles.` reference fails to compile. An IDE that hosts an older Roslyn has the same problem.

Each routine bump of `Microsoft.CodeAnalysis.CSharp` raises this floor without any visible sign, since CI only runs the SDK pinned in `global.json`.

Suggested fix: pin `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Analyzers` to the lowest compiler version to be supported (per target framework if both folders are kept) and exclude them from routine bumps. Otherwise raise `MinimumSdkVersion`, the folder names and the readme to match what is referenced.

### 13. Backslash `Link` produces a different API on Linux

Where: [Generator.cs line 162](src/ProjectFiles/Generator.cs#L162) and [line 457](src/ProjectFiles/Generator.cs#L457) split on `Path.DirectorySeparatorChar` and `Path.AltDirectorySeparatorChar`, which are both `/` outside Windows. `ToFilePropertyName` and `FindDuplicatePropertyNames` use `Path.GetFileName` style calls with the same limitation.

The same project was built against `ProjectFiles.1.4.0.nupkg` on Windows and in the `mcr.microsoft.com/dotnet/sdk:10.0` Linux container, SDK 10.0.401 on both.

| `Link` value | Windows | Linux |
|---|---|---|
| `Assets\Images\logo.txt`, no `Assets` folder on disk | `ProjectFiles.Assets.Images.logo_txt` | `ProjectFiles.Assets_Images_logo_txt` |
| `Existing\Sub\other.txt`, `Existing` folder exists | `ProjectFiles.Existing.Sub.other_txt` | same |
| `Fwd/Images/fwd.txt` | `ProjectFiles.Fwd.Images.fwd_txt` | same |

A source file using `ProjectFiles.Assets.Images.logo_txt` builds with 0 errors on Windows and fails on Linux:

```
WindowsShape.cs(4,47): error CS0117: 'ProjectFiles' does not contain a definition for 'Assets'
```

Cause: on Linux, MSBuild converts the backslashes in a metadata value only when the first folder of the value exists in the project directory. Otherwise it hands the generator `Assets\Images\logo.txt` unchanged, and the generator does not split it. A link into a folder that does not exist on disk is the usual case for linked files.

The file itself is copied to `Assets/Images/logo.txt` on both systems and the generated path string is correct. Only the shape of the API differs.

Suggested fix: replace `\` with `/` where the metadata is read ([Generator.cs lines 58 and 65](src/ProjectFiles/Generator.cs#L58)) and split on `/` only.

## Generated code that does not compile

In each case the build fails inside `ProjectFiles.g.cs` and no `PROJFILES` diagnostic explains why.

### 8. File extension is not sanitised

Where: [Generator.cs lines 437 to 442](src/ProjectFiles/Generator.cs#L437). `ToFilePropertyName` passes the name through `Identifier.Build` but appends the lowercased extension as it is.

| File | Generated property name |
|---|---|
| `Dockerfile.linux-arm64` | `Dockerfile_linux-arm64` |
| `README.zh-CN` | `README_zh-cn` |
| `backup.txt~` | `backup_txt~` |
| `notes.c++` | `notes_c++` |
| `report.v1 final` | `report_v1 final` |

Each gives syntax errors such as CS1002 and CS1003.

Related, also reproduced: `class.json` generates `@class_json` and an extensionless file `class_json` generates `class_json`. These are the same identifier, the duplicate check compares them as different strings, and the compiler reports CS0102.

Suggested fix: build the identifier from the combined text, `Identifier.Build($"{nameWithoutExtension}_{extensionWithoutDot.ToLowerInvariant()}")`. This produces the same names as today for every currently valid case, and is expected to remove the `@class_json` mismatch as well.

### 9. Identifier collisions are not detected

Where: [Generator.cs lines 184 to 222](src/ProjectFiles/Generator.cs#L184). `FindDuplicatePropertyNames` compares files with files inside one directory. Directory names, inherited members and the enclosing type name are not considered.

| Input | Compiler result |
|---|---|
| `config_json/inner.txt` and `config.json` | CS0102: `ProjectFiles` already contains a definition for `config_json` |
| `my-dir/a.txt` and `my_dir/b.txt` | CS8863 (two `partial class my_dirType()` with parameter lists) and CS0102 |
| `v1.0/a.txt` and `v1_0/b.txt` | CS8863 and CS0102 |
| `Top/my-dir/a.txt` and `Top/my_dir/b.txt` | CS0102 |
| `ProjectFiles/a.txt` (root folder named `ProjectFiles`) | CS0542: member names cannot be the same as their enclosing type |
| `Config/ConfigType` (extensionless file named after the folder's type) | CS0542 |
| `Docs/Path/a.txt`, `Docs/Info/b.txt` | CS0108: `DocsType.Path` and `DocsType.Info` hide `ProjectDirectory.Path` and `ProjectDirectory.Info` |

CS0108 is a warning, so it becomes an error under `TreatWarningsAsErrors`. It was confirmed in a real build for `Top\Info\b.txt`. The other inherited names (`FullPath`, `GetFiles`, `EnumerateFiles` and the rest) follow the same pattern.

Minor, Windows only: `Config/a.json` and `config/b.json`, two spellings of one folder in the project file, produce two separate types `ConfigType` and `configType`.

Suggested fix: keep one name table per scope that holds directory properties, file properties, inherited member names and the enclosing type name, and report clashes with a `PROJFILES004` style diagnostic.

## API

### 10. Nested directory types are not `ProjectDirectory`

Where: [Generator.cs lines 372 to 379](src/ProjectFiles/Generator.cs#L372). Top-level types are emitted as `partial class ConfigType() : ProjectDirectory("Config")` (line 339). Nested types are emitted as `public partial class DevType` with no base class.

Observed with `Config/Dev/a.json`:

```cs
static string top = ProjectFiles.Config;              // compiles
static string nested = ProjectFiles.Config.Dev;       // CS0029: cannot convert DevType to string
static string path = ProjectFiles.Config.Dev.Path;    // CS1061: DevType has no Path
```

A nested directory therefore has no `Path`, no string conversion, no `+` operators and no `EnumerateFiles`. The readme (line 513) describes `ProjectDirectory` as the base class for all generated directory types.

`BuildFileTree` already computes the path of each nested node ([Generator.cs lines 486 to 493](src/ProjectFiles/Generator.cs#L486)); it is used only for naming and ordering.

Suggested fix: emit `public partial class DevType() : ProjectDirectory("Config/Dev")`. Once that is done, the inherited-member clashes from item 9 apply at every level and need handling.

### 11. `ProjectDirectory` converts implicitly to `FileInfo`

Where: [ProjectDirectory.cs lines 17, 18](src/Templates/ProjectDirectory.cs#L17). Read from the code, not exercised.

The operator looks copied from `ProjectFile`. The `Info` property on the same class (line 72) already returns `DirectoryInfo`.

Suggested fix: change the target type to `DirectoryInfo`. That is a breaking change for any caller relying on the current conversion.

### 12. Reserved-name check is broader than needed

Where: [Generator.cs lines 157 to 182](src/ProjectFiles/Generator.cs#L157).

| Input | Diagnostic | Actual clash |
|---|---|---|
| `ProjectFile.json` | PROJFILES001 | none, the property would be `ProjectFile_json` |
| `projectdirectory.txt` | PROJFILES001 | none, the property would be `projectdirectory_txt` |
| `SolutionFile.Backup/a.txt` | PROJFILES002 | none, the class would be `SolutionFile_BackupType`; `Path.GetFileNameWithoutExtension` is applied to a folder name |

The tests `ConflictWithProjectFile`, `ConflictCaseInsensitive` and `NoConflictWhenPropertyNotSet` assert the first two, so this may be intended.

The PROJFILES002 message also prints the file path where the directory is meant (`Directory 'SolutionDirectory/config.json' would generate ...`), and it is reported once per file in the folder.

## Perf

### P1. Editing a copied data file recompiles the project

Where: [ProjectFiles.props lines 26, 41, 80](src/ProjectFiles/buildTransitive/ProjectFiles.props#L26). Each copied file is added to `AdditionalFiles`, and `AdditionalFiles` are inputs of the `CoreCompile` target.

Observed after appending one line to `plain.txt` (a `None` item with `CopyToOutputDirectory="PreserveNewest"`):

| Setup | `CoreCompile` |
|---|---|
| props imported | `Building target "CoreCompile" completely. Input file "plain.txt" is newer than output file "obj\Release\net10.0\App1.pdb".` |
| props not imported (control) | `Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.` |

With ProjectFiles, every edit to a copied settings file or test fixture recompiles the project even though the generator only uses the paths. Embedded resources are compile inputs regardless, so the added cost is for copied files.

Suggested fix: have the target write one manifest file into `obj` (`WriteLinesToFile` with `WriteOnlyWhenDifferent="true"`), listing kind, relative path and resource name per item plus the project and solution paths, and add only that file to `AdditionalFiles`. The generator reads its text. Compilation then reruns only when the set of files changes. The same change removes finding 3, and it hands the compiler and the IDE one additional file instead of one per copied file.

### P2. `EmbeddedResource.ReadAllBytes` buffers twice

Where: [EmbeddedResource.cs lines 43 to 49](src/Templates/EmbeddedResource.cs#L43). Read from the code, not measured.

The stream is copied into a growing `MemoryStream` and then copied again by `ToArray`. A manifest resource stream knows its `Length`, so one array of that size can be allocated and filled directly.

### P3. `ReadAllTextAsync` is synchronous on older targets

Where: [ProjectFile.cs lines 45 to 51](src/Templates/ProjectFile.cs#L45). Read from the code.

Below `netstandard2.1` and `netcoreapp2.0` the method is `Task.FromResult(File.ReadAllText(Path))`: it blocks the caller and ignores the cancellation token. `EmbeddedResource.ReadAllTextAsync` ([EmbeddedResource.cs lines 51 to 60](src/Templates/EmbeddedResource.cs#L51)) awaits without `ConfigureAwait(false)`.

### P4. Culture-sensitive ordering of generated members

Where: [Generator.cs lines 253, 318, 331, 357, 388](src/ProjectFiles/Generator.cs#L253). `OrderBy(_ => _.Path)` uses the default string comparer.

Observed: `my_dir` is emitted before `my-dir` and `config` before `Config`, which is culture order, not ordinal order. The member order of the generated file therefore depends on the culture and globalization mode of the compiler host.

Suggested fix: pass `StringComparer.Ordinal`. It is faster and stable across machines. It reorders members in the existing snapshots.

## Checked and fine

- **Incremental pipeline.** With step tracking on, the source output step was `New` on the first run and `Cached` after an unrelated source edit, after one additional file was replaced (content change), and after the options provider was replaced by one with equal values.
- **Generator speed.** 10,000 copied files: 220 ms on the first run including JIT, 34 to 37 ms on later runs, 1,253,573 characters generated.
- **MSBuild target cost.** In a project with 5,000 plain `None` items and one copied file, `AddProjectFilesToAdditionalFiles` took 29 to 37 ms per build and `AddProjectEmbeddedResourcesToAdditionalFiles` took 0 ms.
- **Files outside the project with no `Link`.** `<None Include="..\Shared\outside.txt" CopyToOutputDirectory="PreserveNewest" />` generates `ProjectFiles.outside_txt` with path `"outside.txt"`, matching where MSBuild copies it. The SDK sets `Link` to the file name for such items. This answers the open question in `todo.md` about sibling-project includes: they appear at the root, named `<name>_<ext>`.
- **Explicit `Link` on Windows.** `Link="Virtual\Folder\linked.txt"` generates `ProjectFiles.Virtual.Folder.linked_txt`.
- **Three files with one property name.** `a.b.txt`, `a_b.txt` and `a-b.txt` give two `PROJFILES004` diagnostics and all three are left out of the output.

## Small things

- Top-level types inside `namespace ProjectFilesGenerator.Types` are emitted with no indentation ([Generator.cs line 276](src/ProjectFiles/Generator.cs#L276) passes an indent of 0).
- `ProjectDirectory` and `SolutionDirectory` paths end with `/` ([Generator.cs line 306](src/ProjectFiles/Generator.cs#L306)) while generated folder paths do not. The readme usage example (line 105) shows the project directory without the trailing slash.
- `ReadResouce` is misspelled ([Generator.cs line 21](src/ProjectFiles/Generator.cs#L21)).
- `parts.Length <= 0` can never be true ([Generator.cs line 164](src/ProjectFiles/Generator.cs#L164)).

## Not verified

- SDK bands 10.0.1xx to 10.0.3xx for finding 7. The mechanism was reproduced with SDK 9.0.318 only.
- The Worker SDK and `Directory.Build.props` cases for finding 4.
- `SolutionPath` containing `#` for finding 3.
- The cost of P2 and P3 was not measured.
- `ProjectFile.Path` values are relative, so `File.Exists` and `FullPath` resolve them against the current directory, not `AppContext.BaseDirectory`. A process started from another working directory would not find the files. This is a design note, not a reproduced failure.

## Environment

- Windows 11, .NET SDK 10.0.401 (Roslyn 5.9.0).
- Linux check: `mcr.microsoft.com/dotnet/sdk:10.0`, SDK 10.0.401, with the project and the `nugets` folder copied in using `docker cp`. The image is still in the local Docker cache (917 MB) and can be removed with `docker rmi mcr.microsoft.com/dotnet/sdk:10.0`.

## Suggested order

1. One-line props changes: findings 1 and 2, the `IfDifferent` part of 5, and the filter in 4.
2. Generator fixes with small blast radius: 8, 6 and 13.
3. Decide the Roslyn floor (7) before the next release.
4. Larger changes: the manifest file (P1 and 3), the name table (9) and nested `ProjectDirectory` types (10). These change snapshots or the public surface.
