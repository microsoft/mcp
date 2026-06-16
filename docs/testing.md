# Testing in `microsoft/mcp`

This guide explains how unit tests and live/recorded tests work in this repository, using the **Azure AI Search** toolset as a concrete example. For the full recorded-test workflow (recording, pushing assets, sanitizers), see [`docs/recorded-tests.md`](recorded-tests.md).

## Two Kinds of Tests

Every Azure service toolset ships two complementary kinds of tests that live side-by-side in the same test project (`tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.Tests/`):

| | Unit Tests | Live / Recorded Tests |
|---|---|---|
| **Base class** | `CommandUnitTestsBase<TCommand, TService>` | `RecordedCommandTestsBase` |
| **Azure resources** | None – service is mocked with NSubstitute | Real resources during recording; proxy replay afterwards |
| **Purpose** | Validate command initialization, option parsing, error handling, and service-layer logic | Validate end-to-end tool behaviour against real Azure responses |
| **When they run** | Every CI build, no credentials required | Require live resources to record; replay from stored recordings in CI |
| **Trait** | _(none)_ | `[Trait("TestType", "Live")]` |

---

## Unit Tests

Unit tests are fast, fully offline tests that mock the service layer with [NSubstitute](https://nsubstitute.github.io/). They exercise the command class directly – option registration, argument parsing, result serialization, and error handling – without launching a server or calling Azure.

### Base class: `CommandUnitTestsBase<TCommand, TService>`

The base class (in `core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Tests/Client/CommandUnitTestsBase.cs`) wires up a minimal DI container with:

- A substituted `TService` (available as the `Service` property)
- A substituted `ILogger<TCommand>`
- The real `TCommand` registered as a singleton

It exposes helpers used in every command test:

| Helper | Purpose |
|---|---|
| `ExecuteCommandAsync(params string[] args)` | Parses `args` through the full `System.CommandLine` pipeline and calls `TCommand.ExecuteAsync` |
| `ValidateAndDeserializeResponse<T>(response, jsonTypeInfo)` | Asserts the response is HTTP 200 and deserializes the result using the AOT-safe JSON context |
| `DeserializeResponse<T>(response, jsonTypeInfo)` | Deserializes without asserting the status code |
| `CommandDefinition` | The `System.CommandLine.Command` instance returned by `TCommand.GetCommand()` |
| `Service` | The NSubstitute mock of `TService` |

> **Note for subscription commands**: Commands that extend `SubscriptionCommand<TOptions, TResult>` need `SubscriptionCommandUnitTestsBase<TCommand, TService>` instead. That base class automatically registers a mock `ISubscriptionResolver` in DI; without it, the test will fail at runtime with "Unable to resolve service for type `ISubscriptionResolver`".

### Search example – command unit tests

The Search toolset keeps command unit tests in `Index/`, `Service/`, and `Knowledge/` sub-folders.

**`Service/ServiceListCommandTests.cs`** – tests the `search_service_list` command:

```csharp
public class ServiceListCommandTests : CommandUnitTestsBase<ServiceListCommand, ISearchService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsServices_WhenServicesExist()
    {
        // Arrange: tell the mock what to return
        var expectedServices = new List<string> { "service1", "service2" };
        Service.ListServices(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServices);

        // Act: invoke the command with CLI-style arguments
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert: deserialize the result using the AOT-safe JSON context
        var result = ValidateAndDeserializeResponse(response, SearchJsonContext.Default.ServiceListCommandResult);
        Assert.Equal(expectedServices, result.Services);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        Service.ListServices(/* ... */).ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
```

**`Index/IndexGetCommandTests.cs`** – tests the `search_index_get` command, including `Constructor_InitializesCommandCorrectly`:

```csharp
[Fact]
public void Constructor_InitializesCommandCorrectly()
{
    Assert.Equal("get", CommandDefinition.Name);
    Assert.NotNull(CommandDefinition.Description);
    Assert.NotEmpty(CommandDefinition.Description);

    // Verify the expected options are registered
    Assert.Contains(CommandDefinition.Options, o => o.Name == "--service");
    Assert.Contains(CommandDefinition.Options, o => o.Name == "--index");
}

[Fact]
public async Task ExecuteAsync_ValidatesRequiredOptions()
{
    // Passing an empty string exercises the required-option validation path
    var response = await ExecuteCommandAsync("");

    Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    Assert.Contains("service", response.Message);
}
```

### Search example – service unit tests

Some tests target the service layer directly rather than going through the command. **`Service/SearchServiceTests.cs`** tests `SearchService` methods that contain non-trivial logic:

```csharp
public class SearchServiceTests
{
    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesIntentForMinimalReasoning_WhenMessagesProvided()
    {
        var messages = new List<(string role, string message)>
        {
            ("user", "Hello"),
            ("assistant", "How can I help?")
        };

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(true, null, messages);

        var intent = Assert.IsType<KnowledgeRetrievalSemanticIntent>(request.Intents.Single());
        Assert.Equal("Hello\nHow can I help?", intent.Search);
        Assert.Empty(request.Messages);
    }
}
```

`SearchServiceCacheTests` (in the same file) tests caching behaviour by injecting a substituted `ICacheService` directly into the `SearchService` constructor – this verifies that the service reads from the cache before hitting ARM.

### Running unit tests

```powershell
# Run all unit tests for Search
dotnet test tools/Azure.Mcp.Tools.Search/tests/Azure.Mcp.Tools.Search.Tests

# Run a specific test class
dotnet test --filter "FullyQualifiedName~ServiceListCommandTests"
```

---

## Live / Recorded Tests

Live tests spin up the actual MCP server binary (the real `azmcp` process) and call tools through an MCP client over stdio. They prove that the full end-to-end path works against real Azure responses.

Because running these tests on every commit would require live Azure credentials, they are **recorded**: during a recording pass the test harness routes HTTP traffic through the [Azure SDK Test Proxy](https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/README.md), captures the sanitized responses in JSON files, and stores them in the shared `Azure/azure-sdk-assets` repository. In CI, the tests replay those recordings without needing real credentials.

### Base class hierarchy

```
IAsyncLifetime
└── CommandTestsBase          (live server lifecycle, CallToolAsync, LoadSettingsAsync)
    └── RecordedCommandTestsBase  (test proxy management, sanitizers, record/playback modes)
        └── SearchCommandTests    (Search-specific settings & sanitizers)
```

`CommandTestsBase` (in `core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Tests/Client/CommandTestsBase.cs`):

- Starts the MCP server binary as a child process via `LiveServerFixture`
- Provides `CallToolAsync(toolName, parameters)` which calls the tool over MCP and returns the `results` JSON element
- Loads test settings from `.testsettings.json` (subscription ID, resource names, test mode)

`RecordedCommandTestsBase` adds:

- Auto-download and lifecycle management of the Test Proxy
- `StartRecordOrPlayback()` / `StopRecordOrPlayback()` called around each test
- Virtual collections for customizing sanitizers (see below)
- Support for `TestVariables` (values saved in the recording and replayed during playback)

### Search example – `SearchCommandTests.cs`

```csharp
public class SearchCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string SanitizedValue = "sanitized";
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

    // Opt out of the AZSDK3493 ($..name) sanitizer because Search index names
    // appear in assertions and must survive playback verbatim.
    public override List<string> DisabledDefaultSanitizers =>
    [
        ..base.DisabledDefaultSanitizers,
        "AZSDK3493"
    ];

    // Replace the real resource base name with a stable token in every recording.
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        new(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceBaseName,
            Value = SanitizedValue,
        }),
        new(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.SubscriptionId,
            Value = EmptyGuid,
        }),
    ];

    [Fact]
    public async Task Should_list_search_services_by_subscription_id()
    {
        Assert.NotNull(Settings.SubscriptionId);

        var result = await CallToolAsync(
            "search_service_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var services = result.AssertProperty("services");
        Assert.Equal(JsonValueKind.Array, services.ValueKind);
    }

    [Fact]
    public async Task Should_get_index_details()
    {
        var result = await CallToolAsync(
            "search_index_get",
            new()
            {
                { "service", Settings.ResourceBaseName },
                { "index", "products" }
            });

        var indexes = result.AssertProperty("indexes");
        Assert.Single(indexes.EnumerateArray());

        var index = indexes.EnumerateArray().First();
        Assert.Equal("products", index.AssertProperty("name").GetString());
    }
}
```

Key points:

- `Settings.ResourceBaseName` is the base name of the test Azure Search service, loaded from `.testsettings.json`. During playback it resolves to `"sanitized"` (see `LoadSettingsAsync` override).
- `result.AssertProperty("services")` is an extension method that throws a descriptive assertion failure if the property is missing, which is more informative than using `TryGetProperty`.
- Sanitizers strip real subscription IDs and resource names before writing the recording to disk. This keeps recordings free of live credentials.

### Test settings and playback

Each test project has a `.testsettings.json` file:

```jsonc
{
  "SubscriptionId": "...",
  "SubscriptionName": "...",
  "ResourceGroupName": "...",
  "ResourceBaseName": "my-search-service",
  "TestMode": "Playback"   // or "Record" or "Live"
}
```

When `TestMode` is `Playback` (the default in CI), the harness:

1. Sets `AZURE_TOKEN_CREDENTIALS=PlaybackTokenCredential` – the server returns fake tokens instead of calling Azure AD
2. Sets `TEST_PROXY_URL` – `IHttpClientFactory.CreateClient()` routes all HTTP to the proxy
3. The proxy matches incoming requests against the stored recording and returns the saved responses

### `assets.json` – where recordings live

Each test project includes an `assets.json` file that tells the proxy where to find (or push) recordings:

```json
{
  "AssetsRepo": "Azure/azure-sdk-assets",
  "AssetsRepoPrefixPath": "",
  "TagPrefix": "Azure.Mcp.Tools.Search.Tests",
  "Tag": "Azure.Mcp.Tools.Search.Tests_642cba3ade"
}
```

The `Tag` field points at the exact commit in `Azure/azure-sdk-assets` that contains the current recordings. It is updated by the `push` command after a successful recording session.

### Running live/recorded tests

```powershell
# Run in playback mode (default, no Azure credentials needed)
dotnet test tools/Azure.Mcp.Tools.Search/tests/Azure.Mcp.Tools.Search.Tests

# Switch to record mode and re-run against live resources
# 1. Edit .testsettings.json: "TestMode": "Record"
# 2. Run tests
dotnet test tools/Azure.Mcp.Tools.Search/tests/Azure.Mcp.Tools.Search.Tests
# 3. Push updated recordings
./.proxy/Azure.Sdk.Tools.TestProxy.exe push -a tools/Azure.Mcp.Tools.Search/tests/Azure.Mcp.Tools.Search.Tests/assets.json
# 4. Restore TestMode to "Playback" and re-run to verify playback works
```

For the complete recording workflow including deploying test resources, see [`docs/recorded-tests.md`](recorded-tests.md).

---

## File layout for Search

```
tools/Azure.Mcp.Tools.Search/tests/Azure.Mcp.Tools.Search.Tests/
├── Azure.Mcp.Tools.Search.Tests.csproj
├── assets.json                          # Points to recordings in azure-sdk-assets
├── SearchCommandTests.cs                # Live/recorded tests (RecordedCommandTestsBase)
├── Index/
│   ├── IndexGetCommandTests.cs          # Unit tests for search_index_get
│   └── IndexQueryCommandTests.cs        # Unit tests for search_index_query
├── Service/
│   ├── ServiceListCommandTests.cs       # Unit tests for search_service_list
│   ├── SearchServiceTests.cs            # Service-layer unit tests
│   └── SearchServiceValidateServiceNameTests.cs
└── Knowledge/
    ├── KnowledgeBaseGetCommandTests.cs
    ├── KnowledgeBaseRetrieveCommandTests.cs
    └── KnowledgeSourceGetCommandTests.cs
```

---

## Summary

| | Unit tests | Live / Recorded tests |
|---|---|---|
| **Where** | `Index/`, `Service/`, `Knowledge/` sub-folders | `SearchCommandTests.cs` (project root) |
| **Base class** | `CommandUnitTestsBase<TCommand, ISearchService>` | `RecordedCommandTestsBase` |
| **Mocking** | NSubstitute mock of `ISearchService` | None – real server, real (or recorded) Azure |
| **Assertions** | `Assert.*`, `ValidateAndDeserializeResponse` | `result.AssertProperty(...)`, `Assert.*` |
| **Run anywhere** | ✅ | ✅ (playback) / requires Azure (live/record) |

For more detail on the recording framework, sanitizers, matchers, and troubleshooting, see [`docs/recorded-tests.md`](recorded-tests.md).
