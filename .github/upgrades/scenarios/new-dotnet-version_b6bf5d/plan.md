# .NET 9 Upgrade Plan - Authorization.Core Solution

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Implementation Timeline](#implementation-timeline)
- [Detailed Execution Steps](#detailed-execution-steps)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing-validation-strategy)
- [Complexity & Effort Assessment](#complexity-effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Overview
This plan details the upgrade of the **Authorization.Core** solution from **.NET 8.0** to **.NET 9.0**. The solution consists of 7 projects including core class libraries, an ASP.NET Core test web application, and multiple test projects using xUnit, NUnit, and Playwright.

### Scope
**Projects Affected:** All 7 projects in the solution
- 2 class libraries (Authorization.Core, Authorization.Core.UI)
- 1 ASP.NET Core web application (Authorization.Core.UI.Test.Web)
- 4 test projects (Authorization.Core.Tests, Authorization.Core.UI.Tests, Authorization.Core.UI.Tests.Integration, Authorization.Core.UI.Tests.Playwright)

**Current State:** All projects targeting net8.0  
**Target State:** All projects targeting net9.0

### Selected Strategy
**All-At-Once Strategy** - All projects upgraded simultaneously in a single atomic operation.

**Rationale:**
- Small solution (7 projects)
- Simple dependency structure (3 levels deep, no circular dependencies)
- All projects currently on .NET 8.0 (homogeneous codebase)
- Low complexity - all projects rated as low difficulty
- All required packages have .NET 9 compatible versions available
- Clear dependency resolution path
- No security vulnerabilities requiring immediate isolation

### Key Metrics
| Metric | Value |
|--------|-------|
| Total Projects | 7 |
| Total Packages to Update | 10 (8 from analysis + 2 user-specified) |
| Total Code Files | 129 |
| Files with API Issues | 10 |
| Total LOC | 13,615 |
| Estimated LOC Impact | 13+ (0.1% of codebase) |
| Source-Incompatible APIs | 13 |
| Security Vulnerabilities | 0 |

### Critical Issues
- **Deprecated Package:** xunit package (version 2.9.3) is deprecated and used in 3 test projects
- **API Compatibility:** 13 source-incompatible APIs requiring recompilation, primarily in ASP.NET Core Identity and Entity Framework areas
- **Custom Package Versions:** User requires CRFricke.EF.Core.Utilities and CRFricke.Test.Support upgraded to 9.0.0

### Recommended Approach
Execute as a single coordinated operation with all projects and packages updated simultaneously, followed by comprehensive build verification and testing. The homogeneous nature of the solution and clear upgrade path make this the most efficient approach.

## Migration Strategy

### Strategy Selection: All-At-Once

This upgrade will use the **All-At-Once Strategy**, where all 7 projects are upgraded to .NET 9.0 simultaneously in a single coordinated operation.

#### Why All-At-Once?

**Ideal Conditions Met:**
- ✅ Small solution (7 projects, well under the 30-project threshold)
- ✅ All projects currently on .NET 8.0 (modern .NET, no Framework to Core migration needed)
- ✅ Homogeneous codebase with consistent patterns
- ✅ Low external dependency complexity (22 total packages, 10 requiring updates)
- ✅ All required NuGet packages have .NET 9 compatible versions available
- ✅ Simple dependency structure (4 levels, no circular dependencies)
- ✅ All projects rated as low difficulty for upgrade
- ✅ Good test coverage (4 test projects with unit, integration, and E2E tests)

**Benefits for This Solution:**
- **Fastest Completion:** Single upgrade operation rather than phased approach
- **No Multi-Targeting Complexity:** Avoid intermediate states with mixed framework versions
- **Simplified Testing:** Single comprehensive test pass rather than testing at each phase
- **Clean Dependency Resolution:** All projects move together, avoiding version conflicts
- **Immediate Benefits:** All projects gain .NET 9 improvements simultaneously

#### Execution Approach

The All-At-Once strategy for this solution means:

1. **Single Atomic Operation:** All project files updated to net9.0 in one pass
2. **Simultaneous Package Updates:** All 10 package updates applied across all affected projects
3. **Unified Build Verification:** Single solution build to identify all compilation errors
4. **Consolidated Error Fixing:** Address all framework/package-related breaking changes together
5. **Comprehensive Testing:** Execute all test projects after successful build
6. **Single Commit:** All changes committed together (recommended)

#### Risk Mitigation

While All-At-Once is faster, it concentrates risk into a single operation. Mitigations:

- **Comprehensive Assessment:** Detailed analysis completed, no surprises expected
- **No Security Vulnerabilities:** No urgent security fixes complicating the upgrade
- **Low API Impact:** Only 13 source-incompatible APIs (0.02% of total APIs)
- **Clear Breaking Changes:** Well-documented .NET 8 → 9 migration path
- **Strong Test Coverage:** 4 test projects covering unit, integration, and E2E scenarios
- **Git Branch Isolation:** Work performed on dedicated upgrade-to-NET9 branch
- **Rollback Ready:** Can revert entire upgrade if blocking issues discovered

#### Dependency-Based Ordering (Context Only)

While all projects upgrade simultaneously, understanding the dependency order helps with troubleshooting:

1. **Level 0:** Authorization.Core (foundation, no dependencies)
2. **Level 1:** Authorization.Core.UI, Authorization.Core.Tests (depend on Level 0)
3. **Level 2:** Authorization.Core.UI.Test.Web, Authorization.Core.UI.Tests (depend on Levels 0-1)
4. **Level 3:** Authorization.Core.UI.Tests.Integration, Authorization.Core.UI.Tests.Playwright (depend on Levels 0-2)

If build errors occur, reviewing them in this dependency order (bottom-up) helps identify root causes.

#### Parallel vs Sequential Execution

**File Updates:** All project files (.csproj) updated in parallel (batch operation)  
**Package Updates:** All package references updated in parallel (batch operation)  
**Build:** Single solution-level build  
**Error Fixing:** Address errors in dependency order if needed (Level 0 → Level 3)  
**Testing:** Test projects can be executed in parallel or sequentially

## Detailed Dependency Analysis

### Dependency Graph Summary
The solution has a clean, hierarchical dependency structure with 4 distinct levels (0-3), with no circular dependencies. This structure is ideal for the All-At-Once strategy as all projects can be upgraded simultaneously without dependency conflicts.

```
Level 0 (Foundation):
  └─ Authorization.Core.csproj (ClassLibrary)

Level 1 (Core Dependencies):
  ├─ Authorization.Core.Tests.csproj (Test) → Authorization.Core
  └─ Authorization.Core.UI.csproj (ClassLibrary) → Authorization.Core

Level 2 (UI and Tests):
  ├─ Authorization.Core.UI.Test.Web.csproj (AspNetCore) → Authorization.Core.UI
  └─ Authorization.Core.UI.Tests.csproj (Test) → Authorization.Core.UI

Level 3 (Integration Tests):
  ├─ Authorization.Core.UI.Tests.Integration.csproj (Test) → Authorization.Core.UI.Test.Web
  └─ Authorization.Core.UI.Tests.Playwright.csproj (Test) → Authorization.Core.UI.Test.Web
```

### Project Groupings for All-At-Once Migration

Since this is an All-At-Once upgrade, all projects are updated in a single atomic operation. However, understanding the dependency structure is important for troubleshooting if issues arise:

**Group 1: Foundation Libraries** (Level 0)
- Authorization.Core.csproj
  - **No project dependencies**
  - Used by: 2 projects (Authorization.Core.Tests, Authorization.Core.UI)
  - Package updates: Microsoft.AspNetCore.Identity.EntityFrameworkCore, CRFricke.EF.Core.Utilities, Microsoft.SourceLink.GitHub

**Group 2: UI Libraries & Core Tests** (Level 1)
- Authorization.Core.UI.csproj
  - Depends on: Authorization.Core
  - Used by: 2 projects (Authorization.Core.UI.Test.Web, Authorization.Core.UI.Tests)
  - Package updates: Microsoft.AspNetCore.Identity.UI, Microsoft.SourceLink.GitHub

- Authorization.Core.Tests.csproj
  - Depends on: Authorization.Core
  - Package updates: Test frameworks (coverlet.collector, xunit, etc.)

**Group 3: ASP.NET Core Application & UI Tests** (Level 2)
- Authorization.Core.UI.Test.Web.csproj
  - Depends on: Authorization.Core.UI
  - Used by: 2 integration test projects
  - Package updates: Multiple ASP.NET Core and EF Core packages (most updates in solution)

- Authorization.Core.UI.Tests.csproj
  - Depends on: Authorization.Core.UI
  - Package updates: Microsoft.Extensions.Identity.Core, CRFricke.Test.Support, xunit, test frameworks

**Group 4: Integration & E2E Tests** (Level 3)
- Authorization.Core.UI.Tests.Integration.csproj
  - Depends on: Authorization.Core.UI.Test.Web
  - Package updates: Microsoft.AspNetCore.Mvc.Testing, xunit, test frameworks

- Authorization.Core.UI.Tests.Playwright.csproj
  - Depends on: Authorization.Core.UI.Test.Web
  - Package updates: Microsoft.AspNetCore.Mvc.Testing, NUnit frameworks

### Critical Path
The dependency chain flows as:
```
Authorization.Core 
  → Authorization.Core.UI 
    → Authorization.Core.UI.Test.Web 
      → Integration/Playwright Tests
```

However, for All-At-Once strategy, all projects are updated simultaneously, so there is no sequential critical path during upgrade execution.

### Dependency Considerations
- **No Circular Dependencies:** Clean hierarchy allows straightforward upgrade
- **Clear Separation:** Core logic, UI components, test web app, and tests are well-separated
- **Shared Test Infrastructure:** Test projects share common packages (xunit, coverlet.collector, Microsoft.NET.Test.Sdk)
- **ASP.NET Core Concentration:** Most API-incompatible issues concentrated in Authorization.Core.UI.Test.Web project

## Implementation Timeline

### Phase 1: Atomic Upgrade

**Operations** (performed as single coordinated batch):
1. Update all 7 project files from net8.0 to net9.0
2. Update all 10 package references across affected projects
3. Restore dependencies (dotnet restore)
4. Build solution to identify compilation errors
5. Fix all compilation errors from framework and package upgrades
6. Rebuild solution to verify fixes

**Deliverables:**
- All projects targeting net9.0
- All packages updated to .NET 9 compatible versions
- Solution builds with 0 errors
- Solution builds with 0 warnings (target)

### Phase 2: Test Validation

**Operations:**
1. Execute all test projects
2. Address any test failures related to framework/package changes
3. Verify test coverage maintained

**Test Projects to Execute:**
- Authorization.Core.Tests.csproj (xUnit tests)
- Authorization.Core.UI.Tests.csproj (xUnit tests)
- Authorization.Core.UI.Tests.Integration.csproj (xUnit integration tests)
- Authorization.Core.UI.Tests.Playwright.csproj (NUnit E2E tests)

**Deliverables:**
- All tests pass
- No test coverage regression
- No new test failures introduced by upgrade

### Phase 3: Finalization

**Operations:**
1. Review and commit all changes
2. Document any migration notes for team
3. Update CI/CD pipelines if needed (verify .NET 9 SDK availability)

**Deliverables:**
- Changes committed to upgrade-to-NET9 branch
- Migration documentation complete
- Ready for PR and merge to main branch

## Detailed Execution Steps

### Step 1: Update Project Target Frameworks

Update the `TargetFramework` property in all project files from `net8.0` to `net9.0`:

**All Projects:**
- Authorization.Core\Authorization.Core.csproj
- Authorization.Core.UI\Authorization.Core.UI.csproj
- Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj
- Authorization.Core.Tests\Authorization.Core.Tests.csproj
- Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj
- Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj
- Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj

**Change:**
```xml
<TargetFramework>net8.0</TargetFramework>
```
to
```xml
<TargetFramework>net9.0</TargetFramework>
```

### Step 2: Update Package References

See [Package Update Reference](#package-update-reference) for complete matrix.

**Key Updates:**
- **ASP.NET Core packages:** 8.0.14 → 9.0.12 (6 package updates, affecting 5 projects)
- **Entity Framework packages:** 8.0.14 → 9.0.12 (2 package updates, affecting 1 project)
- **CRFricke packages:** 8.0.1 → 9.0.0 (2 package updates, affecting 2 projects - user requirement)

**Affected Projects:**
- Authorization.Core.csproj: 2 packages
- Authorization.Core.UI.csproj: 1 package
- Authorization.Core.UI.Test.Web.csproj: 5 packages
- Authorization.Core.UI.Tests.csproj: 2 packages
- Authorization.Core.UI.Tests.Integration.csproj: 1 package
- Authorization.Core.UI.Tests.Playwright.csproj: 1 package

### Step 3: Restore Dependencies

Execute dependency restoration:
```bash
dotnet restore Authorization.Core.sln
```

### Step 4: Build and Address Breaking Changes

Build the solution to identify compilation errors:
```bash
dotnet build Authorization.Core.sln
```

See [Breaking Changes Catalog](#breaking-changes-catalog) for comprehensive list of expected issues.

**Focus Areas:**
- ASP.NET Core Identity API changes (Authorization.Core.UI.Test.Web)
- Entity Framework migration patterns (Authorization.Core.UI.Test.Web)
- TimeSpan API signature changes (3 occurrences across projects)
- Configuration and middleware updates (Authorization.Core.UI.Test.Web)

**Expected Outcome:** Solution builds with 0 errors

### Step 5: Execute Tests

Run all test projects to validate functionality:
```bash
dotnet test Authorization.Core.sln
```

**Test Projects:**
- Authorization.Core.Tests.csproj
- Authorization.Core.UI.Tests.csproj
- Authorization.Core.UI.Tests.Integration.csproj
- Authorization.Core.UI.Tests.Playwright.csproj

**Expected Outcome:** All tests pass

## Package Update Reference

### Common Package Updates (Affecting Multiple Projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | 2 projects | .NET 9 compatibility |
| Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | 2 projects | .NET 9 compatibility |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | 2 projects | .NET 9 compatibility |
| Microsoft.SourceLink.GitHub | 8.0.0 | *no change* | 2 projects | Already compatible |

### Category-Specific Updates

**ASP.NET Core Packages** (Authorization.Core.UI.Test.Web):
- Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore: 8.0.14 → 9.0.12
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: 8.0.14 → 9.0.12
- Microsoft.AspNetCore.Identity.UI: 8.0.14 → 9.0.12
- Microsoft.AspNetCore.Mvc.Testing: 8.0.14 → 9.0.12

**Entity Framework Packages** (Authorization.Core.UI.Test.Web):
- Microsoft.EntityFrameworkCore.Sqlite: 8.0.14 → 9.0.12
- Microsoft.EntityFrameworkCore.Tools: 8.0.14 → 9.0.12

**Identity Packages** (Authorization.Core.UI.Tests):
- Microsoft.Extensions.Identity.Core: 8.0.14 → 9.0.12

**CRFricke Packages** (User-Specified Versions):
- CRFricke.EF.Core.Utilities: 8.0.1 → 9.0.0 (Authorization.Core)
- CRFricke.Test.Support: 8.0.1 → 9.0.0 (Authorization.Core.UI.Tests)

### Packages Remaining Unchanged

These packages are already compatible with .NET 9.0:

| Package | Version | Projects |
|---------|---------|----------|
| coverlet.collector | 6.0.4 | 4 test projects |
| Microsoft.Extensions.Diagnostics.Testing | 8.10.0 | 2 test projects |
| Microsoft.NET.Test.Sdk | 17.13.0 | 4 test projects |
| Microsoft.Playwright.NUnit | 1.50.0 | 1 project |
| Microsoft.Playwright.TestAdapter | 1.50.0 | 1 project |
| MockQueryable.Moq | 7.0.3 | 2 test projects |
| Moq | 4.20.72 | 2 test projects |
| NUnit | 4.3.2 | 1 project |
| NUnit.Analyzers | 4.6.0 | 1 project |
| NUnit3TestAdapter | 5.0.0 | 1 project |
| xunit | 2.9.3 | 3 test projects (⚠️ deprecated but functional) |
| xunit.runner.visualstudio | 3.0.2 | 3 test projects |

### Project-Specific Package Update Matrix

#### Authorization.Core.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| CRFricke.EF.Core.Utilities | 8.0.1 | 9.0.0 | User requirement - align with .NET 9 |
| Microsoft.SourceLink.GitHub | 8.0.0 | *no change* | Compatible |

#### Authorization.Core.UI.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| Microsoft.SourceLink.GitHub | 8.0.0 | *no change* | Compatible |

#### Authorization.Core.UI.Test.Web.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| Microsoft.EntityFrameworkCore.Tools | 8.0.14 | 9.0.12 | .NET 9 compatibility |

#### Authorization.Core.Tests.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| *All packages compatible* | - | - | No updates needed |

#### Authorization.Core.UI.Tests.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.Extensions.Identity.Core | 8.0.14 | 9.0.12 | .NET 9 compatibility |
| CRFricke.Test.Support | 8.0.1 | 9.0.0 | User requirement - align with .NET 9 |

#### Authorization.Core.UI.Tests.Integration.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | .NET 9 compatibility |

#### Authorization.Core.UI.Tests.Playwright.csproj
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | .NET 9 compatibility |

### Special Considerations

**Deprecated xunit Package:**
- The xunit package (2.9.3) is marked as deprecated
- Used in: Authorization.Core.Tests, Authorization.Core.UI.Tests, Authorization.Core.UI.Tests.Integration
- **Recommendation:** Package remains functional for .NET 9; no immediate action required
- **Future Consideration:** Plan migration to xunit v3 in a future update

## Breaking Changes Catalog

### Overview

The assessment identified **13 source-incompatible APIs** that require recompilation. These are categorized as "Source Incompatible," meaning the code will need to be recompiled and may require minor adjustments, but no major architectural changes are expected.

**API Compatibility Summary:**
- 🔴 Binary Incompatible: 0 (High - would require code changes)
- 🟡 Source Incompatible: 13 (Medium - needs re-compilation, potential minor fixes)
- 🔵 Behavioral Changes: 0 (Low - runtime behavior changes)
- ✅ Compatible: 54,576 (99.98% of APIs)

### Source-Incompatible APIs (13 occurrences)

#### 1. System.TimeSpan.FromMinutes (3 occurrences)
**Locations:**
- Authorization.Core.UI.Test.Web\AppGuids.cs (2 occurrences)
- Authorization.Core.UI\Areas\Authorization\Pages\V5\User\Index.cshtml (1 occurrence)

**Issue:** Method signature or behavior changes in .NET 9  
**Impact:** Low - likely implicit conversion changes  
**Resolution:** Review usages; may require explicit type casting or updated parameter types  
**Reference:** https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0

#### 2. ASP.NET Core Identity UI APIs (7 occurrences)

##### UIFrameworkAttribute (2 occurrences)
- `Microsoft.AspNetCore.Identity.UI.UIFrameworkAttribute` (Type)
- `Microsoft.AspNetCore.Identity.UI.UIFrameworkAttribute.UIFramework` (Property)

**Locations:** Authorization.Core.UI.Test.Web\Program.cs  
**Issue:** Identity UI framework configuration API changes  
**Impact:** Low - configuration-related  
**Resolution:** Update Identity UI configuration to use .NET 9 patterns  
**Reference:** https://learn.microsoft.com/en-us/aspnet/core/migration/80-90

##### MigrationsEndPointExtensions (2 occurrences)
- `Microsoft.AspNetCore.Builder.MigrationsEndPointExtensions` (Type)
- `Microsoft.AspNetCore.Builder.MigrationsEndPointExtensions.UseMigrationsEndPoint` (Method)

**Locations:** Authorization.Core.UI.Test.Web\Program.cs  
**Issue:** EF Core migrations endpoint configuration changes  
**Impact:** Low - development-time feature  
**Resolution:** Update middleware configuration; likely minor syntax change  
**Reference:** https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes

##### IdentityServiceCollectionUIExtensions (2 occurrences)
- `Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions` (Type)
- `Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions.AddDefaultIdentity` (Method)

**Locations:** Authorization.Core.UI.Test.Web\Program.cs  
**Issue:** Identity service registration API changes  
**Impact:** Low - service configuration  
**Resolution:** Update service registration to .NET 9 pattern  
**Reference:** https://learn.microsoft.com/en-us/aspnet/core/migration/80-90#identity

##### IdentityEntityFrameworkBuilderExtensions (2 occurrences)
- `Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions` (Type)
- `Microsoft.Extensions.DependencyInjection.IdentityEntityFrameworkBuilderExtensions.AddEntityFrameworkStores` (Method)

**Locations:** Authorization.Core.UI.Test.Web\Program.cs  
**Issue:** Identity EF store configuration changes  
**Impact:** Low - EF store registration  
**Resolution:** Update EF store registration to .NET 9 pattern  
**Reference:** https://learn.microsoft.com/en-us/aspnet/core/migration/80-90#identity

##### DatabaseDeveloperPageExceptionFilterServiceExtensions (2 occurrences)
- `Microsoft.Extensions.DependencyInjection.DatabaseDeveloperPageExceptionFilterServiceExtensions` (Type)
- `Microsoft.Extensions.DependencyInjection.DatabaseDeveloperPageExceptionFilterServiceExtensions.AddDatabaseDeveloperPageExceptionFilter` (Method)

**Locations:** Authorization.Core.UI.Test.Web\Program.cs  
**Issue:** Developer exception page configuration changes  
**Impact:** Low - development-time feature  
**Resolution:** Update developer exception page registration  
**Reference:** https://learn.microsoft.com/en-us/aspnet/core/migration/80-90

### Expected Breaking Change Categories

Based on .NET 8 → .NET 9 migration patterns, expect potential issues in these areas:

#### ASP.NET Core Identity Configuration
**Affected Project:** Authorization.Core.UI.Test.Web  
**File:** Program.cs  
**Expected Changes:**
- Identity service registration syntax updates
- UI framework configuration adjustments
- Middleware registration order or syntax changes

**Example Migration Pattern:**
```csharp
// .NET 8
services.AddDefaultIdentity<IdentityUser>(options => ...)
    .AddEntityFrameworkStores<AppDbContext>();

// .NET 9 (potential change - verify during compilation)
services.AddIdentity<IdentityUser, IdentityRole>(options => ...)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI();
```

#### Entity Framework Core Migrations
**Affected Project:** Authorization.Core.UI.Test.Web  
**File:** Program.cs  
**Expected Changes:**
- Migrations endpoint configuration updates
- Developer exception page registration syntax

#### TimeSpan API Usage
**Affected Projects:** Authorization.Core.UI.Test.Web, Authorization.Core.UI  
**Files:** AppGuids.cs, User\Index.cshtml  
**Expected Changes:**
- Explicit type casting for TimeSpan parameters
- Updated method signatures

### Breaking Changes Documentation References

- **General .NET 9 Breaking Changes:** https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0
- **ASP.NET Core 8 to 9 Migration:** https://learn.microsoft.com/en-us/aspnet/core/migration/80-90
- **Entity Framework Core 9 Breaking Changes:** https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes

### Code Review Focus Areas

When addressing compilation errors, prioritize review in this order:

1. **Authorization.Core.UI.Test.Web\Program.cs** - Most Identity and EF configuration issues
2. **Authorization.Core.UI.Test.Web\AppGuids.cs** - TimeSpan API issues
3. **Authorization.Core.UI\Areas\Authorization\Pages\V5\User\Index.cshtml** - TimeSpan API issues
4. **Other files** - Unlikely to have issues based on assessment

### Migration Notes

- **No Binary-Incompatible Changes:** All APIs can be recompiled; no forced architectural changes
- **Well-Documented Paths:** ASP.NET Core Identity and EF Core migrations are well-documented
- **Low Risk:** 13 issues out of 54,589 APIs analyzed (0.02% incompatibility rate)
- **Concentration:** Most issues in single file (Program.cs) simplifies troubleshooting

## Project-by-Project Migration Plans

### Project: Authorization.Core

**Location:** Authorization.Core\Authorization.Core.csproj  
**Type:** ClassLibrary (SDK-style)  
**Dependency Level:** 0 (Foundation - no project dependencies)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** None
- **Dependants:** 2 projects (Authorization.Core.Tests, Authorization.Core.UI)
- **Package Count:** 3 packages
- **Lines of Code:** 2,095
- **Files with Issues:** 2
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 2 packages

#### Migration Steps

1. **Prerequisites**
   - ✅ No project dependencies to wait for
   - ✅ Foundation library - can upgrade independently in All-At-Once approach

2. **Update Target Framework**
   - File: `Authorization.Core\Authorization.Core.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | CRFricke.EF.Core.Utilities | 8.0.1 | 9.0.0 | User requirement |
   | Microsoft.SourceLink.GitHub | 8.0.0 | *no change* | Already compatible |

4. **Expected Breaking Changes**
   - **API Issues:** 2 source-incompatible APIs
   - **Likely Locations:** TimeSpan API usage, Identity/EF Core patterns
   - **Resolution:** Review compilation errors; likely minor adjustments

5. **Code Modifications**
   - Review files identified with issues (2 files)
   - Update API calls to align with .NET 9 signatures
   - Ensure no obsolete API usage

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core\Authorization.Core.csproj`
   - Verify 0 errors, 0 warnings
   - Run dependent test project: Authorization.Core.Tests

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Dependent test project passes

---

### Project: Authorization.Core.Tests

**Location:** Authorization.Core.Tests\Authorization.Core.Tests.csproj  
**Type:** DotNetCoreApp (Test Project, SDK-style)  
**Dependency Level:** 1 (depends on Authorization.Core)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core)
- **Dependants:** None (top-level test project)
- **Package Count:** 6 packages
- **Lines of Code:** 1,105
- **Files with Issues:** 1
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 0 (all packages compatible)

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.Tests\Authorization.Core.Tests.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   - ✅ All packages already compatible - no updates needed
   - Note: xunit 2.9.3 is deprecated but functional

4. **Expected Breaking Changes**
   - **API Issues:** 0 source-incompatible APIs
   - **Resolution:** Minimal changes expected; recompilation should succeed

5. **Code Modifications**
   - Unlikely to need changes
   - Review 1 file with issues if compilation errors occur

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.Tests\Authorization.Core.Tests.csproj`
   - Run tests: `dotnet test Authorization.Core.Tests\Authorization.Core.Tests.csproj`
   - Verify all tests pass

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass
   - [ ] No test coverage regression

---

### Project: Authorization.Core.UI

**Location:** Authorization.Core.UI\Authorization.Core.UI.csproj  
**Type:** ClassLibrary (SDK-style)  
**Dependency Level:** 1 (depends on Authorization.Core)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core)
- **Dependants:** 2 projects (Authorization.Core.UI.Test.Web, Authorization.Core.UI.Tests)
- **Package Count:** 2 packages
- **Lines of Code:** 3,716
- **Files with Issues:** 2
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 1 package

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.UI\Authorization.Core.UI.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | Microsoft.SourceLink.GitHub | 8.0.0 | *no change* | Already compatible |

4. **Expected Breaking Changes**
   - **API Issues:** 2 source-incompatible APIs
   - **Likely Locations:** Identity UI components, TimeSpan usage in User management
   - **Resolution:** Update Identity UI patterns to .NET 9; review TimeSpan API calls

5. **Code Modifications**
   - Review 2 files with issues
   - Focus: Areas\Authorization\Pages\V5\User\Index.cshtml (TimeSpan.FromMinutes)
   - Update Identity UI integration patterns if needed

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.UI\Authorization.Core.UI.csproj`
   - Verify 0 errors, 0 warnings
   - Run dependent test project: Authorization.Core.UI.Tests

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Dependent test projects pass

---

### Project: Authorization.Core.UI.Tests

**Location:** Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj  
**Type:** DotNetCoreApp (Test Project, SDK-style)  
**Dependency Level:** 1 (depends on Authorization.Core.UI)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core.UI)
- **Dependants:** None (top-level test project)
- **Package Count:** 7 packages
- **Lines of Code:** 3,007
- **Files with Issues:** 1
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 2 packages

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core.UI (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.Extensions.Identity.Core | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | CRFricke.Test.Support | 8.0.1 | 9.0.0 | User requirement |

4. **Expected Breaking Changes**
   - **API Issues:** 0 source-incompatible APIs
   - **Resolution:** Minimal changes expected; package updates should integrate smoothly

5. **Code Modifications**
   - Review 1 file with issues if compilation errors occur
   - Update test assertions if Identity behavior changes

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj`
   - Run tests: `dotnet test Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj`
   - Verify all tests pass

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass
   - [ ] No test coverage regression

---

### Project: Authorization.Core.UI.Test.Web

**Location:** Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj  
**Type:** AspNetCore (Web Application, SDK-style)  
**Dependency Level:** 2 (depends on Authorization.Core.UI)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core.UI)
- **Dependants:** 2 projects (Authorization.Core.UI.Tests.Integration, Authorization.Core.UI.Tests.Playwright)
- **Package Count:** 5 packages requiring updates
- **Lines of Code:** 1,993
- **Files with Issues:** 2
- **Risk Level:** 🟡 Medium

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 5 packages

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core.UI (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | Microsoft.AspNetCore.Identity.UI | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | Microsoft.EntityFrameworkCore.Sqlite | 8.0.14 | 9.0.12 | .NET 9 compatibility |
   | Microsoft.EntityFrameworkCore.Tools | 8.0.14 | 9.0.12 | .NET 9 compatibility |

4. **Expected Breaking Changes**
   - **API Issues:** 9 source-incompatible APIs (most in solution)
   - **Primary Location:** Program.cs (ASP.NET Core Identity and EF configuration)
   - **Categories:**
     - Identity service registration (AddDefaultIdentity, AddEntityFrameworkStores)
     - Migrations endpoint configuration (UseMigrationsEndPoint)
     - Developer exception page (AddDatabaseDeveloperPageExceptionFilter)
     - TimeSpan API (AppGuids.cs)
   - **Resolution:** Update startup configuration to .NET 9 patterns

5. **Code Modifications**
   **High Priority - Program.cs:**
   - Update Identity service registration syntax
   - Update EF migrations endpoint configuration
   - Update developer exception page registration
   - Review middleware pipeline configuration

   **Medium Priority - AppGuids.cs:**
   - Fix TimeSpan.FromMinutes calls (2 occurrences)
   - Add explicit type casting if needed

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj`
   - Verify 0 errors, 0 warnings
   - Manual smoke test: Start application, verify basic functionality
   - Run dependent test projects: Integration and Playwright tests

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All 5 packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application starts successfully
   - [ ] Identity functionality works (login, register)
   - [ ] Database migrations work
   - [ ] Dependent test projects pass

---

### Project: Authorization.Core.UI.Tests.Integration

**Location:** Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj  
**Type:** DotNetCoreApp (Integration Test Project, SDK-style)  
**Dependency Level:** 3 (depends on Authorization.Core.UI.Test.Web)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core.UI.Test.Web)
- **Dependants:** None (top-level test project)
- **Package Count:** 4 packages
- **Lines of Code:** 325
- **Files with Issues:** 1
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 1 package

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core.UI.Test.Web (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | .NET 9 compatibility |

4. **Expected Breaking Changes**
   - **API Issues:** 0 source-incompatible APIs
   - **Resolution:** Package update should integrate seamlessly

5. **Code Modifications**
   - Review 1 file with issues if compilation errors occur
   - Update test host configuration if WebApplicationFactory behavior changes

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj`
   - Run tests: `dotnet test Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj`
   - Verify all integration tests pass

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All integration tests pass
   - [ ] WebApplicationFactory works correctly

---

### Project: Authorization.Core.UI.Tests.Playwright

**Location:** Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj  
**Type:** DotNetCoreApp (E2E Test Project, SDK-style)  
**Dependency Level:** 3 (depends on Authorization.Core.UI.Test.Web)

#### Current State
- **Target Framework:** net8.0
- **Project Dependencies:** 1 (Authorization.Core.UI.Test.Web)
- **Dependants:** None (top-level test project)
- **Package Count:** 5 packages
- **Lines of Code:** 1,374
- **Files with Issues:** 1
- **Risk Level:** 🟢 Low

#### Target State
- **Target Framework:** net9.0
- **Package Updates:** 1 package

#### Migration Steps

1. **Prerequisites**
   - Depends on: Authorization.Core.UI.Test.Web (upgraded simultaneously in All-At-Once)

2. **Update Target Framework**
   - File: `Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj`
   - Change: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`

3. **Update Package References**
   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|--------|
   | Microsoft.AspNetCore.Mvc.Testing | 8.0.14 | 9.0.12 | .NET 9 compatibility |

4. **Expected Breaking Changes**
   - **API Issues:** 0 source-incompatible APIs
   - **Resolution:** Package update should integrate seamlessly

5. **Code Modifications**
   - Review 1 file with issues if compilation errors occur
   - Update WebApplicationFactory configuration if needed
   - Verify Playwright browser automation compatibility

6. **Testing Strategy**
   - Build project: `dotnet build Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj`
   - Run tests: `dotnet test Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj`
   - Verify all E2E tests pass
   - Verify browser automation works correctly

7. **Validation Checklist**
   - [ ] Project file updated to net9.0
   - [ ] All packages updated to specified versions
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All E2E tests pass
   - [ ] Playwright browser automation works
   - [ ] WebApplicationFactory works correctly

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| Authorization.Core.UI.Test.Web | 🟡 Medium | ASP.NET Core application with 9 source-incompatible APIs; most complex project in solution | Review breaking changes documentation; test thoroughly; have rollback plan ready |
| All Test Projects | 🟢 Low | xunit package (2.9.3) is deprecated | Package still functional; monitor for future replacement; consider migration to xunit v3 in future |
| Authorization.Core | 🟢 Low | Foundation library; 2 source-incompatible APIs | Changes likely TimeSpan-related; low impact |
| Authorization.Core.UI | 🟢 Low | UI library; 2 source-incompatible APIs | Identity UI changes; well-documented migration |

### Security Vulnerabilities

✅ **No security vulnerabilities detected** in current package versions.

### Contingency Plans

**If Blocking Compilation Errors:**
1. Review error messages against Breaking Changes Catalog
2. Consult .NET 9 migration documentation
3. Check for conditional compilation directives needing updates
4. If unresolvable: revert to assessment for deeper analysis

**If Package Incompatibility:**
1. Verify package versions are correct (.NET 9 compatible)
2. Check for transitive dependency conflicts
3. Review package release notes for breaking changes
4. If blocking: temporarily pin to compatible version, file issue

**If Test Failures:**
1. Determine if failure is upgrade-related or pre-existing
2. Check for behavioral changes in .NET 9
3. Review test assertions for framework-specific assumptions
4. Update tests to align with .NET 9 behavior if appropriate

**If Performance Regression:**
1. Benchmark critical paths before/after upgrade
2. Review .NET 9 performance characteristics
3. Profile to identify regression source
4. Optimize or file issue with .NET team

**Rollback Strategy:**
- All work on dedicated branch (upgrade-to-NET9)
- Can abandon branch and return to master if needed
- Git history preserved for analysis

## Testing & Validation Strategy

### Multi-Level Testing Approach

Given the All-At-Once strategy, testing occurs after all projects are upgraded and the solution builds successfully.

### Level 1: Build Verification

**Objective:** Ensure all projects compile without errors after framework and package updates.

**Steps:**
1. Restore dependencies: `dotnet restore Authorization.Core.sln`
2. Build solution: `dotnet build Authorization.Core.sln --configuration Release`
3. Verify output: 0 errors, 0 warnings (target)

**Success Criteria:**
- [ ] All 7 projects build successfully
- [ ] No compilation errors
- [ ] No new warnings introduced (stretch goal: 0 warnings)
- [ ] All package dependencies resolved correctly

### Level 2: Unit Testing

**Objective:** Verify core functionality and business logic unaffected by upgrade.

**Test Projects:**
- Authorization.Core.Tests (xUnit)
- Authorization.Core.UI.Tests (xUnit)

**Steps:**
```bash
dotnet test Authorization.Core.Tests\Authorization.Core.Tests.csproj --configuration Release
dotnet test Authorization.Core.UI.Tests\Authorization.Core.UI.Tests.csproj --configuration Release
```

**Success Criteria:**
- [ ] All unit tests pass
- [ ] Test execution time comparable to .NET 8 baseline
- [ ] No test coverage regression
- [ ] No new test failures

### Level 3: Integration Testing

**Objective:** Verify ASP.NET Core application and Identity integration work correctly.

**Test Project:**
- Authorization.Core.UI.Tests.Integration (xUnit)

**Steps:**
```bash
dotnet test Authorization.Core.UI.Tests.Integration\Authorization.Core.UI.Tests.Integration.csproj --configuration Release
```

**Focus Areas:**
- WebApplicationFactory initialization
- ASP.NET Core Identity integration
- Entity Framework Core database operations
- HTTP request/response handling
- Authentication/authorization flows

**Success Criteria:**
- [ ] All integration tests pass
- [ ] WebApplicationFactory creates test server successfully
- [ ] Database operations work correctly
- [ ] Identity features functional

### Level 4: End-to-End Testing

**Objective:** Verify complete application workflows through browser automation.

**Test Project:**
- Authorization.Core.UI.Tests.Playwright (NUnit)

**Steps:**
```bash
dotnet test Authorization.Core.UI.Tests.Playwright\Authorization.Core.UI.Tests.Playwright.csproj --configuration Release
```

**Focus Areas:**
- Browser automation compatibility
- Full user workflows (login, registration, authorization)
- UI rendering and interaction
- JavaScript functionality

**Success Criteria:**
- [ ] All Playwright tests pass
- [ ] Browser automation works correctly
- [ ] UI renders correctly in target browsers
- [ ] User workflows complete successfully

### Level 5: Manual Smoke Testing

**Objective:** Verify application behavior in real-world usage.

**Application:** Authorization.Core.UI.Test.Web

**Steps:**
1. Start application: `dotnet run --project Authorization.Core.UI.Test.Web\Authorization.Core.UI.Test.Web.csproj`
2. Navigate to application URL (typically https://localhost:5001)
3. Execute smoke test scenarios

**Smoke Test Scenarios:**
- [ ] Application starts without errors
- [ ] Home page loads correctly
- [ ] User registration works
- [ ] User login works
- [ ] Authorization checks function correctly
- [ ] Database migrations apply successfully
- [ ] No console errors or warnings

### Comprehensive Test Execution

Run all tests in single command:
```bash
dotnet test Authorization.Core.sln --configuration Release
```

**Expected Output:**
- Total tests: [number based on test projects]
- Passed: 100%
- Failed: 0
- Skipped: 0

### Validation Checklist

#### Technical Validation
- [ ] All projects target net9.0
- [ ] All packages updated to specified versions
- [ ] Solution builds with 0 errors
- [ ] Solution builds with 0 warnings (target)
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] All E2E tests pass
- [ ] No dependency conflicts
- [ ] No security vulnerabilities remain

#### Functional Validation
- [ ] ASP.NET Core Identity works correctly
- [ ] Entity Framework Core operations successful
- [ ] User authentication functions properly
- [ ] User authorization functions properly
- [ ] Database migrations work
- [ ] UI renders correctly

#### Performance Validation
- [ ] Build time comparable to .NET 8 baseline
- [ ] Test execution time comparable to baseline
- [ ] Application startup time acceptable
- [ ] No performance regressions observed

### Regression Testing Focus

**Areas to Monitor:**
1. **Identity Operations:** Login, logout, registration, password reset
2. **Authorization Rules:** Role-based and claims-based authorization
3. **Entity Framework:** CRUD operations, migrations, queries
4. **UI Components:** Razor Pages rendering, JavaScript functionality
5. **Test Infrastructure:** xUnit and NUnit framework compatibility

### Test Failure Troubleshooting

If tests fail after upgrade:

1. **Identify Pattern:**
   - Single test failure → likely test-specific issue
   - Multiple related failures → likely framework behavior change
   - All tests in project fail → likely configuration or infrastructure issue

2. **Review Changes:**
   - Check .NET 9 behavioral changes documentation
   - Review test assertions for framework-specific assumptions
   - Verify test data and fixtures compatibility

3. **Isolate Issue:**
   - Run failing test in isolation
   - Enable detailed logging
   - Compare behavior to .NET 8 baseline

4. **Resolution Paths:**
   - Update test to align with .NET 9 behavior (preferred)
   - Fix application code if regression identified
   - Report issue to .NET team if suspected framework bug

### Test Execution Order

For manual testing or troubleshooting, execute tests in dependency order:

1. **Authorization.Core.Tests** (foundation library tests)
2. **Authorization.Core.UI.Tests** (UI library tests)
3. **Authorization.Core.UI.Tests.Integration** (web app integration tests)
4. **Authorization.Core.UI.Tests.Playwright** (E2E tests)

This order ensures issues are identified at the lowest level first, simplifying root cause analysis.

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Dependencies | Package Updates | API Issues | Risk Factors |
|---------|-----------|--------------|-----------------|------------|--------------|
| Authorization.Core | 🟢 Low | 0 projects | 2 | 2 | Foundation library; straightforward upgrade |
| Authorization.Core.Tests | 🟢 Low | 1 project | 0 | 0 | Test project; minimal changes |
| Authorization.Core.UI | 🟢 Low | 1 project | 1 | 2 | UI library; Identity changes |
| Authorization.Core.UI.Tests | 🟢 Low | 1 project | 2 | 0 | Test project; package updates only |
| Authorization.Core.UI.Test.Web | 🟡 Medium | 1 project | 5 | 9 | ASP.NET Core app; most API changes |
| Authorization.Core.UI.Tests.Integration | 🟢 Low | 1 project | 1 | 0 | Integration test project |
| Authorization.Core.UI.Tests.Playwright | 🟢 Low | 1 project | 1 | 0 | E2E test project |

### All-At-Once Complexity Assessment

**Overall Complexity: 🟢 Low to 🟡 Medium**

**Factors Contributing to Low Complexity:**
- Small solution size (7 projects)
- All projects already on modern .NET (8.0)
- Clean dependency structure
- All packages have clear upgrade paths
- No security vulnerabilities to address urgently
- Low percentage of incompatible APIs (0.02%)
- Good test coverage for validation

**Factors Contributing to Medium Complexity:**
- ASP.NET Core Identity and EF Core changes concentrated in one project
- Deprecated xunit package (though still functional)
- Source-incompatible APIs requiring code review
- Multiple test frameworks (xUnit, NUnit) to validate

### Resource Requirements

**Skills Needed:**
- .NET Core/ASP.NET Core development experience
- Familiarity with Entity Framework Core
- Understanding of ASP.NET Core Identity
- Experience with xUnit and NUnit test frameworks
- Git and branching workflows

**Parallel Work Capacity:**
Since All-At-Once strategy updates everything simultaneously:
- **File Updates:** Can be automated/batched (single developer)
- **Build & Fix:** Sequential; requires developer attention to compilation errors
- **Testing:** Can parallelize test execution; sequential failure analysis

**Estimated Relative Effort:**
- Framework updates: Low (automated find/replace)
- Package updates: Low (version number changes)
- Breaking changes: Low-Medium (13 API issues, well-documented)
- Testing: Medium (comprehensive test suite to execute and validate)

## Source Control Strategy

### Branch Strategy

**Current Setup:**
- Main branch: `master`
- Upgrade branch: `upgrade-to-NET9` (already created and active)
- All upgrade work performed on upgrade branch

### Commit Strategy

**Recommended Approach for All-At-Once Strategy: Single Atomic Commit**

Since all changes are interdependent and the solution must be upgraded as a unit, a single comprehensive commit is recommended.

**Single Commit Structure:**
```
feat: Upgrade solution to .NET 9.0

- Update all 7 projects from net8.0 to net9.0
- Update Microsoft.AspNetCore.* packages from 8.0.14 to 9.0.12
- Update Microsoft.EntityFrameworkCore.* packages from 8.0.14 to 9.0.12
- Update CRFricke.EF.Core.Utilities from 8.0.1 to 9.0.0
- Update CRFricke.Test.Support from 8.0.1 to 9.0.0
- Fix ASP.NET Core Identity configuration for .NET 9
- Fix TimeSpan API usage for .NET 9
- Update EF Core migrations endpoint configuration
- All tests passing

BREAKING CHANGE: Upgrades to .NET 9.0
```

**Alternative: Checkpoint Commits (if needed for safety)**

If you prefer intermediate checkpoints:

1. **Commit 1:** Project file updates
   ```
   chore: Update all projects to net9.0 target framework
   ```

2. **Commit 2:** Package updates
   ```
   chore: Update all packages to .NET 9 compatible versions
   ```

3. **Commit 3:** Breaking change fixes
   ```
   fix: Resolve .NET 9 compilation errors and breaking changes
   ```

4. **Commit 4:** Test validation (if test fixes needed)
   ```
   test: Update tests for .NET 9 compatibility
   ```

### Review and Merge Process

**Pull Request Requirements:**

**Title:**
```
Upgrade Authorization.Core solution to .NET 9.0
```

**Description Template:**
```markdown
## Overview
Upgrades the entire Authorization.Core solution from .NET 8.0 to .NET 9.0 using All-At-Once strategy.

## Changes
- ✅ All 7 projects upgraded to net9.0
- ✅ 10 packages updated to .NET 9 compatible versions
- ✅ ASP.NET Core Identity configuration updated
- ✅ Entity Framework Core migrations endpoint updated
- ✅ TimeSpan API usage fixed
- ✅ All tests passing

## Verification
- [x] Solution builds with 0 errors, 0 warnings
- [x] All unit tests pass (Authorization.Core.Tests, Authorization.Core.UI.Tests)
- [x] All integration tests pass (Authorization.Core.UI.Tests.Integration)
- [x] All E2E tests pass (Authorization.Core.UI.Tests.Playwright)
- [x] Application starts and runs correctly
- [x] No security vulnerabilities

## Testing Performed
- Unit testing: [X passed, 0 failed]
- Integration testing: [X passed, 0 failed]
- E2E testing: [X passed, 0 failed]
- Manual smoke testing: ✅ Passed

## Breaking Changes
- Requires .NET 9 SDK for development and deployment
- ASP.NET Core Identity configuration syntax updated
- Entity Framework Core migrations configuration updated

## Migration Notes
[Any team-specific notes about the upgrade]

## Related Documentation
- Assessment: `.github/upgrades/scenarios/new-dotnet-version_b6bf5d/assessment.md`
- Plan: `.github/upgrades/scenarios/new-dotnet-version_b6bf5d/plan.md`
```

**PR Checklist:**
- [ ] All projects build successfully
- [ ] All tests pass
- [ ] No new warnings introduced
- [ ] Documentation updated
- [ ] CI/CD pipeline verified (if applicable)
- [ ] .NET 9 SDK available in deployment environments
- [ ] Team notified of .NET 9 requirement

### Merge Criteria

**Ready to Merge When:**
1. ✅ All technical success criteria met (see Success Criteria section)
2. ✅ PR approved by required reviewers
3. ✅ CI/CD pipeline passes (if applicable)
4. ✅ No merge conflicts with master
5. ✅ Deployment environment ready (.NET 9 SDK installed)

### Post-Merge Actions

1. **Tag Release:**
   ```bash
   git tag -a v[version]-net9.0 -m "Upgraded to .NET 9.0"
   git push origin v[version]-net9.0
   ```

2. **Update CI/CD:**
   - Verify build agents have .NET 9 SDK
   - Update Docker base images to .NET 9
   - Update deployment scripts if needed

3. **Team Communication:**
   - Notify team of merge
   - Document any new development requirements (.NET 9 SDK)
   - Share migration experience and lessons learned

4. **Cleanup:**
   - Delete upgrade branch after successful deployment (optional)
   - Archive assessment and plan documents for reference

### Rollback Plan

If critical issues discovered after merge:

**Option 1: Revert Merge Commit**
```bash
git revert -m 1 [merge-commit-hash]
```

**Option 2: Hotfix on .NET 8**
- Create hotfix branch from pre-upgrade commit
- Apply critical fix
- Deploy hotfix while .NET 9 issues investigated

**Option 3: Fast-Forward to .NET 8**
- Temporarily revert to last .NET 8 commit in production
- Keep .NET 9 branch for continued development/fixing

## Success Criteria

### Technical Criteria

The .NET 9 upgrade is technically successful when:

#### Framework Migration
- [x] All 7 projects updated to `<TargetFramework>net9.0</TargetFramework>`
- [x] No projects remain on net8.0
- [x] No mixed framework versions in solution

#### Package Updates
- [x] All 10 required packages updated to specified versions:
  - Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore: 9.0.12
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore: 9.0.12
  - Microsoft.AspNetCore.Identity.UI: 9.0.12
  - Microsoft.AspNetCore.Mvc.Testing: 9.0.12
  - Microsoft.EntityFrameworkCore.Sqlite: 9.0.12
  - Microsoft.EntityFrameworkCore.Tools: 9.0.12
  - Microsoft.Extensions.Identity.Core: 9.0.12
  - CRFricke.EF.Core.Utilities: 9.0.0
  - CRFricke.Test.Support: 9.0.0
- [x] No package dependency conflicts
- [x] All package dependencies successfully restored

#### Build Success
- [x] Solution builds without errors: `dotnet build Authorization.Core.sln`
- [x] All 7 projects compile successfully
- [x] Build output shows 0 errors
- [x] Target: Build output shows 0 warnings (stretch goal)
- [x] Release configuration builds successfully

#### Test Success
- [x] All unit tests pass: Authorization.Core.Tests, Authorization.Core.UI.Tests
- [x] All integration tests pass: Authorization.Core.UI.Tests.Integration
- [x] All E2E tests pass: Authorization.Core.UI.Tests.Playwright
- [x] Test execution: `dotnet test Authorization.Core.sln` shows 100% pass rate
- [x] No test coverage regression
- [x] No new test failures introduced

#### Breaking Changes Resolved
- [x] All 13 source-incompatible APIs addressed
- [x] ASP.NET Core Identity configuration updated (Program.cs)
- [x] Entity Framework migrations endpoint updated
- [x] TimeSpan API usage fixed (3 occurrences)
- [x] No compilation errors related to framework APIs

#### Security & Quality
- [x] No security vulnerabilities in upgraded packages
- [x] No new static analysis warnings
- [x] No deprecated API usage (except xunit - acceptable)

### Functional Criteria

The upgrade is functionally successful when:

#### Application Functionality
- [x] Authorization.Core.UI.Test.Web application starts successfully
- [x] No runtime exceptions during startup
- [x] Application serves requests correctly

#### Identity & Authentication
- [x] User registration works
- [x] User login works
- [x] User logout works
- [x] Password reset/change works
- [x] Identity cookie authentication works

#### Authorization
- [x] Role-based authorization works
- [x] Claims-based authorization works
- [x] Authorization policies enforce correctly

#### Database Operations
- [x] Entity Framework Core CRUD operations work
- [x] Database migrations apply successfully
- [x] SQLite database operations functional
- [x] No data loss or corruption

#### UI Functionality
- [x] Razor Pages render correctly
- [x] Identity UI pages work
- [x] JavaScript functionality works
- [x] CSS styling applies correctly

### Process Criteria

The upgrade process is complete when:

#### Documentation
- [x] All changes committed to upgrade-to-NET9 branch
- [x] Commit messages clearly describe changes
- [x] Assessment document preserved
- [x] Plan document preserved
- [x] Migration notes documented for team

#### Source Control
- [x] All changes on upgrade-to-NET9 branch
- [x] No merge conflicts with master
- [x] Ready for pull request creation
- [x] PR description complete with verification checklist

#### Team Readiness
- [x] Team notified of .NET 9 requirement
- [x] Development environment requirements documented
- [x] .NET 9 SDK installation verified/documented
- [x] CI/CD pipeline compatibility verified (if applicable)

### All-At-Once Strategy Specific Criteria

Given the All-At-Once strategy, additional success criteria:

#### Atomic Update
- [x] All projects upgraded in single coordinated operation
- [x] No intermediate multi-targeting states
- [x] Solution remains in working state throughout (or not applicable during upgrade)

#### Unified Testing
- [x] All test projects validated after complete upgrade
- [x] No phased testing required
- [x] Test results consistent across all test projects

#### Single Deployment Unit
- [x] Entire solution ready for deployment as unit
- [x] No partial deployments possible
- [x] All projects compatible with each other

### Definition of Done

**The .NET 9 upgrade is DONE when:**

1. ✅ All technical criteria met (framework, packages, build, tests, security)
2. ✅ All functional criteria met (application works, features functional)
3. ✅ All process criteria met (documented, committed, PR-ready)
4. ✅ Pull request created and approved
5. ✅ Changes merged to master branch
6. ✅ Deployment environment ready (.NET 9 SDK available)
7. ✅ Team onboarded and aware of changes

**At this point, the Authorization.Core solution is successfully running on .NET 9.0.**
