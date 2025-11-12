# Changelog Management System

## Overview

This document describes the implementation of a conflict-free changelog management system inspired by [GitLab's approach](https://about.gitlab.com/blog/solving-gitlabs-changelog-conflict-crisis/). Instead of directly editing `CHANGELOG.md`, contributors create individual YAML files that are compiled during the release process.

**Key Benefits:**
- ✅ No merge conflicts on CHANGELOG.md
- ✅ Easier code reviews (changelog entry visible in PR)
- ✅ Automated compilation and formatting
- ✅ Structured, validated data
- ✅ Flexible organization with sections and subsections
- ✅ Git history shows who added each entry

---

## Directory Structure

```
servers/Azure.Mcp.Server/
├── CHANGELOG.md                    # Main changelog (compiled output)
└── changelog-entries/              # Individual entry files
    ├── 1731260400123.yml
    ├── 1731260405789.yml
    ├── ...
    └── README.md                   # Documentation for contributors
```

**Why keep it simple with a single flat directory?**
- We only release from one branch at a time, so we never need multiple folders
- Simpler workflow: contributors only need to know one location
- All files get compiled and removed together on release
- No unnecessary nesting

---

## YAML File Format

### Filename Convention

**Format:** `{unix-timestamp-milliseconds}.yml`

**Example:** `1731260400123.yml`

**Why this approach?**
- **Uniqueness**: Millisecond precision makes collisions extremely unlikely (1000 unique values per second)
- **Sortable**: Files naturally sort chronologically
- **Simple**: Just a number, no need to coordinate PR numbers
- **Pre-PR friendly**: Can create entries before opening a PR
- **Cross-platform safe**: No special characters

**How to generate:**
```powershell
# In PowerShell
[DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()

# In Bash
date +%s%3N
```

### YAML Schema

```yaml
# Required fields
section: "Features Added"  # One of: Features Added, Breaking Changes, Bugs Fixed, Other Changes
description: "Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable."
pr: 1033  # PR number (integer) - can be added later if not known yet

# Optional fields
subsection: null  # Optional: "Dependency Updates", "Telemetry", etc.
```

**Note:** Not every PR needs a changelog entry! Only include entries for changes that are worth mentioning to users or maintainers (new features, breaking changes, important bug fixes, etc.). Minor internal refactoring, documentation updates, or test changes typically don't need entries.

### Valid Values

**Sections (required):**
- `Features Added`
- `Breaking Changes`
- `Bugs Fixed`
- `Other Changes`

**Subsections (optional):**
- `Dependency Updates`
- `Telemetry`
- Any custom subsection for grouping related changes

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
  "type": "object",
  "required": ["section", "description", "pr"],
  "properties": {
    "section": {
      "type": "string",
      "enum": [
        "Features Added",
        "Breaking Changes",
        "Bugs Fixed",
        "Other Changes"
      ]
    },
    "subsection": {
      "type": ["string", "null"]
    },
    "description": {
      "type": "string",
      "minLength": 10
    },
    "pr": {
      "type": "integer",
      "minimum": 1
    }
  },
  "additionalProperties": false
}
```

### 2. Generator Script

**File:** `eng/scripts/New-ChangelogEntry.ps1`

Helper script to create properly formatted changelog entries.

**Usage:**

```powershell
# Interactive mode (prompts for all fields)
./eng/scripts/New-ChangelogEntry.ps1

# With parameters
./eng/scripts/New-ChangelogEntry.ps1 `
  -Description "Added new feature" `
  -Section "Features Added" `
  -PR 1234

# With subsection
./eng/scripts/New-ChangelogEntry.ps1 `
  -Description "Updated Azure.Core to 1.2.3" `
  -Section "Other Changes" `
  -Subsection "Dependency Updates" `
  -PR 1234
```

**Features:**
- Interactive prompts for section, description, PR number
- Auto-generates filename with timestamp
- Validates YAML against schema
- Places file in correct directory
- Creates `changelog-entries/` directory if it doesn't exist

### 3. Compiler Script

**File:** `eng/scripts/Compile-Changelog.ps1`

Compiles all YAML entries into CHANGELOG.md.

**Usage:**

```powershell
# Preview what will be compiled (dry run)
./eng/scripts/Compile-Changelog.ps1 -DryRun

# Compile entries into CHANGELOG.md
./eng/scripts/Compile-Changelog.ps1

# Compile and remove YAML files after successful compilation
./eng/scripts/Compile-Changelog.ps1 -DeleteFiles

# Custom changelog path
./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "path/to/CHANGELOG.md"
```

**Features:**
- Reads all YAML files from `changelog-entries/`
- Validates each file against schema
- Groups entries by section, then by subsection
- Formats entries as markdown with PR links
- Inserts compiled entries into CHANGELOG.md under "Unreleased"
- Optional deletion of YAML files after compilation
- Error handling for missing/invalid files
- Summary output

