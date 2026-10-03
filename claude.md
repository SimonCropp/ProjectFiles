# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A Roslyn C# incremental source generator (`ProjectFiles`) that emits a strongly-typed static API for every file marked with `CopyToOutputDirectory` (`PreserveNewest`/`Always`). Packaged as a NuGet analyzer.

## Build and test

Requires .NET SDK 10.0.401+ (pinned via `global.json`). The analyzer is compiled against the Roslyn that SDK ships (5.9), so `Microsoft.CodeAnalysis.CSharp` in `src/Directory.Packages.props`, `MinimumSdkVersion` in `ProjectFiles.props` and the `roslyn5.9` package folder in `ProjectFiles.csproj` must move together.

```bash
dotnet build src --configuration Release
dotnet build IntegrationTests --configuration Release
# Project-level build without $(SolutionDir) — must not break:
dotnet build src/ConsumingTests/ConsumingTests.csproj --configuration Release --force

dotnet test src --configuration Release --no-build --no-restore
dotnet test IntegrationTests --configuration Release --no-build --no-restore
dotnet test src/ConsumingTests/ConsumingTests.csproj --configuration Release --no-build --no-restore
```

Run a single test: `dotnet test src/Tests/Tests.csproj --filter "FullyQualifiedName~GeneratorTest.SingleFileAtRoot"`

## Architecture

### Two-sided design: MSBuild props + generator

1. `src/ProjectFiles/buildTransitive/ProjectFiles.props` — ships in the NuGet package. Before compilation it writes `obj/.../ProjectFiles.manifest.txt` and adds that single file to `AdditionalFiles`. The manifest has one line per entry: `Project|path`, `Solution|path`, `File|path` for `None`/`Content` items with `CopyToOutputDirectory` (using `%(Link)` if present), and `Resource|path|name` for embedded resources. A manifest is used (rather than one `AdditionalFiles` item per file) so editing a copied file does not recompile the project, and because values passed through the generated `.editorconfig` are cut at `#` or `;`. Only `ImplicitUsings` is exposed via `CompilerVisibleProperty`.
2. `src/ProjectFiles/Generator.cs` (`IIncrementalGenerator`) — parses the manifest (`Manifest.cs`) and emits three (optionally four) files: `ProjectFiles.g.cs`, `ProjectFiles.ProjectDirectory.g.cs`, `ProjectFiles.ProjectFile.g.cs`, plus `ProjectFiles.GlobalUsings.g.cs` when `ImplicitUsings` is on.

The two base-class files (`ProjectDirectory.cs`, `ProjectFile.cs`) live in `src/Templates/` and are pulled into `ProjectFiles.csproj` as `EmbeddedResource`s — the `Templates` project itself is a multi-target compile check (every TFM from `net461` to `net10.0`) to ensure the templates stay portable.

### Conflict detection (before codegen)

`Generator.cs` runs two passes and reports diagnostics `PROJFILES001`–`PROJFILES004`:
- Reserved-name conflicts (`ProjectDirectory`, `ProjectFile`, `SolutionDirectory`, `SolutionFile`, `GitRepoDirectory`) for root files/directories.
- Duplicate property names within the same directory (e.g. `config.json` vs `config_json` → both become `config_json`).

Conflicting files are stripped from the tree; the rest still generate.

Identifier generation lives in `Identifier.cs` + `KeywordDetect.cs`. `ToFilePropertyName` produces `<name>_<ext-lowercased>`; dot-files (`.env`) are handled specially. File paths in emitted code are always forward-slashed.

Language version is enforced: the generator emits `PROJFILES003` and bails if the consuming compilation is pre-C# 14.

When no `SolutionPath` is provided by MSBuild, `SolutionDirectoryFinder.cs` walks up (bounded, stops at a `.git` dir) looking for `.sln`/`.slnx`.

`GitRepoDirectoryFinder.cs` walks up from the project file to the nearest directory containing `.git` (directory or file) and the generator emits it as `ProjectFiles.GitRepoDirectory`; omitted when the project is not in a git repo.

### Test strategy

`src/Tests/` is the unit-test project — drives the generator in-memory with `CSharpGeneratorDriver` and snapshot-verifies every output with **Verify** (`Verify.SourceGenerators`, `Verify.NUnit`). Each test produces three `.verified.*` files (one per emitted source) plus a `.verified.txt` for diagnostics.

When changing the generator, expect many `*.received.*` files — review and promote them via the Verify diff tool.

The four consumption surfaces each exercise a different integration path and should all keep building:
- `src/ConsumingTests/` — uses the generator via `ProjectReference` with `OutputItemType="Analyzer"` (imports `buildTransitive/ProjectFiles.props` manually).
- `src/NugetTests/` — consumes the NuGet package built into `nugets/`.
- `IntegrationTests/IntegrationTests/` — multi-TFM (`net471;net48;net8.0;net9.0;net10.0`) NuGet consumer, also asserts `ImplicitUsings=true` flow.
- `src/Tests/` — pure generator unit tests, no consumption.

`IntegrationTests/nuget.config` and `src/NugetTests/nuget.config` add `../nugets` as a local feed so `PackageReference Include="ProjectFiles"` picks up a locally-packed build (the latter also overrides the `signatureValidationMode=require` from `src/nuget.config`, since the local package is unsigned). The `ProjectFiles` package version is pinned to `$(Version)` in `src/Directory.Packages.props` / `IntegrationTests/Directory.Packages.props`.

`NugetTests` consumes the locally-packed nuget, so it is **not** in `src/ProjectFiles.slnx` (the package won't exist at restore time during a clean `dotnet build src`). It is built/tested as a separate step in `.github/workflows/build.yml` after `src` has been packed.

## Conventions

- Central package versions: `src/Directory.Packages.props`, `IntegrationTests/Directory.Packages.props`.
- `TreatWarningsAsErrors=true` and `EnforceCodeStyleInBuild=true` project-wide (see `src/Directory.Build.props`).
- Readme code snippets are synced by `MarkdownSnippets.MsBuild` — `mdsnippets.json` with `InPlaceOverwrite`. Don't hand-edit the `<!-- snippet: -->` blocks in `readme.md`; edit the source file and rebuild.
- `src/Directory.Build.props` carries the package `<Version>` — bump it there when shipping.
