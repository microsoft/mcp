# Changelog Management System

## Overview

This document describes the implementation of a conflict-free changelog management system inspired by [GitLab's approach](https://about.gitlab.com/blog/solving-gitlabs-changelog-conflict-crisis/). Instead of directly editing `CHANGELOG.md`, contributors create individual YAML files that are compiled during the release process.

**Key Benefits:**
- ✅ No merge conflicts on CHANGELOG.md
- ✅ Automated compilation and formatting
- ✅ Structured, validated data
- ✅ Flexible organization with sections and subsections
- ✅ Git history shows who added each entry

---

## Directory Structure

```
servers/Azure.Mcp.Server/
├── CHANGELOG.md                    # Main changelog
└── changelog-entries/              # Individual entry files
    ├── vcolin7-fix-changelog-system.yml
    ├── jfree-documentation-update.yml
    ├── ...
    └── alzimmermsft-improve-performance.yml
```

**Why keep it simple with a single flat directory?**
- Each release train (i.e. beta, stable) lives in a separate branch with its own `CHANGELOG.md` and `changelog-entries/` folder, so there is no need for subdirectories.
- Simpler workflow: contributors only need to create files in a single known location
- Simplified file compilation and removal together on release

---

## YAML File Format

### Filename Convention

**Format:** `{username-brief-change-description}.yml`

**Example:** `jdoe-add-user-authentication.yml`

**Why this approach?**
- **Uniqueness**: Usernames and brief descriptions make filenames unique and descriptive
- **Sortable**: Files naturally sort alphabetically by username and description
- **Simple**: Easy to understand and manage
- **Pre-PR friendly**: Can create entries before opening a PR

### YAML Schema

```yaml
pr: <number>          # Required: PR number (use 0 if not known yet)
changes:              # Required: Array of changes (minimum 1)
  - section: <string>     # Required
    description: <string> # Required
    subsection: <string>  # Optional
```

#### Required Fields

- **pr**: Pull request number at the top level (integer, use 0 if not known yet)
- **changes**: Array of `change` objects (must have at least one). Each `change` requires:
  - **section**: One of the following:
    - `Features Added` - New features, tools, or capabilities
    - `Breaking Changes` - Changes that break backward compatibility
    - `Bugs Fixed` - Bug fixes
    - `Other Changes` - Everything else (dependency updates, refactoring, etc.)
  - **description**: Description of the change

#### Optional Fields

- **subsection**: Optional subsection to group changes under. Currently, the only valid subsection is `Dependency Updates` under the `Other Changes` section.

**Note:** Not every PR needs a changelog entry! Only include entries for changes that are worth mentioning to users or maintainers (new features, breaking changes, important bug fixes, etc.). Minor internal refactoring, documentation updates, or test changes typically don't need entries.

**When to create a changelog entry:**
- ✅ New user-facing features or tools
- ✅ Breaking changes that affect users or API consumers
- ✅ Important bug fixes
- ✅ Significant performance improvements
- ✅ Dependency updates that affect functionality
- ❌ Internal refactoring with no user impact
- ❌ Documentation-only changes (unless major)
- ❌ Test-only changes
- ❌ Minor code cleanup or formatting

---

## Implementation Components

### 1. JSON Schema for Validation

**File:** `eng/schemas/changelog-entry.schema.json`

Validates YAML structure and ensures required fields are present.

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Changelog Entry",
  "description": "Schema for individual changelog entry YAML files. One file per PR, supporting multiple changes.",
  "type": "object",
  "required": ["pr", "changes"],
  "properties": {
    "pr": {
      "type": "integer",
      "description": "Pull request number (use 0 if not known yet)",
      "minimum": 0
    },
    "changes": {
      "type": "array",
      "description": "List of changes in this PR (must have at least one)",
      "minItems": 1,
      "items": {
        "type": "object",
        "required": ["section", "description"],
        "properties": {
          "section": {
            "type": "string",
            "description": "The changelog section this entry belongs to",
            "enum": [
              "Features Added",
              "Breaking Changes",
              "Bugs Fixed",
              "Other Changes"
            ]
          },
          "subsection": {
            "type": "string",
            "description": "Optional subsection for grouping related changes",
            "enum": [
              "Dependency Updates"
            ]
          },
          "description": {
            "type": "string",
            "description": "Description of the change",
            "minLength": 10
          }
        },
        "additionalProperties": false
      }
    }
  },
  "additionalProperties": false
}
```

### 2. Generator Script

**File:** `eng/scripts/New-ChangelogEntry.ps1`

Helper script to create properly formatted changelog entries. It supports both interactive and one-line modes.

**Usage:**

```powershell
# Interactive mode (prompts for all fields)
./eng/scripts/New-ChangelogEntry.ps1

