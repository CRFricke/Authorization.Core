# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v9.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [Authorization.Core.Tests\Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)
  - [Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj)
  - [Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj)
  - [Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj)
  - [Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)
  - [Authorization.Core.UI\Authorization.Core.UI.csproj](#authorizationcoreuiauthorizationcoreuicsproj)
  - [Authorization.Core\Authorization.Core.csproj](#authorizationcoreauthorizationcorecsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 7 | All require upgrade |
| Total NuGet Packages | 22 | 8 need upgrade |
| Total Code Files | 129 |  |
| Total Code Files with Incidents | 10 |  |
| Total Lines of Code | 13615 |  |
| Total Number of Issues | 33 |  |
| Estimated LOC to modify | 13+ | at least 0.1% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [Authorization.Core.Tests\Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj) | net8.0 | 🟢 Low | 1 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | net8.0 | 🟢 Low | 5 | 9 | 9+ | AspNetCore, Sdk Style = True |
| [Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj) | net8.0 | 🟢 Low | 2 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | net8.0 | 🟢 Low | 1 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj) | net8.0 | 🟢 Low | 2 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Authorization.Core.UI\Authorization.Core.UI.csproj](#authorizationcoreuiauthorizationcoreuicsproj) | net8.0 | 🟢 Low | 1 | 2 | 2+ | ClassLibrary, Sdk Style = True |
| [Authorization.Core\Authorization.Core.csproj](#authorizationcoreauthorizationcorecsproj) | net8.0 | 🟢 Low | 1 | 2 | 2+ | ClassLibrary, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 14 | 63.6% |
| ⚠️ Incompatible | 1 | 4.5% |
| 🔄 Upgrade Recommended | 7 | 31.8% |
| ***Total NuGet Packages*** | ***22*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 13 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 54576 |  |
| ***Total APIs Analyzed*** | ***54589*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| coverlet.collector | 6.0.4 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)<br/>[Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj)<br/>[Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| CRFricke.EF.Core.Utilities | 8.0.1 |  | [Authorization.Core.csproj](#authorizationcoreauthorizationcorecsproj) | ✅Compatible |
| CRFricke.Test.Support | 8.0.1 |  | [Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj) | ✅Compatible |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.14 | 9.0.12 | [Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | [Authorization.Core.csproj](#authorizationcoreauthorizationcorecsproj)<br/>[Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | [Authorization.Core.UI.csproj](#authorizationcoreuiauthorizationcoreuicsproj)<br/>[Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | [Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj)<br/>[Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.14 | 9.0.12 | [Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Tools | 8.0.14 | 9.0.12 | [Authorization.Core.UI.Test.Web.csproj](#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj) | NuGet package upgrade is recommended |
| Microsoft.Extensions.Diagnostics.Testing | 8.10.0 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj) | ✅Compatible |
| Microsoft.Extensions.Identity.Core | 8.0.14 | 9.0.12 | [Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj) | NuGet package upgrade is recommended |
| Microsoft.NET.Test.Sdk | 17.13.0 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)<br/>[Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj)<br/>[Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| Microsoft.Playwright.NUnit | 1.50.0 |  | [Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| Microsoft.Playwright.TestAdapter | 1.50.0 |  | [Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj) | ✅Compatible |
| Microsoft.SourceLink.GitHub | 8.0.0 |  | [Authorization.Core.csproj](#authorizationcoreauthorizationcorecsproj)<br/>[Authorization.Core.UI.csproj](#authorizationcoreuiauthorizationcoreuicsproj) | ✅Compatible |
| MockQueryable.Moq | 7.0.3 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj) | ✅Compatible |
| Moq | 4.20.72 |  | [Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)<br/>[Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj) | ✅Compatible |
| NUnit | 4.3.2 |  | [Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| NUnit.Analyzers | 4.6.0 |  | [Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| NUnit3TestAdapter | 5.0.0 |  | [Authorization.Core.UI.Tests.Playwright.csproj](#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj) | ✅Compatible |
| xunit | 2.9.3 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)<br/>[Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj) | ⚠️NuGet package is deprecated |
| xunit.runner.visualstudio | 3.0.2 |  | [Authorization.Core.Tests.csproj](#authorizationcoretestsauthorizationcoretestscsproj)<br/>[Authorization.Core.UI.Tests.csproj](#authorizationcoreuitestsauthorizationcoreuitestscsproj)<br/>[Authorization.Core.UI.Tests.Integration.csproj](#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| M:System.TimeSpan.FromMinutes(System.Double) | 3 | 23.1% | Source Incompatible |
| P:Microsoft.AspNetCore.Identity.UI.UIFrameworkAttribute.UIFramework | 1 | 7.7% | Source Incompatible |
| T:Microsoft.AspNetCore.Identity.UI.UIFrameworkAttribute | 1 | 7.7% | Source Incompatible |
| T:Microsoft.AspNetCore.Builder.MigrationsEndPointExtensions | 1 | 7.7% | Source Incompatible |
| M:Microsoft.AspNetCore.Builder.MigrationsEndPointExtensions.UseMigrationsEndPoint(Microsoft.AspNetCore.Builder.IApplicationBuilder) | 1 | 7.7% | Source Incompatible |
| T:Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions | 1 | 7.7% | Source Incompatible |
| M:Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions.AddDefaultIdentity''1(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Microsoft.AspNetCore.Identity.IdentityOptions}) | 1 | 7.7% | Source Incompatible |
| T:Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions | 1 | 7.7% | Source Incompatible |
| M:Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions.AddEntityFrameworkStores''1(Microsoft.AspNetCore.Identity.IdentityBuilder) | 1 | 7.7% | Source Incompatible |
| T:Microsoft.Extensions.DependencyInjection.DatabaseDeveloperPageExceptionFilterServiceExtensions | 1 | 7.7% | Source Incompatible |
| M:Microsoft.Extensions.DependencyInjection.DatabaseDeveloperPageExceptionFilterServiceExtensions.AddDatabaseDeveloperPageExceptionFilter(Microsoft.Extensions.DependencyInjection.IServiceCollection) | 1 | 7.7% | Source Incompatible |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Authorization.Core.csproj</b><br/><small>net8.0</small>"]
    P2["<b>📦&nbsp;Authorization.Core.Tests.csproj</b><br/><small>net8.0</small>"]
    P3["<b>📦&nbsp;Authorization.Core.UI.csproj</b><br/><small>net8.0</small>"]
    P4["<b>📦&nbsp;Authorization.Core.UI.Test.Web.csproj</b><br/><small>net8.0</small>"]
    P5["<b>📦&nbsp;Authorization.Core.UI.Tests.csproj</b><br/><small>net8.0</small>"]
    P6["<b>📦&nbsp;Authorization.Core.UI.Tests.Integration.csproj</b><br/><small>net8.0</small>"]
    P7["<b>📦&nbsp;Authorization.Core.UI.Tests.Playwright.csproj</b><br/><small>net8.0</small>"]
    P2 --> P1
    P3 --> P1
    P4 --> P3
    P5 --> P3
    P6 --> P4
    P7 --> P4
    click P1 "#authorizationcoreauthorizationcorecsproj"
    click P2 "#authorizationcoretestsauthorizationcoretestscsproj"
    click P3 "#authorizationcoreuiauthorizationcoreuicsproj"
    click P4 "#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"
    click P5 "#authorizationcoreuitestsauthorizationcoreuitestscsproj"
    click P6 "#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj"
    click P7 "#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj"

```

## Project Details

<a id="authorizationcoretestsauthorizationcoretestscsproj"></a>
### Authorization.Core.Tests\Authorization.Core.Tests.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 7
- **Number of Files with Incidents**: 1
- **Lines of Code**: 1105
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Authorization.Core.Tests.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.Tests.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoretestsauthorizationcoretestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;Authorization.Core.csproj</b><br/><small>net8.0</small>"]
        click P1 "#authorizationcoreauthorizationcorecsproj"
    end
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 2701 |  |
| ***Total APIs Analyzed*** | ***2701*** |  |

<a id="authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"></a>
### Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 69
- **Number of Files with Incidents**: 2
- **Lines of Code**: 1993
- **Estimated LOC to modify**: 9+ (at least 0.5% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P6["<b>📦&nbsp;Authorization.Core.UI.Tests.Integration.csproj</b><br/><small>net8.0</small>"]
        P7["<b>📦&nbsp;Authorization.Core.UI.Tests.Playwright.csproj</b><br/><small>net8.0</small>"]
        click P6 "#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj"
        click P7 "#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj"
    end
    subgraph current["Authorization.Core.UI.Test.Web.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.UI.Test.Web.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P3["<b>📦&nbsp;Authorization.Core.UI.csproj</b><br/><small>net8.0</small>"]
        click P3 "#authorizationcoreuiauthorizationcoreuicsproj"
    end
    P6 --> MAIN
    P7 --> MAIN
    MAIN --> P3

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 9 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 6015 |  |
| ***Total APIs Analyzed*** | ***6024*** |  |

<a id="authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj"></a>
### Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 325
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Authorization.Core.UI.Tests.Integration.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.UI.Tests.Integration.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreuitestsintegrationauthorizationcoreuitestsintegrationcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P4["<b>📦&nbsp;Authorization.Core.UI.Test.Web.csproj</b><br/><small>net8.0</small>"]
        click P4 "#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"
    end
    MAIN --> P4

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 474 |  |
| ***Total APIs Analyzed*** | ***474*** |  |

<a id="authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj"></a>
### Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 7
- **Number of Files with Incidents**: 1
- **Lines of Code**: 1374
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Authorization.Core.UI.Tests.Playwright.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.UI.Tests.Playwright.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreuitestsplaywrightauthorizationcoreuitestsplaywrightcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P4["<b>📦&nbsp;Authorization.Core.UI.Test.Web.csproj</b><br/><small>net8.0</small>"]
        click P4 "#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"
    end
    MAIN --> P4

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 3491 |  |
| ***Total APIs Analyzed*** | ***3491*** |  |

<a id="authorizationcoreuitestsauthorizationcoreuitestscsproj"></a>
### Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 8
- **Number of Files with Incidents**: 1
- **Lines of Code**: 3007
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Authorization.Core.UI.Tests.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.UI.Tests.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreuitestsauthorizationcoreuitestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P3["<b>📦&nbsp;Authorization.Core.UI.csproj</b><br/><small>net8.0</small>"]
        click P3 "#authorizationcoreuiauthorizationcoreuicsproj"
    end
    MAIN --> P3

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 8868 |  |
| ***Total APIs Analyzed*** | ***8868*** |  |

<a id="authorizationcoreuiauthorizationcoreuicsproj"></a>
### Authorization.Core.UI\Authorization.Core.UI.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 45
- **Number of Files with Incidents**: 2
- **Lines of Code**: 3716
- **Estimated LOC to modify**: 2+ (at least 0.1% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P4["<b>📦&nbsp;Authorization.Core.UI.Test.Web.csproj</b><br/><small>net8.0</small>"]
        P5["<b>📦&nbsp;Authorization.Core.UI.Tests.csproj</b><br/><small>net8.0</small>"]
        click P4 "#authorizationcoreuitestwebauthorizationcoreuitestwebcsproj"
        click P5 "#authorizationcoreuitestsauthorizationcoreuitestscsproj"
    end
    subgraph current["Authorization.Core.UI.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.UI.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreuiauthorizationcoreuicsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;Authorization.Core.csproj</b><br/><small>net8.0</small>"]
        click P1 "#authorizationcoreauthorizationcorecsproj"
    end
    P4 --> MAIN
    P5 --> MAIN
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 30986 |  |
| ***Total APIs Analyzed*** | ***30988*** |  |

<a id="authorizationcoreauthorizationcorecsproj"></a>
### Authorization.Core\Authorization.Core.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net9.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 2
- **Number of Files**: 30
- **Number of Files with Incidents**: 2
- **Lines of Code**: 2095
- **Estimated LOC to modify**: 2+ (at least 0.1% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P2["<b>📦&nbsp;Authorization.Core.Tests.csproj</b><br/><small>net8.0</small>"]
        P3["<b>📦&nbsp;Authorization.Core.UI.csproj</b><br/><small>net8.0</small>"]
        click P2 "#authorizationcoretestsauthorizationcoretestscsproj"
        click P3 "#authorizationcoreuiauthorizationcoreuicsproj"
    end
    subgraph current["Authorization.Core.csproj"]
        MAIN["<b>📦&nbsp;Authorization.Core.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#authorizationcoreauthorizationcorecsproj"
    end
    P2 --> MAIN
    P3 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 2 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 2041 |  |
| ***Total APIs Analyzed*** | ***2043*** |  |

