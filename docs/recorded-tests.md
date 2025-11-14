# Recorded Testing in `microsoft/mcp`

## Context

This repository ships CLI tools. Specifically, multiple combinations of `tools` assembled into `mcp servers` that are effectively standalone CLI tools themselves. Developers contribute LiveTests that invoke these tools against live azure resources and verify the output is as expected.

## Architecture Overview

- **CLI and Servers** – MCP ships multiple CLI-like toolsets that can run under the MCP server host. Commands typically interact with Azure resources.
- **Test Harness** – Live tests inherit from [`CommandTestsBase`](../core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/CommandTestsBase.cs). **Recorded** tests inherit from [`RecordedCommandTestsBase`](../core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/CommandTestsBase.cs) The harness:
  - Auto-downloads the Test Proxy into the repo at `.proxy/Azure.Sdk.Tools.TestProxy.exe` (Windows) or `.proxy/Azure.Sdk.Tools.TestProxy` for unix platforms.
  - Handles start/stop of the proxy as necessary
  - Registers any behavior changes from default for the auto-started proxy
  - Manages recording state (`Record`, `Playback`, `Live`) based on `.livesettings.json`.

- **HTTP Redirect** – In Debug builds the server-side `HttpClientService.CreateClient()` automatically routes traffic through the proxy when `TEST_PROXY_URL` is set. Tests don’t need to customize transports, they merely need to ensure the tool they are testing is correctly injecting and utilizing `HttpClientService`.

## Test Proxy Primer (Relevant Bits)

The Azure SDK Test Proxy is a cross-language recorder/playback service. Full upstream documentation lives in the Azure SDK Tools repo:

- [Test Proxy README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/README.md)
- [Asset Sync README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/documentation/asset-sync/README.md)

For MCP developers, the key takeaways are:

- The proxy exposes various endpoints that affect matching behavior, sanitization of recordings at rest and during playback, and other transport customizations. `RecordedCommandTestsBase` handles these calls automatically.
- Recordings are **externalized** via `assets.json` files and stored in the shared `Azure/azure-sdk-assets` repository. The proxy clones the relevant slice into `.assets/<hash>/...` on demand.
- Asset management commands are exposed through the proxy CLI (`restore`, `reset`, `push`, `config locate/show`). MCP devs invoke these via the auto-downloaded binary in `.proxy/`.

## Repository Layout Recap

```
docs/recorded-tests.md             # this file
core/Azure.Mcp.Core/tests/...      # RecordedCommandTestsBase and supporting infrastructure
.proxy/                            # auto-downloaded Test Proxy binaries (created on demand)
.assets/                           # sparse clones of Azure/azure-sdk-assets slices
```

The `.proxy` directory is recreated whenever a recorded test run needs the Test Proxy. This folder is gitignored by default. Do not commit these binaries.

## Recording Workflow

Follow this checklist any time you need to update recordings:

0. **Deploy LiveResources** - `Connect-AzAccount` with your targeted subscription, then invoke `./eng/scripts/Deploy-TestResources.ps1`. EG `./eng/scripts/Deploy-TestResources.ps1 -Paths KeyVault`.
1. **Set record mode** – Locate the `.livesettings.json` next to your test project (for example `tools/Azure.Mcp.Tools.KeyVault/tests/Azure.Mcp.Tools.KeyVault.LiveTests/..livetestsettings.json`). Update the file `TestMod` value to `Record`:
   ```jsonc
   {
     // ...
     "TestMode": "Record"
     // ...
   }
   ```
2. **Run tests** – Invoke the live test project (e.g. `dotnet test tools/Azure.Mcp.Tools.KeyVault/tests/Azure.Mcp.Tools.KeyVault.LiveTests`). The harness boots the proxy, registers default sanitizers, and writes fresh recordings under `.assets/`.
3. **Inspect recordings** – Use the helper to locate the exact folder:
   ```powershell
   ./.proxy/Azure.Sdk.Tools.TestProxy.exe config locate -a tools/Azure.Mcp.Tools.KeyVault/tests/assets.json
   ```
   Review each JSON recording and confirm no secrets or unstable data were missed by existing sanitizers.
