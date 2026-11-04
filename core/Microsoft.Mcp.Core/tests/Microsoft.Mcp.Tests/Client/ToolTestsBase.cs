// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

[Trait("TestType", "Live")]
public abstract class ToolTestsBase(ITestOutputHelper output) : IAsyncLifetime, IDisposable
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    protected LiveTestSettings Settings { get; set; } = default!;
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;

    public string[]? CustomArguments;
    public TestMode TestMode = TestMode.Live;

    private static Lazy<Task<LiveTestSettings?>> s_liveSettingsTask = CreateLiveSettingsTask();

    public void SetArguments(params string[] arguments) => CustomArguments = arguments;

    public virtual async ValueTask InitializeAsync()
    {
        await LoadSettingsAsync();
        CheckLiveTestOnly(TestMethodResolver.TryResolveCurrentMethodInfo());
        await InitializeAsyncInternal(null);
    }

    public static LiveTestSettings PlaybackSettings => new()
    {
        SubscriptionId = "00000000-0000-0000-0000-000000000000",
        TenantId = "00000000-0000-0000-0000-000000000000",
        ResourceBaseName = "Sanitized",
        SubscriptionName = "Sanitized",
        TenantName = "Sanitized",
        ResourceGroupName = "Sanitized",
        TestMode = TestMode.Playback
    };

    protected virtual async ValueTask LoadSettingsAsync()
    {
        Settings = await TryLoadLiveSettingsAsync().ConfigureAwait(false) ?? PlaybackSettings;
        if (Settings.TestMode == TestMode.Playback)
        {
            Settings = PlaybackSettings;
        }
        TestMode = Settings.TestMode;
    }

    private static Lazy<Task<LiveTestSettings?>> CreateLiveSettingsTask()
        => new(LoadLiveSettingsCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    private static async Task<LiveTestSettings?> TryLoadLiveSettingsAsync()
    {
        var cached = Volatile.Read(ref s_liveSettingsTask);
        try
        {
            return await cached.Value.ConfigureAwait(false);
        }
        catch
        {
            Interlocked.CompareExchange(ref s_liveSettingsTask, CreateLiveSettingsTask(), cached);
            throw;
        }
    }

    private static async Task<LiveTestSettings?> LoadLiveSettingsCoreAsync()
    {
        try
        {
            var fixture = new LiveTestSettingsFixture();
            await fixture.InitializeAsync().ConfigureAwait(false);
            return fixture.Settings;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    protected abstract ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null);

    protected abstract Task<JsonElement> InvokeToolAsync(
        string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken);

    protected Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters)
        => CallToolCoreAsync(command, parameters, token => InvokeToolAsync(command, parameters, token));

    protected Task<JsonElement?> CallToolAsync(
        string command,
        Dictionary<string, object?> parameters,
        Func<JsonElement, JsonElement?> resultProcessor)
        => CallToolCoreAsync(command, parameters, token => InvokeToolAsync(command, parameters, token), resultProcessor);

    protected async Task<JsonElement?> CallToolCoreAsync(
        string command,
        Dictionary<string, object?> parameters,
        Func<CancellationToken, Task<JsonElement>> invoke,
        Func<JsonElement, JsonElement?>? resultProcessor = null)
    {
        bool debug = Settings.DebugOutput ||
            string.Equals(Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG"), "true", StringComparison.OrdinalIgnoreCase);
        Action<string> writeOutput = debug ? Output.WriteLine : text => FailureOutput.AppendLine(text);

        writeOutput($"request: {command}; parameters: {string.Join(", ", parameters.Keys)}");
        var root = await invoke(TestContext.Current.CancellationToken);
        if (root.ValueKind != JsonValueKind.Object)
        {
            writeOutput($"response: {root}");
            throw new JsonException("Invalid JSON response.");
        }

        JsonObject trimmed = root.Deserialize(ToolTestJsonContext.Default.JsonObject)!;
        trimmed.Remove("args");
        writeOutput($"response: {trimmed.ToJsonString(new JsonSerializerOptions { WriteIndented = true })}");

        return resultProcessor is not null
            ? resultProcessor(root)
            : root.TryGetProperty("results", out var result) ? result : null;
    }

    internal void CheckLiveOnly() => CheckLiveTestOnly(TestMethodResolver.TryResolveCurrentMethodInfo());

    protected void CheckLiveTestOnly(MethodInfo? methodInfo)
    {
        if (TestMode != TestMode.Live && methodInfo?.GetCustomAttribute<LiveTestOnlyAttribute>() is not null)
        {
            Assert.Skip("Test is marked [LiveTestOnly] and cannot run in Playback or Record mode.");
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (TestContext.Current?.TestState?.Result == TestResult.Failed && FailureOutput.Length > 0)
        {
            Output.WriteLine(FailureOutput.ToString());
        }
    }

    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
}