**Compilation Logic:**

1. Read all `.yml` files from `changelog-entries/` (excluding README.md)
2. Validate each file against schema
3. Group by section (preserving order: Features, Breaking, Bugs, Other)
4. Within each section, group by subsection (if present)
5. Sort entries within groups
6. Generate markdown format with PR links
7. Find the "Unreleased" section in CHANGELOG.md
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
   ./eng/scripts/Compile-Changelog.ps1 -DryRun
   ```

2. **Compile and clean up:**
   ```powershell
   ./eng/scripts/Compile-Changelog.ps1 -DeleteFiles
   ```

3. **Update version in CHANGELOG.md:**
   - Change "Unreleased" to actual version number
   - Add release date

4. **Commit the compiled changelog**

5. **Tag the release**

---

## Example Output

### Input YAML Files

**File:** `1731260400123.yml`
```yaml
section: "Features Added"
description: "Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable."
pr: 1033
```

**File:** `1731260405789.yml`
```yaml
section: "Features Added"
description: "Added support for speech recognition from an audio file with Fast Transcription via the command `azmcp_speech_stt_recognize`."
pr: 1054
```

**File:** `1731260410456.yml`
```yaml
section: "Other Changes"
subsection: "Telemetry"
description: "Added `ToolId` into telemetry, based on `IBaseCommand.Id`, a unique GUID for each command."
pr: 1028
```

### Compiled Output in CHANGELOG.md

```markdown
## 2.0.0-beta.3 (Unreleased)

### Features Added

- Added support for User-Assigned Managed Identity via the `AZURE_CLIENT_ID` environment variable. [[#1033](https://github.com/microsoft/mcp/pull/1033)]
- Added support for speech recognition from an audio file with Fast Transcription via the command `azmcp_speech_stt_recognize`. [[#1054](https://github.com/microsoft/mcp/pull/1054)]

### Breaking Changes

### Bugs Fixed

### Other Changes

#### Telemetry
- Added `ToolId` into telemetry, based on `IBaseCommand.Id`, a unique GUID for each command. [[#1028](https://github.com/microsoft/mcp/pull/1028)]
```

---

## Validation & Quality Controls

### Schema Validation

All YAML files are validated against the JSON schema before compilation. Invalid files cause compilation to fail with clear error messages.

### Filename Validation

The compiler checks that filenames follow the expected pattern:
- Must end with `.yml`
- Should be numeric (timestamp format) - warning if not
- Ignores `README.md` and other non-YAML files

### CI Integration (Recommended)

Add validation to your CI pipeline:

```yaml
# Example GitHub Actions check
- name: Validate Changelog Entries
  run: |
    # Validate YAML syntax and schema
    ./eng/scripts/Validate-ChangelogEntries.ps1
    
    # Test compilation (dry run)
    ./eng/scripts/Compile-Changelog.ps1 -DryRun
```

### Pre-commit Hooks (Optional)

- Validate YAML schema for changed `.yml` files in `changelog-entries/`
- Ensure filename follows numeric timestamp convention
- Warn if YAML file is missing a `pr` field (can be added later)

---

## Implementation Checklist

- [ ] Create `changelog-entries/` directory
- [ ] Create JSON schema: `eng/schemas/changelog-entry.schema.json`
- [ ] Create generator script: `eng/scripts/New-ChangelogEntry.ps1`
- [ ] Create compiler script: `eng/scripts/Compile-Changelog.ps1`
- [ ] Create documentation: `changelog-entries/README.md`
- [ ] Update `CONTRIBUTING.md` with new changelog workflow
- [ ] Update `docs/new-command.md` to mention changelog entries for AI coding agents
- [ ] Update `AGENTS.md` to include changelog workflow for AI agents
- [ ] Add CI validation for YAML files (optional but recommended)
- [ ] Create sample YAML files for testing
- [ ] Test compilation with sample data
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
| **Timestamp milliseconds** (chosen) | Unique, sortable, simple, pre-PR friendly | None significant | ✅ Best choice |
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

### Q: What if two entries use the same timestamp?

The timestamp is in milliseconds, giving 1000 unique values per second. Collisions are extremely unlikely. If one does occur, Git will highlight a file is being modified instead of added, and you can simply regenerate the timestamp.

### Q: Do I need to compile the changelog in my PR?

No! Contributors only create YAML files. Release managers compile during the release process.

### Q: Can I add multiple changelog entries in one PR?

Yes, just create multiple YAML files with different filenames (different timestamps).

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
   ./eng/scripts/New-ChangelogEntry.ps1 -Description "Your change" -Section "Features Added" -PR <number>
   ```

2. Or manually create `changelog-entries/{timestamp}.yml`:
   ```yaml
   section: "Features Added"
   description: "Your change description"
   pr: 1234  # Can be added later if not known yet
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