4. **Switch to playback** – Change the `TestMode` value in `.livetestsettings.json` to `playback`. Re-run the tests to verify they pass without hitting live resources.
5. **Push assets** – When satisfied, publish the updated recordings:
   ```powershell
   ./.proxy/Azure.Sdk.Tools.TestProxy.exe push -a tools/Azure.Mcp.Tools.KeyVault/tests/assets.json
   ```
   This stages the local recording updates for commit, creates a new tag in `Azure/azure-sdk-assets`, and updates the `Tag` field in local `assets.json` to reflect new recording location.
6. **Commit** to `mcp` repo – Include:
   - Source changes
   - Updated `assets.json`
   - Optional change-log entry as needed

### Helpful Commands

| Scenario | Command |
|---|---|
| Restore recordings referenced by an assets file | `./.proxy/Azure.Sdk.Tools.TestProxy.exe restore -a path/to/assets.json` |
| Reset local clone to the current tag | `./.proxy/Azure.Sdk.Tools.TestProxy.exe reset -a path/to/assets.json` |

## 5. Migration Guide (Live ➜ Recorded)

1. **Rebase on latest** – Ensure your branch includes the current recorded-test infrastructure.
2. **Reparent the test class** – Update live tests to inherit from `RecordedCommandTestsBase` instead of `CommandTestsBase`. Override `BodyKeySanitizers`, `BodyRegexSanitizers`, etc. as needed.
3. **Ensure proxy-aware HTTP usage** – Commands must obtain `HttpClient` instances via `HttpClientService.CreateClient()` to benefit from playback redirection.
4. **Add `assets.json`** – If the toolset doesn’t have one, create `tools/<Tool>/tests/assets.json`:
   ```json
   {
     "AssetsRepo": "Azure/azure-sdk-assets",
     "AssetsRepoPrefixPath": "net",
     "TagPrefix": "net/Azure.Mcp.Tools.YourService",
     "Tag": ""
   }
   ```
5. **Record and push** – Follow the workflow above to generate recordings and push them to the assets repo.
6. **Document sanitizers** – Leave brief comments explaining why custom sanitizers exist to help future maintainers.

Example Migrations:
 - [Azure.Mcp.Tools.KeyVault](https://github.com/microsoft/mcp/pull/1080)

## 6. Working With Sanitizers, Matchers, and Transforms

`RecordedCommandTestsBase` exposes virtual collections for customization:

- `GeneralRegexSanitizers` – global replacements across URI/body/headers.
- `HeaderRegexSanitizers` – replace specific header values.
- `BodyKeySanitizers` / `BodyRegexSanitizers` – patch JSON fields or bodies.
- `UriRegexSanitizers` – mask host or query segments.
- `DisabledDefaultSanitizers` – opt out of built-in sanitizers if they interfere with playback.
- `TestMatcher` or `[CustomMatcher]` – adjust matching rules during playback.

todo: examples of each

## 7. Troubleshooting Tips

- **Proxy missing** – Delete `.proxy/` and re-run the tests; the harness re-downloads the latest release automatically.
- **Recordings missing** – Use `config locate` to confirm where the sparse clone lives. Check timestamps under `.assets/`.
- **Playback mismatch** – Add sanitizers for dynamic data, adjust the matcher to ignore irrelevant fields, or register a variable.
- **Need a clean slate** – Run `reset` before re-recording to ensure the sparse clone matches the tagged state.

## 8. Additional Resources

- [RecordedCommandTestsBase source](../core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/RecordedCommandTestsBase.cs)
- [Azure SDK Test Proxy README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/README.md)
- [Test Proxy Asset Sync Guide](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/documentation/asset-sync/README.md)
  - Details on how assets are stored in `Azure/azure-sdk-assets` repo
- [Azure SDK Test Proxy Discussions](https://teams.microsoft.com/l/channel/19%3Ab7c3eda7e0864d059721517174502bdb%40thread.skype/Test-Proxy%20-%20Questions%2C%20Help%2C%20and%20Discussion?groupId=3e17dcb0-4257-4a30-b843-77f47f1d4121&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)
  - Feel free to post any questions about the test-proxy here in addition to the standard MCP channels.

Keeping this document up to date ensures everyone follows the same playbook for safe, deterministic recorded tests. Cross-link any new tooling or scripts you introduce so future devs can pick up where you left off.

