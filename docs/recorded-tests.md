# Recorded Testing in `microsoft/mcp`

## Context

This repository ships CLI tools. Specifically, multiple combinations of `tools` assembled into `mcp servers` that are effectively standalone CLI tools themselves. Developers contribute LiveTests that invoke these tools against live azure resources and verify the output is as expected.

## Architecture Overview

- **CLI and Servers** – MCP ships multiple CLI-like toolsets that can run under the MCP server host. Commands typically interact with Azure resources.
- **Test Harness** – Live tests inherit from [`CommandTestsBase`](https://github.com/microsoft/mcp/blob/main/core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/CommandTestsBase.cs). **Recorded** tests inherit from [`RecordedCommandTestsBase`](https://github.com/microsoft/mcp/blob/main/core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/RecordedCommandTestsBase.cs) The harness:
  - Auto-downloads the Test Proxy into the repo at `.proxy/Azure.Sdk.Tools.TestProxy.exe` (Windows) or `.proxy/Azure.Sdk.Tools.TestProxy` for unix platforms.
  - Handles start/stop of the proxy as necessary
  - Registers any behavior changes from default for the auto-started proxy
  - Manages recording state (`Record`, `Playback`, `Live`) based on `.testsettings.json`.

- **HTTP Redirect** – In Debug builds the server-side `HttpClientService.CreateClient()` automatically routes traffic through the proxy when `TEST_PROXY_URL` is set. Tests don’t need to customize transports, they merely need to ensure the tool they are testing is correctly injecting and utilizing `HttpClientService`.

## Test Proxy Primer (Relevant Bits)

The Azure SDK Test Proxy is a cross-language recorder/playback service. Full upstream documentation lives in the Azure SDK Tools repo:

- [Test Proxy README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/README.md)
- [Asset Sync README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/documentation/asset-sync/README.md)

For MCP developers, the key takeaways are:

- The proxy exposes various endpoints that affect matching behavior, sanitization of recordings at rest and during playback, and other transport customizations. `RecordedCommandTestsBase` handles these calls automatically.
- Recordings are **externalized** via `assets.json` files and stored in the shared `Azure/azure-sdk-assets` repository. The proxy clones the relevant slice into `.assets/<hash>/...` on demand.
- Asset management commands are exposed through the proxy CLI (`restore`, `reset`, `push`, `config locate/show`). MCP developers invoke these via the auto-downloaded binary in `.proxy/`.

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
1. **Set record mode** – Locate the `.testsettings.json` next to your test project (for example `tools/Azure.Mcp.Tools.KeyVault/tests/Azure.Mcp.Tools.KeyVault.LiveTests/.testsettings.json`). Update the file `TestMode` value to `Record`:
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
   - Note that on `unix` platforms there is no `.exe` suffix.
4. **Switch to playback** – Change the `TestMode` value in `.testsettings.json` to `Playback`. Re-run the tests to verify they pass without hitting live resources.
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

## Migration Guide (Live ➜ Recorded)

1. **Rebase on latest** – Ensure your branch includes the current recorded-test infrastructure.
2. **Re-parent the test class** – Update live tests to inherit from `RecordedCommandTestsBase` instead of `CommandTestsBase`.
3. **Ensure proxy-aware HTTP usage** – Commands must obtain `HttpClient` instances via `HttpClientService.CreateClient()` to benefit from playback redirection.
4. **Add `assets.json`** – If the toolset doesn’t have one, create `tools/<Tool>/tests/<LiveTest.CsProj.Folder>/assets.json`:
   ```json
   {
     "AssetsRepo": "Azure/azure-sdk-assets",
     "AssetsRepoPrefixPath": "",
     "TagPrefix": "Azure.Mcp.Tools.YourService",
     "Tag": ""
   }
   ```
5. **Record and push** – Follow the workflow above to generate recordings and push them to the assets repo.
6. **Document sanitizers** – Leave brief comments explaining why custom sanitizers exist to help future maintainers.

Example Migrations:
 - [Azure.Mcp.Tools.KeyVault](https://github.com/microsoft/mcp/pull/1080)

## Working With Sanitizers and Matchers

The test proxy supports abstractions that must be understood:

- `Sanitizers`: Applied before writing a recording to disk, and while matching requests in `playback` mode.
  - Think of these as regex-based censors that blank out sensitive parts of your recording.
- `Matchers`: By default, the test-proxy compares all parts of the request: headers, body bytes, and the URI
  - These can be optionally overridden for all tests within a test class or for an individual test case.

`RecordedCommandTestsBase` exposes virtual collections for customization:

- `GeneralRegexSanitizers` – global replacements across URI/body/headers.
- `HeaderRegexSanitizers` – replace specific header values.
- `BodyKeySanitizers` / `BodyRegexSanitizers` – patch JSON fields or bodies.
- `UriRegexSanitizers` – mask host or query segments.
- `DisabledDefaultSanitizers` – opt out of built-in sanitizers if they interfere with playback.

`RecordedCommandTestsBase` exposes a global configuration via:
- Overridable `TestMatcher` property
- OR devs can set the attribute `[CustomMatcher]` on an individual test-case to adjust the matching behavior for a specific test.

### In practice

#### Playback variables

When writing tests, users can identify values that should be retrieved from the recording during `playback` mode.

Example:

```cs
    [Fact]
    public async Task Should_create_key()
    {
        var keyName = "key" + Random.Shared.NextInt64();

        RegisterVariable("keyName", keyName); // register a variable for save when recording ends

        var result = await CallToolAsync(
            "keyvault_key_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "vault", Settings.ResourceBaseName },
                // during playback, the saved value from recording will be retrieved and utilized
                { "key", TestVariables["keyName"]},
                { "key-type", KeyType.Rsa.ToString() }
            });
```

This means values that don't make sense for `sanitization` can be propagated to the recording and automatically retrieved by the test-proxy harness during `playback`.

#### An example of setting each sanitizer type

```cs
public class SampleRecordedTest(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture) {

    // given a json path
    public override List<BodyKeySanitizer> BodyKeySanitizers => new()
    {
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id") // this input uses JSONPath syntax
        {
            // Regex = ".*" by default
            // GroupForReplace = null (replace entire match)
            Value = "Sanitized"
        }),
        // clear out latter half of a Body Key by targeting group
        // named groups are also supported
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.attributes.recoveryLevel")
        {
            Regex = "Recoverable(.*)",
            GroupForReplace = "0",
            Value = ""
        })
    };

    public override List<BodyRegexSanitizer> BodyRegexSanitizers => new List<BodyRegexSanitizer>() {
        // should clear out kid hostnames of actual vault names appearing anywhere in any section
        // of the body
        new BodyRegexSanitizer(new BodyRegexSanitizerBody() {
          Regex = "(?=http://|https://)(?<host[^/?\.]+)",
          GroupForReplace = "host",
        })
    };

    public override List<UriRegexSanitizer> UriRegexSanitizers => new()
    {
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "/subscriptions/(?<sub>[^/]+)/",
            GroupForReplace = "sub",
            Value = "00000000-0000-0000-0000-000000000000"
        })
    };

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers => new()
    {
        // named regex replace example.
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Authorization")
        {
            Regex = "Bearer (?<token>.+)",
            GroupForReplace = "token",
            Value = "Sanitized"
        })
    };

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers => new()
    {
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            // notice escaped \ for \s regex character
            Regex = "tenantId\\s*:\\s*(?<tenant>[0-9a-fA-F-]{36})",
            GroupForReplace = "tenant",
            Value = "00000000-0000-0000-0000-000000000000"
        })
    };
    ...
```

#### Setting the matcher

```cs
public class SampleRecordedTest(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture) {
    public override CustomDefaultMatcher? TestMatcher { get; set; } = new CustomDefaultMatcher()
    {
        // By default, request and response bodies are compared during matching. You can disable this by setting CompareBodies to false.
        CompareBodies = false,
        // By default query ordering is considered a different URI during matching. To ignore query ordering, set IgnoreQueryOrdering to true.
        IgnoreQueryOrdering = true,
        // During matching, excluded headers are totally excluded from matching. Both presence and value are not compared.
        ExcludedHeaders = "x-ms-request-id,x-ms-correlation-request-id,Date,Strict-Transport-Security,Transfer-Encoding,Content-Length",
        // Ignored headers are compared for presence only, not value. EG x-ms-client-request-id present on a recording, but not on incoming request will not cause a mismatch.
        IgnoredHeaders = "x-ms-client-request-id"
    };
```

#### Overriding matcher for specific recording

```cs
    [Fact]
    [CustomMatcher(compareBody: false)] // this test will ignore the body during matching operations
    public async Task Should_import_certificate()
```

## Troubleshooting Tips

- **Proxy missing** – Delete `.proxy/` and re-run the tests; the harness re-downloads the latest release automatically.
- **Recordings missing** – Use `config locate` to confirm where the sparse clone lives. Check timestamps under `.assets/`.
- **Playback mismatch** – Add sanitizers for dynamic data, adjust the matcher to ignore irrelevant fields, or register a variable.
- **Need a clean slate** – Run `reset` before re-recording to ensure the sparse clone matches the tagged state.

## Additional Resources

- [RecordedCommandTestsBase source](https://github.com/microsoft/mcp/blob/main/core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/RecordedCommandTestsBase.cs)
- [Azure SDK Test Proxy README](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/README.md)
- [Test Proxy Asset Sync Guide](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/documentation/asset-sync/README.md)
  - Details on how assets are stored in `Azure/azure-sdk-assets` repo
- [Azure SDK Test Proxy Discussions](https://teams.microsoft.com/l/channel/19%3Ab7c3eda7e0864d059721517174502bdb%40thread.skype/Test-Proxy%20-%20Questions%2C%20Help%2C%20and%20Discussion?groupId=3e17dcb0-4257-4a30-b843-77f47f1d4121&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)
  - Feel free to post any questions about the test-proxy here in addition to the standard MCP channels.

