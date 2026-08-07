# Spelling Check Scripts

This directory contains a script to run cspell (Code Spell Checker) on the repository using the dependencies defined in the adjacent `package*.json` files.

## Adding Legitimate Words

If the spell checker flags legitimate words as misspelled, add project-specific terms to the `cspell.yaml` in that project folder. Add cross-cutting terms used by multiple projects to the root `cspell.yaml`.

### Where to Add Words

There are two main places to add legitimate words. Maintain alphabetical order when adding words to keep the dictionary organized:

1. **Project words array**: Add project-specific terms, technical vocabulary, and proper nouns to the `words` array in that project's `cspell.yaml`.

2. **Root baseline dictionary**: Add cross-cutting terms used by multiple projects to the `baseline` dictionary under `dictionaryDefinitions` in the root `cspell.yaml`.


### Example

To add project-specific words, edit the `cspell.yaml` in that project folder:

```yaml
version: "0.2"
import:
    - ../../cspell.yaml
words:
    - customterm
    - myprojectname
    - technicalword
```

To add a cross-cutting word, add it alphabetically to the existing `baseline` dictionary in the root `cspell.yaml`:

```yaml
dictionaryDefinitions:
    - name: baseline
        words:
            - crosscuttingterm
```

### Guidelines

- Use lowercase for words
- Consider whether the word is truly legitimate or if it might be a typo

## Available Scripts

### PowerShell Version (Windows)
- **File**: `Invoke-Cspell.ps1`
- **Usage**: For Windows PowerShell environments

## Usage Examples

```powershell
# Check all files (default)
./eng/common/spelling/Invoke-Cspell.ps1

# Check specific files
./eng/common/spelling/Invoke-Cspell.ps1 -ScanGlobs 'sdk/*/*/PublicAPI/**/*.md'

# Check multiple globs (powershell invocation only)
./eng/common/spelling/Invoke-Cspell.ps1 -ScanGlobs @('sdk/storage/**', 'sdk/keyvault/**')

# Check single file
./eng/common/spelling/Invoke-Cspell.ps1 -ScanGlobs './README.md'
```

## Parameters

- **Job Type**: The cspell command to run (default: `lint`)
- **Scan Globs**: File patterns to check for spelling
- **Config Path**: Location of the `cspell.yaml` configuration file
- **Spell Check Root**: Root directory for relative paths
- **Package Cache**: Working directory for npm dependencies
- **Leave Cache**: Option to preserve the npm package cache

## Requirements

- Node.js and npm must be installed
- The root `cspell.yaml` configuration file must exist
- `jq` command-line JSON processor (for bash version)

## How It Works

1. Creates a temporary working directory for npm packages
2. Copies `package.json` and `package-lock.json` to the working directory
3. Installs npm dependencies using `npm ci`
4. Modifies the cspell configuration to include specified file globs
5. Runs cspell with the modified configuration
6. Restores the original configuration
7. Cleans up temporary files

The scripts ensure that a LICENSE file (or temporary file) is always included in the scan to meet cspell's requirements for the "files" array.
