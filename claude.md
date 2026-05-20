# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A Roslyn C# incremental source generator (`ProjectFiles`) that emits a strongly-typed static API for every file marked with `CopyToOutputDirectory` (`PreserveNewest`/`Always`). Packaged as a NuGet analyzer.

## Build and test

Requires .NET SDK 10.0.100+ (pinned to 10.0.202 via `src/global.json`).

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

1. `src/ProjectFiles/buildTransitive/ProjectFiles.props` — ships in the NuGet package. Before compilation it promotes `None`/`Content` items with `CopyToOutputDirectory` into `AdditionalFiles`, tagging each with a `ProjectFilesGenerator` metadata value (using `%(Link)` if present). Exposes `MSBuildProjectFullPath`, `SolutionPath`, `ImplicitUsings` to the generator via `CompilerVisibleProperty`.
2. `src/ProjectFiles/Generator.cs` (`IIncrementalGenerator`) — reads those `AdditionalFiles` + MSBuild props and emits three (optionally four) files: `ProjectFiles.g.cs`, `ProjectFiles.ProjectDirectory.g.cs`, `ProjectFiles.ProjectFile.g.cs`, plus `ProjectFiles.GlobalUsings.g.cs` when `ImplicitUsings` is on.

The two base-class files (`ProjectDirectory.cs`, `ProjectFile.cs`) live in `src/Templates/` and are pulled into `ProjectFiles.csproj` as `EmbeddedResource`s — the `Templates` project itself is a multi-target compile check (every TFM from `net461` to `net10.0`) to ensure the templates stay portable.

### Conflict detection (before codegen)

`Generator.cs` runs two passes and reports diagnostics `PROJFILES001`–`PROJFILES004`:
- Reserved-name conflicts (`ProjectDirectory`, `ProjectFile`, `SolutionDirectory`, `SolutionFile`) for root files/directories.
- Duplicate property names within the same directory (e.g. `config.json` vs `config_json` → both become `config_json`).

Conflicting files are stripped from the tree; the rest still generate.

Identifier generation lives in `Identifier.cs` + `KeywordDetect.cs`. `ToFilePropertyName` produces `<name>_<ext-lowercased>`; dot-files (`.env`) are handled specially. File paths in emitted code are always forward-slashed.

Language version is enforced: the generator emits `PROJFILES003` and bails if the consuming compilation is pre-C# 14.

When no `SolutionPath` is provided by MSBuild, `SolutionDirectoryFinder.cs` walks up (bounded, stops at a `.git` dir) looking for `.sln`/`.slnx`.

### Test strategy

`src/Tests/` is the unit-test project — drives the generator in-memory with `CSharpGeneratorDriver` and snapshot-verifies every output with **Verify** (`Verify.SourceGenerators`, `Verify.NUnit`, `Verify.DiffPlex`). Each test produces three `.verified.*` files (one per emitted source) plus a `.verified.txt` for diagnostics.

When changing the generator, expect many `*.received.*` files — review and promote them via the Verify diff tool.

The four consumption surfaces each exercise a different integration path and should all keep building:
- `src/ConsumingTests/` — uses the generator via `ProjectReference` with `OutputItemType="Analyzer"` (imports `buildTransitive/ProjectFiles.props` manually).
- `src/NugetTests/` — consumes the NuGet package built into `nugets/`.
- `IntegrationTests/IntegrationTests/` — multi-TFM (`net471;net48;net8.0;net9.0;net10.0`) NuGet consumer, also asserts `ImplicitUsings=true` flow.
- `src/Tests/` — pure generator unit tests, no consumption.

`IntegrationTests/nuget.config` and `src/NugetTests/nuget.config` add `../nugets` as a local feed so `PackageReference Include="ProjectFiles"` picks up a locally-packed build (the latter also overrides the `signatureValidationMode=require` from `src/nuget.config`, since the local package is unsigned). The `ProjectFiles` package version is pinned to `$(Version)` in `src/Directory.Packages.props` / `IntegrationTests/Directory.Packages.props`.

`NugetTests` consumes the locally-packed nuget, so it is **not** in `src/ProjectFiles.slnx` (the package won't exist at restore time during a clean `dotnet build src`). It is built/tested as a separate step in `src/appveyor.yml` after `src` has been packed.

## Conventions

- Central package versions: `src/Directory.Packages.props`, `IntegrationTests/Directory.Packages.props`.
- `TreatWarningsAsErrors=true` and `EnforceCodeStyleInBuild=true` project-wide (see `src/Directory.Build.props`).
- Readme code snippets are synced by `MarkdownSnippets.MsBuild` — `mdsnippets.json` with `InPlaceOverwrite`. Don't hand-edit the `<!-- snippet: -->` blocks in `readme.md`; edit the source file and rebuild.
- `src/Directory.Build.props` carries the package `<Version>` — bump it there when shipping.
