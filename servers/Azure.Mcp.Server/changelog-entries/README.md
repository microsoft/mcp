# Changelog Entries

This directory contains individual changelog entry files that are compiled into the main `CHANGELOG.md` during the release process.

## Why Individual Files?

Using separate YAML files for each changelog entry **eliminates merge conflicts** on `CHANGELOG.md` when multiple contributors work on the same branch simultaneously.

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

### Using the Generator Script (Recommended)

The easiest way to create a changelog entry is using the generator script:

```powershell
# Interactive mode (prompts for all fields)
# Defaults to Azure.Mcp.Server
./eng/scripts/New-ChangelogEntry.ps1

# For a different server
./eng/scripts/New-ChangelogEntry.ps1 -ServerName "Fabric.Mcp.Server"

# With parameters (defaults to Azure.Mcp.Server)
./eng/scripts/New-ChangelogEntry.ps1 `
  -Description "Added support for User-Assigned Managed Identity" `
  -Section "Features Added" `
  -PR 1033

# For a specific server
./eng/scripts/New-ChangelogEntry.ps1 `
  -ServerName "Fabric.Mcp.Server" `
  -Description "Added new Fabric workspace tools" `
  -Section "Features Added" `
  -PR 1234

# With subsection (automatically title-cased)
./eng/scripts/New-ChangelogEntry.ps1 `
  -Description "Updated Azure.Core to 1.2.3" `
  -Section "Other Changes" `
  -Subsection "dependency updates" `
  -PR 1234

# Multi-line description with a list
$description = @"
Added new AI Foundry tools:
- foundry_agents_create: Create a new AI Foundry agent
- foundry_threads_create: Create a new AI Foundry Agent Thread
- foundry_threads_list: List all AI Foundry Agent Threads
"@

./eng/scripts/New-ChangelogEntry.ps1 `
  -Description $description `
  -Section "Features Added" `
  -PR 945
```

**Features:**
- Works with any server (Azure.Mcp.Server, Fabric.Mcp.Server, etc.) via `-ServerName` parameter
- Automatic title-casing of subsections
- Whitespace trimming from descriptions
- Support for multi-line descriptions with lists
- Interactive validation

### Manual Creation

If you prefer to create the file manually:

1. **Generate a unique filename** using the current timestamp in milliseconds:
   ```powershell
   # PowerShell
   [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
   
   # Bash
   date +%s%3N
   ```
   Example: `1731260400123.yml`

2. **Create a YAML file** with this structure:
   ```yaml
   section: "Features Added"
   description: "Your change description here"
   subsection: null  # Or a subsection name like "Dependency Updates"
   pr: 1234  # Can be added later if not known yet
   ```

3. **Save the file** in the appropriate server's changelog-entries directory:
   - Azure MCP Server: `servers/Azure.Mcp.Server/changelog-entries/`
   - Fabric MCP Server: `servers/Fabric.Mcp.Server/changelog-entries/`
   - Other servers: `servers/{ServerName}/changelog-entries/`

## YAML File Format

### Required Fields

- **section**: The changelog section (one of: `Features Added`, `Breaking Changes`, `Bugs Fixed`, `Other Changes`)
- **description**: Description of the change (minimum 10 characters)
- **pr**: Pull request number (integer)

### Optional Fields

- **subsection**: Optional subsection for grouping (e.g., `Dependency Updates`, `Telemetry`)

### Valid Sections

- `Features Added` - New features, tools, or capabilities
- `Breaking Changes` - Changes that break backward compatibility
- `Bugs Fixed` - Bug fixes
- `Other Changes` - Everything else (dependency updates, refactoring, etc.)

### Example Entries

**Simple entry:**

**Filename:** `1731260400123.yml`

```yaml
section: "Features Added"
description: "Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable."
subsection: null
pr: 1033
```

**Multi-line entry with a list:**

**Filename:** `1731260401234.yml`

```yaml
section: "Features Added"
description: |
  Added new AI Foundry tools:
  - foundry_agents_create: Create a new AI Foundry agent
  - foundry_threads_create: Create a new AI Foundry Agent Thread
  - foundry_threads_list: List all AI Foundry Agent Threads
subsection: null
pr: 945
```

**Note:** When using multi-line descriptions with the `|` block scalar:
- The first line becomes the main bullet point
- If the following lines are bullet items (`- item`), they'll be automatically indented as sub-bullets
- The PR link is added to the last line
- Trailing newlines are automatically handled

## Workflow

### For Contributors

