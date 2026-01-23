# Authorization.Core .NET 9 Upgrade Tasks

## Overview

This document tracks the execution of the Authorization.Core solution upgrade from .NET 8.0 to .NET 9.0. All 7 projects will be upgraded simultaneously in a single atomic operation, followed by comprehensive testing and validation.

**Progress**: 3/4 tasks complete (75%) ![0%](https://progress-bar.xyz/75)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-01-23 22:24)*
**References**: Plan §Implementation Timeline Phase 0

- [✓] (1) Verify .NET 9 SDK installed per Plan §Prerequisites
- [✓] (2) .NET 9 SDK meets minimum requirements (**Verify**)

---

### [✓] TASK-002: Atomic framework and package upgrade with compilation fixes *(Completed: 2026-01-23 22:31)*
**References**: Plan §Implementation Timeline Phase 1, Plan §Detailed Execution Steps, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update TargetFramework from net8.0 to net9.0 in all 7 project files per Plan §Step 1 (Authorization.Core, Authorization.Core.UI, Authorization.Core.UI.Test.Web, Authorization.Core.Tests, Authorization.Core.UI.Tests, Authorization.Core.UI.Tests.Integration, Authorization.Core.UI.Tests.Playwright)
- [✓] (2) All project files updated to net9.0 (**Verify**)
- [✓] (3) Update all 10 package references per Plan §Package Update Reference (Microsoft ASP.NET Core packages 8.0.14→9.0.12, EF Core packages 8.0.14→9.0.12, CRFricke packages 8.0.1→9.0.0)
- [✓] (4) All package references updated to specified versions (**Verify**)
- [✓] (5) Restore all dependencies: `dotnet restore Authorization.Core.sln`
- [✓] (6) All dependencies restored successfully (**Verify**)
- [✓] (7) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog (focus: Program.cs Identity/EF configuration, AppGuids.cs TimeSpan usage, Index.cshtml TimeSpan usage)
- [✓] (8) Solution builds with 0 errors (**Verify**)

---

### [✓] TASK-003: Run full test suite and validate upgrade *(Completed: 2026-01-23 22:31)*
**References**: Plan §Implementation Timeline Phase 2, Plan §Testing & Validation Strategy

- [✓] (1) Run tests in all 4 test projects (Authorization.Core.Tests, Authorization.Core.UI.Tests, Authorization.Core.UI.Tests.Integration, Authorization.Core.UI.Tests.Playwright): `dotnet test Authorization.Core.sln`
- [✓] (2) Fix any test failures (reference Plan §Breaking Changes Catalog for framework behavior changes)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)

---

### [▶] TASK-004: Final commit
**References**: Plan §Source Control Strategy

- [▶] (1) Commit all changes with message: "feat: Upgrade solution to .NET 9.0 - Update all 7 projects from net8.0 to net9.0 - Update Microsoft.AspNetCore.* packages from 8.0.14 to 9.0.12 - Update Microsoft.EntityFrameworkCore.* packages from 8.0.14 to 9.0.12 - Update CRFricke packages from 8.0.1 to 9.0.0 - Fix ASP.NET Core Identity configuration for .NET 9 - Fix TimeSpan API usage - All tests passing"

---






