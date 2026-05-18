# Baseline Audit — `maintenance/baseline-audit`

Date: 2026-05-18  
Scope: inventory-only baseline plus CI result retention. No production/UI/HTTP/download behavior was changed.

## Environment

- Repository branch prepared for this work: `maintenance/baseline-audit`.
- SDK requested by `global.json`: `10.0.100` with `rollForward: latestFeature`.
- SDK used locally: .NET SDK `10.0.107`; host/runtime `10.0.7`; OS `ubuntu 24.04` x64.

## Commands executed

| Command | Result | Notes |
|---|---:|---|
| `dotnet --info` | Pass | Confirmed .NET SDK `10.0.107` and `global.json` roll-forward. |
| `dotnet restore DownKyi.sln` | Pass | Restored all 3 projects. |
| `dotnet build DownKyi.sln --configuration Release -p:ContinuousIntegrationBuild=true` | Pass with warnings | Build succeeded with 1,160 compiler/analyzer warnings and 0 errors. |
| `dotnet test DownKyi.Core.Tests/DownKyi.Core.Tests.csproj --configuration Release --collect:"XPlat Code Coverage"` | Pass | 34 passed, 0 failed, 0 skipped; Cobertura coverage file emitted under `DownKyi.Core.Tests/TestResults/`. |
| `dotnet list DownKyi.sln package --outdated --include-transitive` | Pass | Outdated package inventory captured below; no package versions changed in this PR. |
| `dotnet list DownKyi.sln package --vulnerable --include-transitive` | Pass | No vulnerable packages reported from `https://api.nuget.org/v3/index.json`. |

## Build warning baseline

The Release CI build currently succeeds but emits a large nullable-warning baseline. The dominant warning categories are:

| Warning | Count in build log | Primary risk theme |
|---|---:|---|
| `CS8618` | 997 | Non-nullable fields/properties not initialized at construction. |
| `CS8602` | 42 | Possible null dereference. |
| `CS8601` | 37 | Possible null assignment. |
| `CS8603` | 25 | Possible null return. |
| `CS8625` | 19 | Null literal assigned to non-nullable reference. |
| `CS8604` | 12 | Possible null argument. |
| Other nullable/compiler/analyzer warnings | 28 | Includes `CS0168`, `CS0169`, `CS0472`, `CS0649`, `CS8600`, `CS8605`, `CS8619`, `CS8620`, `CS8622`, and `CA2022`. |

Recommendation: keep this PR limited to recording the baseline. Address warnings in small follow-up PRs by area, starting with Core models/API DTO nullability and the `CA2022` exact-read warnings in `DownKyi.Core/Utils/Encryptor/Encryptor.File.cs`.

## Dependency inventory

### Direct packages with newer versions available

No package versions were changed. This section records candidates for future low-risk update PRs.

| Project | Package | Current | Latest reported | Notes |
|---|---|---:|---:|---|
| `DownKyi` | `System.Formats.Nrbf` | 10.0.7 | 10.0.8 | Patch-level .NET package candidate. |
| `DownKyi` | `Avalonia` | 11.3.15 | 12.0.3 | Major UI framework upgrade; should be isolated. |
| `DownKyi` | `Avalonia.Controls.DataGrid` | 11.3.13 | 12.0.0 | Major UI framework upgrade; should be isolated. |
| `DownKyi` | `Avalonia.Desktop` | 11.3.15 | 12.0.3 | Major UI framework upgrade; should be isolated. |
| `DownKyi` | `Avalonia.Themes.Simple` | 11.3.15 | 12.0.3 | Major UI framework upgrade; should be isolated. |
| `DownKyi` | `Xaml.Behaviors` | 11.3.9.5 | 12.0.0 | Major UI behavior package upgrade; should be isolated. |
| `DownKyi` | `Prism.Avalonia` | 8.1.97.11073 | 9.0.537.11130 | Major Prism upgrade; should be isolated. |
| `DownKyi` | `Prism.DryIoc.Avalonia` | 8.1.97.11073 | 9.0.537.11130 | Major Prism/DI upgrade; should be isolated. |
| `DownKyi.Core` | `Avalonia` | 11.3.15 | 12.0.3 | Core currently references Avalonia; validate why before changing. |
| `DownKyi.Core` | `Microsoft.Data.Sqlite.Core` | 10.0.7 | 10.0.8 | Patch-level database package candidate. |

### Notable transitive packages with newer versions

- `Microsoft.Data.Sqlite.Core`, `Microsoft.Extensions.*`, and other .NET packages report patch updates from `10.0.7` to `10.0.8`.
- `SQLitePCLRaw.core` reports `2.1.11` to `3.0.3`; this is a major native SQLite stack update and should be tested carefully.
- Avalonia/Skia/HarfBuzz/MicroCom transitive packages report larger UI/native stack updates tied to the Avalonia major upgrade.
- `xunit.analyzers` is transitive in the test project and reports `1.18.0` to `1.27.0`.

## Vulnerability inventory

`dotnet list DownKyi.sln package --vulnerable --include-transitive` reported no vulnerable packages for `DownKyi`, `DownKyi.Core`, or `DownKyi.Core.Tests` against NuGet.org on 2026-05-18.

## CI hardening included

The PR build workflow now uploads `DownKyi.Core.Tests/TestResults/` as a `test-results` artifact on every run, even if tests fail. This preserves coverage and TRX/Cobertura outputs for PR validation without changing build, test, package, application, HTTP, download, storage, or UI behavior.

## Follow-up PR recommendations

1. Patch-level package update PR for .NET packages only (`System.Formats.Nrbf`, `Microsoft.Data.Sqlite.Core`, and related transitive updates where safe), with restore/build/test evidence.
2. Nullable-warning stabilization PRs grouped by bounded folder/module.
3. HTTP/WebClient testability PR that introduces seam(s) without changing request semantics.
4. Stream/download memory-safety PR that adds regression tests before behavior changes.