# One-line with parameters
./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description "Added support for User-Assigned Managed Identity" `
  -Section "Features Added" `
  -PR 1234

# With subsection
./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description "Updated ModelContextProtocol.AspNetCore to version 0.4.0-preview.3" `
  -Section "Other Changes" `
  -Subsection "Dependency Updates" `
  -PR 1234

# With a multi-line entry
$description = @"
Added new Foundry tools:
- foundry_agents_create: Create a new AI Foundry agent
- foundry_threads_create: Create a new AI Foundry Agent Thread
- foundry_threads_list: List all AI Foundry Agent Threads
"@

./eng/scripts/New-ChangelogEntry.ps1 `
  -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
  -Description $description `
  -Section "Features Added" `
  -PR 1234
```

**Features:**
- Interactive prompts for section, description, PR number
- Auto-generates filename based on username and description
- Validates YAML against schema
- Places file in correct `changelog-entries/` directory
- Creates `changelog-entries/` directory if it doesn't exist

### 3. Compiler Script

**File:** `eng/scripts/Compile-Changelog.ps1`

Compiles all YAML entries into CHANGELOG.md.

**Usage:**

```powershell
# Preview what will be compiled (dry run)
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun

# Compile entries into CHANGELOG.md
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md"

# Compile to a specific version
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -Version "<version>"

# Compile and remove YAML files after successful compilation
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DeleteFiles
```

**Parameters:**
- `-ChangelogPath`: Required. Path to the CHANGELOG.md file (e.g., `servers/Azure.Mcp.Server/CHANGELOG.md`)
- `-Version`: Optional. Target version section to compile into. If not specified, compiles to "Unreleased" section
- `-DryRun`: Preview compilation without modifying files
- `-DeleteFiles`: Remove YAML files after successful compilation

**Features:**
- Reads all YAML files from `changelog-entries/`
- Validates each file against schema
- Groups entries by section, then by subsection
- Formats entries as markdown with PR links
- Inserts compiled entries into CHANGELOG.md
- Optional deletion of YAML files after compilation
- Error handling for missing/invalid files
- Summary output

**Compilation Logic:**

1. Read all `.yml` and `.yaml` files from `changelog-entries/`
2. Validate each file against schema
3. Group by section (preserving order: Features, Breaking, Bugs, Other)
4. Within each section, group by subsection (if present)
5. Sort entries within groups
6. Generate markdown format with PR links
7. Find the target version section in CHANGELOG.md (or "Unreleased")
8. Insert compiled entries under appropriate section headers
9. Optionally delete YAML files if `-DeleteFiles` flag is set

---

## Example Workflow

### For Contributors

When making a change that needs a changelog entry:

1. **Make your code changes**

2. **Create a changelog entry:**
   ```powershell
   ./eng/scripts/New-ChangelogEntry.ps1 `
     -ChangelogPath "servers/<server-name>/CHANGELOG.md" `
     -Description "Your change description" `
     -Section "Features Added" `
     -PR <pr-number>
   ```

3. **Commit both your code AND the YAML file**

4. **Open your PR** - no conflicts on CHANGELOG.md!

### For Release Managers

Before tagging a release:

1. **Preview compilation:**
   ```powershell
   # Preview to Unreleased section
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun
   
   # Or preview to a specific version
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -Version "<version>" -DryRun
   ```

2. **Compile and clean up:**
   ```powershell
   ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DeleteFiles
   ```

3. **Sync VS Code extension CHANGELOG (if applicable):**
   ```powershell
   ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md"
   ```

4. **Update version in CHANGELOG.md:**
   - Change "Unreleased" to actual version number
   - Add release date

5. **Commit the compiled changelog**

6. **Tag the release**

---

## Example Output

### Input YAML Files

**File:** `vcolin7-managed-identity.yaml`
```yaml
pr: 1033
changes:
  - section: "Features Added"
    description: "Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable."
