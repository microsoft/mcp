# Changelog Entries

## Overview

Each server's `/changelog-entries` directory contains individual YAML entry files that are compiled into the main `CHANGELOG.md` during the release process. This document describes how to create, manage, and compile said entries files.

**Why individual files?**

Using separate YAML files for each changelog entry **eliminates merge conflicts** on `CHANGELOG.md` when multiple contributors work on the same branch simultaneously, allowing for smoother collaboration on a repository with numerous active contributors.

## Quick Start

For most contributors, here's all you need:

```powershell
# Create a changelog entry for your PR
./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" `
  -Description "Your change description here" `
  -Section "Features Added"
```

That's it! The PR number will be **auto-detected** from the git commit when the changelog is compiled during release. Commit the YAML file with your code changes.

> **Tip:** Not every PR needs a changelog entry. Skip it for internal refactoring, test-only changes, or minor cleanup.

## Table of Contents

- [Changelog Entries](#changelog-entries)
  - [Overview](#overview)
  - [Quick Start](#quick-start)
  - [Table of Contents](#table-of-contents)
  - [Creating a Changelog Entry](#creating-a-changelog-entry)
    - [When to Create an Entry](#when-to-create-an-entry)
    - [File Format](#file-format)
      - [Required Fields](#required-fields)
      - [Optional Fields](#optional-fields)
    - [Using the Generator Script](#using-the-generator-script)
      - [Interactive mode](#interactive-mode)
      - [One-line](#one-line)
        - [With a subsection](#with-a-subsection)
        - [With a multi-line entry](#with-a-multi-line-entry)
    - [Manual Creation](#manual-creation)
      - [Simple entry](#simple-entry)
      - [Multi-line entry](#multi-line-entry)
      - [Using a subsection](#using-a-subsection)
      - [Multiple-entries](#multiple-entries)
  - [Preparing a New Release](#preparing-a-new-release)
    - [Compiled Output](#compiled-output)
    - [Validation](#validation)
  - [Tips](#tips)
  - [FAQ](#faq)
    - [Do I need to compile the changelog in my PR?](#do-i-need-to-compile-the-changelog-in-my-pr)
    - [Can I edit an existing changelog entry?](#can-i-edit-an-existing-changelog-entry)
    - [What if I forget to add a changelog entry?](#what-if-i-forget-to-add-a-changelog-entry)
    - [What if two entries use the same filename?](#what-if-two-entries-use-the-same-filename)
    - [Can I add multiple changelog entries in one PR?](#can-i-add-multiple-changelog-entries-in-one-pr)
    - [Do I need to know the PR number when creating an entry?](#do-i-need-to-know-the-pr-number-when-creating-an-entry)
    - [Does every PR need a changelog entry?](#does-every-pr-need-a-changelog-entry)
    - [What happens to the YAML files after release?](#what-happens-to-the-yaml-files-after-release)
    - [What if I have any other questions?](#what-if-i-have-any-other-questions)

## Creating a Changelog Entry

### When to Create an Entry

Create a changelog entry for:
- ✅ New user-facing features or tools
- ✅ Breaking changes that affect users or API consumers
- ✅ Important bug fixes
- ✅ Significant performance improvements
- ✅ Dependency updates that affect functionality

**Skip** changelog entries for:
- ❌ Internal refactoring with no user impact
- ❌ Documentation-only changes (unless major)
- ❌ Test-only changes
- ❌ Minor code cleanup or formatting

### File Format

**Structure**: One file per PR, supporting multiple changelog entries

```yaml
pr: <number>              # Optional: PR number (auto-detected from git commit during compilation if omitted)
changes:                  # Required: Array of changes (minimum 1)
  - section: <string>     # Required
    description: <string> # Required
    subsection: <string>  # Optional
```

#### Required Fields

- **changes**: Array of `change` objects (must have at least one). Each `change` requires:
  - **section**: One of the following:
    - `Features Added` - New features, tools, or capabilities
    - `Breaking Changes` - Changes that break backward compatibility
    - `Bugs Fixed` - Bug fixes
    - `Other Changes` - Everything else (dependency updates, refactoring, etc.)
  - **description**: Description of the change

#### Optional Fields

- **pr**: Pull request number (positive integer). If not provided, it can be auto-detected from the git commit message during compilation. GitHub squash merges include the PR number in the format `... (#1234)` which the compiler extracts automatically.
- **subsection**: Optional subsection to group changes under. Currently, the only valid subsection is `Dependency Updates` under the `Other Changes` section.

### Using the Generator Script

The easiest way to create a changelog entry is using the generator script located at `./eng/scripts/New-ChangelogEntry.ps1`. It supports both interactive and one-line modes:

#### Interactive mode

```powershell
./eng/scripts/New-ChangelogEntry.ps1
```

#### One-line

```powershell
./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description "Added support for User-Assigned Managed Identity" `
  -Section "Features Added"
```

> **Note:** The `-PR` parameter is optional. If omitted, the PR number will be auto-detected from the git commit during compilation.

##### With a subsection

```powershell
./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description "Updated ModelContextProtocol.AspNetCore to version 0.4.0-preview.3" `
  -Section "Other Changes" `
  -Subsection "Dependency Updates"
```

##### With a multi-line entry

```powershell
$description = @"
Added new Foundry tools:
- foundry_agents_create: Create a new AI Foundry agent
- foundry_threads_create: Create a new AI Foundry Agent Thread
- foundry_threads_list: List all AI Foundry Agent Threads
"@

./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description $description `
  -Section "Features Added"
```

### Manual Creation

If you prefer to do things manually, create a new YAML file with your changes and save it in the appropriate `servers/<server-name>/changelog-entries` directory. **Use a unique name** made up of your alias or GitHub username and a brief description of the change, like this: `alias-brief-description.yaml`. For example: `servers/Azure.Mcp.Server/changelog-entries/vcolin7-fix-serialization.yaml`.

#### Simple entry

```yaml
changes:
  - section: "Features Added"
    description: "Added support for User-Assigned Managed Identity"
```

> **Note:** You can also specify the PR number explicitly if you know it: `pr: 1234`

#### Multi-line entry

```yaml
changes:
  - section: "Features Added"
    description: |
      Added new AI Foundry tools:
      - foundry_agents_create: Create a new AI Foundry agent
      - foundry_threads_create: Create a new AI Foundry Agent Thread
      - foundry_threads_list: List all AI Foundry Agent Threads
```

#### Using a subsection

```yaml
changes:
  - section: "Other Changes"
    subsection: "Dependency Updates"
    description: "Updated ModelContextProtocol.AspNetCore to version 0.4.0-preview.3"
```

#### Multiple-entries

```yaml
changes:
  - section: "Features Added"
    description: "Added support for multiple changes per PR in changelog entries"
  - section: "Bugs Fixed"
    description: "Fixed issue with subsection title casing"
  - section: "Other Changes"
    subsection: "Dependency Updates"
    description: "Updated ModelContextProtocol.AspNetCore to version 0.4.0-preview.3"
```

## Preparing a New Release

If you are a release manager, follow these steps before initiaiting a new release pipeline run:

1. Preview the changelog section for the version you are about to release:*
   ```powershell
   # Compile to the default Unreleased section for Azure MCP Server
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun
   
   # Or compile to a specific version
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -Version "<version>" -DryRun
   ```

2. Compile entries and delete files:
   ```powershell
   # Compile to Unreleased section and delete YAML files for Azure MCP Server
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DeleteFiles

   # Or compile to a specific version
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -Version "<version>" -DeleteFiles
   ```

   Notes:
   - `-ChangelogPath`: Required parameter specifying which changelog file to compile changes for
   - If `-Version` is specified: Entries are compiled into that version section (must exist in `CHANGELOG.md`)
   - If no `-Version` is specified: Entries are compiled into the "Unreleased" section at the top
   - If no "Unreleased" section exists and no `-Version` is specified: A new "Unreleased" section is created with the next version number

3. Sync the VS Code extension CHANGELOG (if applicable):
   ```powershell
   # Preview the sync
   ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun
   
   # Apply the sync
   ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md"
   ```

4. Update release date in CHANGELOG.md
5. Commit and initiate the release process

### Compiled Output

When compiled, entries are grouped by section and subsection. Empty sections will not be included.

```markdown
## 2.0.0-beta.3 (Unreleased)

### Features Added

- Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable. [[#1033](https://github.com/microsoft/mcp/pull/1033)]
- Added speech recognition support. [[#1054](https://github.com/microsoft/mcp/pull/1054)]

### Other Changes

#### Dependency Updates

- Updated the `ModelContextProtocol.AspNetCore` package from version `0.4.0-preview.2` to `0.4.0-preview.3`. [[#887](https://github.com/Azure/azure-mcp/pull/887)]
```

**Note:** When dealing with multi-line descriptions the PR link will be added to the last line. If the first line is followed by lines that are bullet items, they'll be automatically indented as sub-bullets and the PR link will be added to the first line instead.

### Validation

The scripts automatically validate YAML files against the schema at `eng/schemas/changelog-entry.schema.json`. To manually validate, you can use the `-DryRun` flag as follows:

```powershell
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun
```

## Tips

- **Multiple servers**: All scripts require the `-ChangelogPath` parameter
  - Available servers: `Azure.Mcp.Server`, `Fabric.Mcp.Server`, `Template.Mcp.Server`, etc.
  - Each server has its own `changelog-entries/` folder and `CHANGELOG.md`
  - Example paths: `servers/Azure.Mcp.Server/CHANGELOG.md`, `servers/Fabric.Mcp.Server/CHANGELOG.md`
- **PR number auto-detection**: You don't need to know the PR number when creating an entry, it will be auto-detected from the git commit during compilation. You can still provide it explicitly if you want.
- **Edit an existing entry**: Just edit the YAML file and commit the change.
- **Multiple entries**: Create a single YAML file with multiple entries under the `changes` section.

## FAQ

### Do I need to compile the changelog in my PR?

No! Contributors only create YAML files. Release managers compile the changelog during the release process using the compilation script.

### Can I edit an existing changelog entry?

Yes! Just edit the YAML file and commit the change. It will be picked up in the next compilation.

### What if I forget to add a changelog entry?

Add it later in a follow-up PR or ask the maintainer to create one. Each entry is a separate file, so there's no conflict with other ongoing work.

### What if two entries use the same filename?

The filename is the unique identifier for each changelog entry. If two entries use the same filename, Git will show a conflict, and you can simply update the filename for your new change.

### Can I add multiple changelog entries in one PR?

Yes! Just create a single YAML file with multiple entries under the `changes` section. This is common when a PR includes several distinct user-facing changes.

### Do I need to know the PR number when creating an entry?

No! The PR number is **auto-detected** from the git commit message during compilation. When your PR is merged via GitHub's squash merge, the commit message includes the PR number in the format `... (#1234)`. The compilation script extracts this automatically.

If you prefer, you can still specify the PR number explicitly using `pr: 1234` in the YAML file.

### Does every PR need a changelog entry?

No! Only include entries for changes worth mentioning to users:
- ✅ New features, breaking changes, important bug fixes
- ❌ Internal refactoring, test-only changes, minor cleanup

### What happens to the YAML files after release?

They're deleted by the release manager using the `-DeleteFiles` flag when compiling entries into the main `CHANGELOG.md`.

### What if I have any other questions?

Reach out to the maintainers if you have questions or encounter issues with the changelog entry system.
