# Scenario Instructions: .NET Version Upgrade

**Scenario ID**: dotnet-version-upgrade  
**Description**: Upgrade from .NET 9 to .NET 10.0 (LTS)  
**Solution**: C:\Users\Chuck\Source\Repos\Authorization.Core\Authorization.Core.slnx  
**Created**: {date}

---

## Preferences

### Flow Mode
**Mode**: Guided  
**Description**: Pause after each stage (assessment, plan, breakdowns) for user review before proceeding.

### Target Framework
**Target**: .NET 10.0 (LTS)  
**Current**: .NET 9.0

### Source Control
**Source Branch**: master  
**Working Branch**: upgrade-to-NET10  
**Repository Root**: C:\Users\Chuck\Source\Repos\Authorization.Core

---

## User Preferences

### Technical Preferences
- **CRFricke.EF.Core.Utilities**: Use version **10.0.0-beta1.0** (user specified)
- **CRFricke.Test.Support**: Use version **10.0.0-beta1.0** (user specified)

### Execution Style
- Flow mode: Guided (user requested, set during initialization)
- Commit strategy: After Each Task (default for All-At-Once with distinct work phases)

### Custom Instructions
*(Task-specific rules will be added here during execution)*

---

## Key Decisions Log
*(Decisions made during the upgrade will be logged here with dates)*
