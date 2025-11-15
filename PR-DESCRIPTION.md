## Implement Conflict-Free Changelog Management System

### Summary

This PR implements a GitLab-inspired changelog management system that eliminates merge conflicts on `CHANGELOG.md` by using individual YAML files for changelog entries. The system includes automation scripts, multi-server support, and comprehensive documentation.

### What's Changed

**Core System:**
- ✅ Individual YAML files per changelog entry (timestamped filenames)
- ✅ JSON schema validation for entry consistency
- ✅ Three PowerShell automation scripts for the complete workflow
- ✅ Multi-server support (Azure.Mcp.Server, Fabric.Mcp.Server, etc.)

**Scripts Added:**
- `eng/scripts/New-ChangelogEntry.ps1` - Interactive generator for creating changelog entries
- `eng/scripts/Compile-Changelog.ps1` - Compiles YAML files into CHANGELOG.md
- `eng/scripts/Sync-VsCodeChangelog.ps1` - Syncs main CHANGELOG to VS Code extension CHANGELOG

**Documentation:**
- `docs/changelog-entries.md` - User guide for contributors
- `docs/design/changelog-management-system.md` - Design document and implementation details
- `eng/schemas/changelog-entry.schema.json` - JSON schema for validation

**Infrastructure:**
- Created `changelog-entries/` folders for Fabric.Mcp.Server and Template.Mcp.Server
- Created VS Code CHANGELOG.md files for each server
- Updated CONTRIBUTING.md and AGENTS.md with changelog workflow

### Key Features

**1. No More Merge Conflicts**
Contributors create individual YAML files instead of editing CHANGELOG.md directly:
```yaml
section: "Features Added"
description: "Added support for User-Assigned Managed Identity"
pr: 1033
```

**2. Multi-Server Support**
All scripts accept a `-ServerName` parameter (defaults to `Azure.Mcp.Server`):
```powershell
./eng/scripts/New-ChangelogEntry.ps1 -ServerName "Fabric.Mcp.Server"
./eng/scripts/Compile-Changelog.ps1 -ServerName "Fabric.Mcp.Server" -DryRun
```

**3. Smart Compilation**
- Groups entries by section and subsection
- Handles multi-line descriptions with nested lists
- Auto-creates "Unreleased" sections when needed
- Supports compiling to specific version sections
- Removes empty sections from output

**4. VS Code Extension Integration**
Automatically syncs main CHANGELOG to VS Code extension CHANGELOG with section mapping:
- "Features Added" → "Added"
- "Breaking Changes" + "Other Changes" → "Changed"
- "Bugs Fixed" → "Fixed"

### Usage Examples

**For Contributors:**
```powershell
# Create a changelog entry
./eng/scripts/New-ChangelogEntry.ps1 -Description "Added new feature" -Section "Features Added" -PR 1234

# Interactive mode
./eng/scripts/New-ChangelogEntry.ps1
```

**For Release Managers:**
```powershell
# Preview compilation
./eng/scripts/Compile-Changelog.ps1 -DryRun

# Compile and delete YAML files
./eng/scripts/Compile-Changelog.ps1 -DeleteFiles

# Sync to VS Code extension
./eng/scripts/Sync-VsCodeChangelog.ps1
```

### Technical Details

- **Filename convention**: Unix timestamp in milliseconds (e.g., `1731260400123.yml`)
  - Pre-PR friendly (can create before PR number is known)
  - Chronologically sortable
  - 1000 unique values per second (collision-resistant)

- **Version handling**: Supports both `## 2.0.0 (Unreleased)` and `## [0.0.1] - 2025-09-16` formats

- **Validation**: All entries validated against JSON schema during creation and compilation

### Benefits

- 🚀 **Zero merge conflicts** on CHANGELOG.md
- 📝 **Easier code reviews** - changelog entries visible in PR diffs
- ✅ **Automated formatting** - consistent output every time
- 🔍 **Better Git history** - clear attribution of who added each entry
- 🎯 **Structured data** - validated against schema
- 🌐 **Multi-server ready** - works across all MCP servers in the repo

### Testing

Tested with:
- ✅ Creating entries for Azure.Mcp.Server and Fabric.Mcp.Server
- ✅ Compiling to Unreleased and specific version sections
- ✅ Multi-line descriptions with nested bullet lists
- ✅ Empty section removal
- ✅ VS Code CHANGELOG syncing
- ✅ Both interactive and non-interactive modes

### Migration Notes

Existing workflow remains valid during transition:
- Old entries in CHANGELOG.md stay as-is
- New entries use YAML files going forward
- Scripts handle both scenarios gracefully

### Related Documentation

- See `docs/changelog-entries.md` for contributor guide
- See `docs/design/changelog-management-system.md` for design details
- Inspired by [GitLab's changelog system](https://about.gitlab.com/blog/solving-gitlabs-changelog-conflict-crisis/)
