# 03-validation: Test and Validate

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

