# Tool Rename Checklist

This document defines the steps required when renaming an existing MCP tool (i.e., changing the value returned by a command's `Name` property or its parent group name). Tool names form part of the MCP protocol surface, they are how AI agents discover and invoke capabilities, so renames are **breaking changes** and must be handled carefully.

> **Note:** If you are adding a *new* tool rather than renaming an existing one, follow [`new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md) instead.

## What counts as a tool rename?

A tool name is the underscore-separated string exposed to MCP clients (e.g. `azmcp_storage_account_list`, `azmcp_sql_db_get`). It is derived from the command hierarchy: each group and leaf command's `Name` property is joined with `_` and prefixed with `azmcp_`. Changing *any* segment of that hierarchy, the service group, resource group, or operation leaf is a rename.
Examples:
- `azmcp_sql_db_get` → `azmcp_sql_database_get` — rename
- Splitting a combined get/list command into `azmcp_sql_database_get` and `azmcp_sql_database_list` — rename (old name disappears)
- Updating `Description` or `Title` only — **not** a rename

---

## Checklist

### 1. Source code

- [ ] Update the `Name` property on the command class (and parent group command if the group is also changing).
- [ ] Update any hardcoded group name string literals in the toolset's `*Setup.cs` `RegisterCommands` method (e.g. `new CommandGroup("db", ...)` → `new CommandGroup("database", ...)`). Group segment names are **not** derived from a command's `Name` property — they must be changed here explicitly.
- [ ] Update the `Id` property to a new unique GUID if the semantic meaning of the tool has changed materially (not required for pure name-only fixes).
- [ ] Update the command class filename and any references to match the new name (`{Resource}{Operation}Command.cs`).
- [ ] Update the `Title` constant to reflect the new name (used in logs and telemetry).
- [ ] Update the `Description` to remove any references to the old tool name or old CLI equivalent.
- [ ] Update option definitions and binding if parameter names are changing alongside the rename.
- [ ] Run `dotnet build` and resolve all compiler errors before proceeding.

### 2. Docs

- [ ] Update [`servers/Azure.Mcp.Server/docs/azmcp-commands.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md) — rename the entry and update any cross-references to the old name.
- [ ] Update [`servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md) — update or add test prompts that reference the old tool name.
- [ ] Update `core/Microsoft.Mcp.Core/src/Areas/Server/Resources/consolidated-tools.json` — if the renamed tool appears in any `mappedToolList` array, replace the old tool name with the new one. Also review the `description` field of the parent consolidated tool entry to ensure it still accurately describes the grouped capabilities.
- [ ] Search the entire repo for the old tool name string (e.g. `grep -r "old_tool_name"`) and update any remaining documentation, README files, or code comments found.

### 3. Recordings

Recorded tests reference tool names inside their JSON session files. Old recordings will no longer match the renamed tool and must be re-recorded using the externalized recording workflow (`assets.json` + `.assets/<hash>/...` sparse clone of `Azure/azure-sdk-assets`).

- [ ] Re-record tests for the affected tool following [`docs/recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md), ensuring any recordings that reference the old tool name are updated in the external assets repository.
- [ ] Push updated recordings using the local test proxy and commit the new `assets.json` tag. See [`docs/recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md) for the full workflow:
  ```powershell
  ./.proxy/Azure.Sdk.Tools.TestProxy.exe push -a path/to/assets.json
  ```
- [ ] Verify playback passes locally:
  ```powershell
  dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.LiveTests --no-build
  ```

### 4. Unit tests

- [ ] Update any unit test that constructs command args using the old CLI name (e.g. string literals in `Parse(...)` calls).
- [ ] Update test class names and file names if they encode the old command name.
- [ ] Confirm all unit tests pass:
  ```powershell
  dotnet test tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.UnitTests
  ```

### 5. Changelog

A tool rename is a **breaking change** for any MCP client or agent that hard-codes the old tool name.

- [ ] Create a changelog entry in the `Breaking Changes` section:
  ```powershell
  ./eng/scripts/New-ChangelogEntry.ps1 `
    -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" `
    -Description "Renamed tool old_tool_name to new_tool_name." `
    -Section "Breaking Changes"
  ```
  See [`docs/changelog-entries.md`](https://github.com/microsoft/mcp/blob/main/docs/changelog-entries.md) for full guidance.

### 6. Spelling

- [ ] Run the spelling check and add any new technical terms to `.vscode/cspell.json` if needed:
  ```powershell
  .\eng\common\spelling\Invoke-Cspell.ps1
  ```

### 7. Final verification

- [ ] Open a PR with **all** of the above changes in a single commit or stacked commits.
- [ ] The PR description must call out the old and new tool names explicitly so reviewers can assess client impact.
- [ ] Tag the PR with the `breaking-change` label.

---

## Why tool renames are breaking changes

MCP clients including AI agents, IDE extensions, and automation scripts reference tools by their exact name. When a name changes:

- Agents that have cached tool manifests will silently stop finding the renamed tool.
- Users who have configured explicit tool allow-lists by name must update their configuration.
- Recorded sessions keyed on the old name will fail playback until re-recorded.

For these reasons, **avoid renames unless necessary**. If the existing name is genuinely misleading or inconsistent with the naming conventions in [`new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md), prefer doing the rename as part of a larger breaking-change release batch rather than as a standalone PR.
