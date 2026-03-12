# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade from .NET 9 to .NET 10.0 (LTS)
**Scope**: 7 projects (2 class libraries, 1 Razor Pages app, 4 test projects)

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 7 projects, all on .NET 9, clean dependency structure, straightforward upgrade with routine TFM/package updates.

## Tasks

### 01-prerequisites: Validate Prerequisites

Verify that the development environment is ready for .NET 10 upgrade: SDK installation, global.json compatibility, and any tooling requirements.

**Key concerns**:
- .NET 10.0 SDK must be installed
- global.json files (if present) must not restrict to older SDK versions
- Build tooling compatible with .NET 10

**Done when**: SDK validation passes, no global.json conflicts, environment confirmed ready for .NET 10 builds.

---

### 02-atomic-upgrade: Upgrade All Projects to .NET 10

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

### 03-validation: Test and Validate

Run all test suites to validate that the upgrade maintains functionality and identify any behavioral changes requiring code fixes.

**Test projects**:
- Authorization.Core.Tests
- Authorization.Core.UI.Tests
- Authorization.Core.UI.Tests.Integration
- Authorization.Core.UI.Tests.Playwright

**Key concerns**:
- 11 behavioral changes identified in assessment may affect test outcomes
- Razor Pages app (Authorization.Core.UI.Test.Web) functionality validation
- Integration and end-to-end test scenarios

**Done when**: All test suites pass, no regressions detected, behavioral changes validated and documented.