1. **Make your code changes**
2. **Create a changelog entry** (if your change needs one)
3. **Commit both your code and the YAML file** in the same PR
4. **Open your PR** - no CHANGELOG.md conflicts!

You can create the YAML file before you have a PR number. Just set `pr: 0` initially and update it later when you know the PR number.

### For Release Managers

Before tagging a release:

1. **Preview compilation:**
   ```powershell
   # Compile to the default Unreleased section (defaults to Azure.Mcp.Server)
   ./eng/scripts/Compile-Changelog.ps1 -DryRun
   
   # For a different server
   ./eng/scripts/Compile-Changelog.ps1 -ServerName "Fabric.Mcp.Server" -DryRun
   
   # Or compile to a specific version
   ./eng/scripts/Compile-Changelog.ps1 -Version "2.0.0-beta.3" -DryRun
   ```

2. **Compile entries:**
   ```powershell
   # Compile to Unreleased section and delete YAML files (defaults to Azure.Mcp.Server)
   ./eng/scripts/Compile-Changelog.ps1 -DeleteFiles
   
   # For a specific server
   ./eng/scripts/Compile-Changelog.ps1 -ServerName "Fabric.Mcp.Server" -DeleteFiles
   
   # Or compile to a specific version
   ./eng/scripts/Compile-Changelog.ps1 -Version "2.0.0-beta.3" -DeleteFiles
   ```

3. **Server and version behavior:**
   - `-ServerName`: Defaults to `Azure.Mcp.Server`, can be any server (e.g., `Fabric.Mcp.Server`)
   - If `-Version` is specified: Entries are compiled into that version section (must exist in CHANGELOG.md)
   - If no `-Version` is specified: Entries are compiled into the "Unreleased" section at the top
   - If no "Unreleased" section exists and no `-Version` is specified: A new "Unreleased" section is created with the next version number

4. **Sync the VS Code extension CHANGELOG** (if applicable):
   ```powershell
   # Preview the sync (Azure.Mcp.Server)
   ./eng/scripts/Sync-VsCodeChangelog.ps1 -DryRun
   
   # For a different server
   ./eng/scripts/Sync-VsCodeChangelog.ps1 -ServerName "Fabric.Mcp.Server" -DryRun
   
   # Apply the sync
   ./eng/scripts/Sync-VsCodeChangelog.ps1
   ```

5. **Update CHANGELOG.md** (if compiling to Unreleased):
   - Change "Unreleased" to the actual version number
   - Add release date

6. **Commit and tag the release**

## Compiled Output

When compiled, entries are grouped by section and subsection:

```markdown
## 2.0.0-beta.3 (Unreleased)

### Features Added

- Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable. [[#1033](https://github.com/microsoft/mcp/pull/1033)]
- Added speech recognition support. [[#1054](https://github.com/microsoft/mcp/pull/1054)]

### Breaking Changes

### Bugs Fixed

### Other Changes

#### Telemetry
- Added `ToolId` into telemetry. [[#1028](https://github.com/microsoft/mcp/pull/1028)]
```

## Validation

The scripts automatically validate your YAML files against the schema at `eng/schemas/changelog-entry.schema.json`.

To manually validate:
```powershell
# The New-ChangelogEntry.ps1 script validates automatically (defaults to Azure.Mcp.Server)
./eng/scripts/New-ChangelogEntry.ps1

# For a specific server
./eng/scripts/New-ChangelogEntry.ps1 -ServerName "Fabric.Mcp.Server"

# The Compile-Changelog.ps1 script also validates
./eng/scripts/Compile-Changelog.ps1 -DryRun

# For a specific server
./eng/scripts/Compile-Changelog.ps1 -ServerName "Fabric.Mcp.Server" -DryRun
```

## Tips

- **Multiple servers**: All scripts support the `-ServerName` parameter (defaults to `Azure.Mcp.Server`)
  - Available servers: `Azure.Mcp.Server`, `Fabric.Mcp.Server`, `Template.Mcp.Server`, etc.
  - Each server has its own `changelog-entries/` folder and `CHANGELOG.md`
- **Filename collisions**: The timestamp is in milliseconds, giving 1000 unique values per second. Collisions are extremely unlikely.
- **PR number unknown**: You can create the entry before opening a PR. Just use `pr: 0` and update it later.
- **Edit existing entry**: Just edit the YAML file and commit the change.
- **Multiple entries**: Create multiple YAML files with different timestamps.
- **Subsections**: Use sparingly for grouping related changes (e.g., dependency updates).

## Questions?

See the full documentation at `docs/changelog-management-system.md` or reach out to the maintainers.