```

**File:** `jdoe-speech-recognition.yaml`
```yaml
pr: 1054
changes:
  - section: "Features Added"
    description: "Added support for speech recognition from an audio file with Fast Transcription via the command `azmcp_speech_stt_recognize`."
```

**File:** `alzimmermsft-dependency-update.yaml`
```yaml
pr: 887
changes:
  - section: "Other Changes"
    subsection: "Dependency Updates"
    description: "Updated the `ModelContextProtocol.AspNetCore` package from version `0.4.0-preview.2` to `0.4.0-preview.3`."
```

**File:** `jfree-multiple-changes.yaml` (multiple entries in one file)
```yaml
pr: 1234
changes:
  - section: "Features Added"
    description: "Added support for multiple changes per PR in changelog entries"
  - section: "Bugs Fixed"
    description: "Fixed issue with subsection title casing"
```

### Compiled Output in CHANGELOG.md

When compiled, entries are grouped by section and subsection. Empty sections will not be included.

```markdown
## 2.0.0-beta.3 (Unreleased)

### Features Added

- Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable. [[#1033](https://github.com/microsoft/mcp/pull/1033)]
- Added support for speech recognition from an audio file with Fast Transcription via the command `azmcp_speech_stt_recognize`. [[#1054](https://github.com/microsoft/mcp/pull/1054)]
- Added support for multiple changes per PR in changelog entries [[#1234](https://github.com/microsoft/mcp/pull/1234)]

### Bugs Fixed

- Fixed issue with subsection title casing [[#1234](https://github.com/microsoft/mcp/pull/1234)]

### Other Changes

#### Dependency Updates

- Updated the `ModelContextProtocol.AspNetCore` package from version `0.4.0-preview.2` to `0.4.0-preview.3`. [[#887](https://github.com/microsoft/mcp/pull/887)]
```

**Note:** When dealing with multi-line descriptions the PR link will be added to the last line. If the first line is followed by lines that are bullet items, they'll be automatically indented as sub-bullets and the PR link will be added to the first line instead.

---

## Validation & Quality Controls

### Schema Validation

All YAML files are validated against the JSON schema before compilation. Invalid files cause compilation to fail with clear error messages.

### Filename Validation

The compiler checks that filenames follow the expected pattern:
- Must end with `.yml` or `.yaml`
- Should follow `{username}-{brief-description}` format
- Ignores `README.md` and other non-YAML files

### CI Integration (Recommended)

Add validation to your CI pipeline:

```yaml
# Example GitHub Actions check
- name: Validate Changelog Entries
  run: |
    # Test compilation (dry run) - includes entry validation
    ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -DryRun
```

### Pre-commit Hooks (Optional)

- Validate YAML schema for changed `.yml` or `.yaml` files in `changelog-entries/`
- Ensure filename follows `{username}-{brief-description}` convention
- Warn if YAML file has `pr: 0` (can be updated later)

---

## Implementation Checklist

- [x] Create `changelog-entries/` directory
- [x] Create JSON schema: `eng/schemas/changelog-entry.schema.json`
- [x] Create generator script: `eng/scripts/New-ChangelogEntry.ps1`
- [x] Create compiler script: `eng/scripts/Compile-Changelog.ps1`
- [x] Create documentation: `docs/changelog-entries`
- [x] Update `CONTRIBUTING.md` with new changelog workflow
- [x] Update `docs/new-command.md` to mention changelog entries for AI coding agents
- [x] Update `AGENTS.md` to include changelog workflow for AI agents
- [ ] Add CI validation for YAML files (optional but recommended)
- [x] Create sample YAML files for testing
- [x] Test compilation with sample data
- [ ] Document in team wiki/onboarding materials
- [ ] Migrate any existing unreleased entries to YAML format

---

## Migration Strategy

For existing unreleased entries in CHANGELOG.md:

1. **Create YAML files** for each existing entry in the "Unreleased" section
2. **Run compilation** to verify output matches current format
3. **Remove unreleased entries** from CHANGELOG.md (will be re-added by compilation)
4. **Commit YAML files** to the repository
5. **Going forward**, all new entries use YAML files only

---

## Alternative Filename Strategies Considered

| Strategy | Pros | Cons | Verdict |
|----------|------|------|---------|
| **Username + description** (chosen) | Unique, descriptive, simple, pre-PR friendly | None significant | ✅ Best choice |
| Timestamp milliseconds | Unique, sortable | Opaque, not descriptive | ❌ Less readable |
| Timestamp + PR number | Guaranteed unique, traceable | Verbose, requires PR first | ❌ Unnecessary complexity |
| Git commit SHA | Unique, Git-native | Harder to read/sort, requires commit | ❌ Less convenient |
| GUID only | Guaranteed unique | Completely opaque, no sorting | ❌ Too opaque |
| Sequential counter | Simple, short | Requires coordination/locking | ❌ Conflict-prone |
| PR number only | Simple, short | Not unique, requires PR first | ❌ Defeats purpose |

---

## FAQ

### Q: What if I forget to add a changelog entry?

Add it later in a follow-up PR. Each entry is a separate file, so there's no conflict.

### Q: Can I edit an existing changelog entry?

Yes! Just edit the YAML file and commit the change. It will be picked up in the next compilation.

### Q: What if two entries use the same filename?

The filename is the unique identifier for each changelog entry. If two entries use the same filename, Git will show a conflict, and you can simply update the filename for your new change.

### Q: Do I need to compile the changelog in my PR?

No! Contributors only create YAML files. Release managers compile during the release process.

### Q: Can I add multiple changelog entries in one PR?

Yes! Just create a single YAML file with multiple entries under the `changes` section. This is common when a PR includes several distinct user-facing changes.

### Q: Do I need to know the PR number when creating an entry?

No! You can create the YAML file before opening a PR. Just add or update the `pr` field later when you know the PR number.

### Q: Does every PR need a changelog entry?

No. Only include entries for changes worth mentioning to users (new features, breaking changes, important bug fixes). Skip entries for internal refactoring, minor documentation updates, test-only changes, or code cleanup.

### Q: What happens to the YAML files after release?

They're deleted by the release manager using `Compile-Changelog.ps1 -DeleteFiles` after compilation.

---

## References

- [GitLab's Original Blog Post](https://about.gitlab.com/blog/solving-gitlabs-changelog-conflict-crisis/)
- [Azure SDK Changelog Guidelines](https://aka.ms/azsdk/guideline/changelogs)
- [Keep a Changelog](https://keepachangelog.com/)

---

## AI Agent Integration

When using AI coding agents like GitHub Copilot to work on features, ensure they are aware of the changelog workflow:

### Key Documents for AI Agents

1. **`docs/new-command.md`** - Should include step about creating changelog entry
2. **`AGENTS.md`** - Should reference this changelog system
3. **`CONTRIBUTING.md`** - Should explain when and how to create entries

### Recommended Addition to Agent Instructions

Add to relevant agent instruction files:

```markdown
## Changelog Entries

When adding user-facing changes (new features, breaking changes, important bug fixes):

1. Create a changelog entry YAML file:
   ```bash
   ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/<server-name>/CHANGELOG.md" -Description "Your change" -Section "Features Added" -PR <number>
   ```

2. Or manually create `changelog-entries/{username}-{brief-description}.yaml`:
   ```yaml
   pr: 1234  # Can use 0 if not known yet
   changes:
     - section: "Features Added"
       description: "Your change description"
   ```

3. Not every change needs a changelog entry - skip for internal refactoring, test-only changes, or minor updates.
```

---

## Future Enhancements

Potential improvements to consider:

- [ ] Automatic PR number detection from git branch or PR context
- [ ] GitHub Action to auto-create YAML file from PR template
- [ ] Validation bot that comments on PRs when user-facing changes lack changelog entries
- [ ] Support for multiple changelog files (e.g., separate for different components)
- [ ] Integration with release notes generation
- [ ] Automatic backporting of changelog entries to stable branches
- [ ] AI agent templates that auto-generate changelog entries based on code changes
