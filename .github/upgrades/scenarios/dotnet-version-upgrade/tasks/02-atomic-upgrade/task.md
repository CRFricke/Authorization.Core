# 02-atomic-upgrade: Upgrade All Projects to .NET 10

Update target frameworks, package references, and fix compilation issues across all 7 projects in a single atomic operation. This includes updating the 2 CRFricke packages to version 10.0.0-beta1.0 per user preference.

**Affected projects**:
- Authorization.Core (class library)
- Authorization.Core.UI (class library)
- Authorization.Core.Tests (test project)
- Authorization.Core.UI.Test.Web (Razor Pages app)
- Authorization.Core.UI.Tests (test project)
- Authorization.Core.UI.Tests.Integration (test project)
- Authorization.Core.UI.Tests.Playwright (test project)

**Key package updates**:
- Microsoft.AspNetCore.* packages: 9.0.12 → 10.0.5
- Microsoft.EntityFrameworkCore.* packages: 9.0.12 → 10.0.5
- Microsoft.Extensions.Identity.Core: 9.0.12 → 10.0.5
- CRFricke.EF.Core.Utilities: 9.0.0 → 10.0.0-beta1.0 (user specified)
- CRFricke.Test.Support: 9.0.0 → 10.0.0-beta1.0 (user specified)

**Deprecated package to address**:
- xunit (2.9.3) - used in 3 test projects - review replacement or upgrade options

**Code issues to resolve**:
- 13 source incompatibility fixes
- 11 behavioral change validations

**Done when**: All project files updated to net10.0, all packages updated to target versions, solution builds with 0 errors, no unresolved API compatibility issues.

---

