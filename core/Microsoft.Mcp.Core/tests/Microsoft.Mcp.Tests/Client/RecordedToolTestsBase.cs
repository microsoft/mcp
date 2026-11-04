// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.Services.Time;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

/// <summary>Records real tool commands without starting an MCP server or client.</summary>
public abstract class RecordedToolTestsBase(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedTestsBase(output, fixture)
{
    private ServiceProvider? _services;
    private DirectToolInvoker? _invoker;
    private TestEnvironmentScope? _environment;

    protected abstract IAreaSetup Area { get; }

    public override async ValueTask InitializeAsync()
    {
        var settings = LiveTestSettings.TryLoadTestSettings(out var configured) ? configured : PlaybackSettings;
        Dictionary<string, string?> environment = settings.EnvironmentVariables
            .ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        environment["TEST_MODE"] = settings.TestMode.ToString();
        _environment = new TestEnvironmentScope(environment);
        try
        {
            await base.InitializeAsync();
        }
        catch
        {
            await DisposeExecutionAsync();
            throw;
        }
    }

    protected override async ValueTask LoadSettingsAsync()
    {
        await base.LoadSettingsAsync();
        Settings = new LiveTestSettings
        {
            PrincipalName = Settings.PrincipalName,
            IsServicePrincipal = Settings.IsServicePrincipal,
            TenantId = Settings.TenantId,
            TenantName = Settings.TenantName,
            SubscriptionId = Settings.SubscriptionId,
            SubscriptionName = Settings.SubscriptionName,
            ResourceGroupName = Settings.ResourceGroupName,
            ResourceBaseName = Settings.ResourceBaseName,
            SettingsDirectory = Settings.SettingsDirectory,
            TestPackage = Settings.TestPackage,
            TestMode = Settings.TestMode,
            DebugOutput = Settings.DebugOutput,
            DeploymentOutputs = new(Settings.DeploymentOutputs),
            EnvironmentVariables = new(Settings.EnvironmentVariables)
        };
    }

    protected override ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
    {
        if (CustomArguments is { Length: > 0 } || !string.IsNullOrEmpty(Settings.TestPackage))
        {
            throw new InvalidOperationException(
                "Direct tool tests do not launch server arguments or npm packages. Use RecordedCommandTestsBase for process-level tests.");
        }
#if !DEBUG
        if (TestMode != TestMode.Live)
        {
            throw new InvalidOperationException("Direct recording and playback require a Debug build with recording transports enabled.");
        }
#endif
        Dictionary<string, string?> environment = Settings.EnvironmentVariables
            .ToDictionary(pair => pair.Key, pair => (string?)pair.Value);
        environment["TEST_MODE"] = TestMode.ToString();
        _environment!.SetVariables(environment);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddEnvironmentVariables().Build());
        services.AddOptions<ServerRuntimeConfiguration>();
        services.AddSingleton<ITelemetryService, NoopTelemetryService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IExternalProcessService, ExternalProcessService>();
        services.AddCommandContextAccessor();
        services.AddEndpointValidation();
        services.AddMemoryCache();
        services.AddSingleUserCliCacheService(disabled: true);
        services.AddHttpClientServices(options => options.RecordingProxy = null);
        services.ConfigureDefaultHttpClient(proxy is null ? null : proxy.GetProxyUri);
        services.ConfigureArmHttpClient();
        services.Configure<McpServerConfiguration>(options =>
        {
            options.Name = "Direct tool tests";
            options.ShortName = "direct";
            options.DisplayName = "Direct tool tests";
            options.Description = "In-process tool integration tests";
            options.RootCommandGroupName = "azmcp";
            options.Version = "test";
            options.IsTelemetryEnabled = false;
        });
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<DirectToolInvoker>();
        services.AddSingleton(Area);
        ConfigureServices(services);
        Area.ConfigureServices(services);
        _services = services.BuildServiceProvider(validateScopes: true);
        _invoker = _services.GetRequiredService<DirectToolInvoker>();
        return ValueTask.CompletedTask;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected override async Task<JsonElement> InvokeToolAsync(
        string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken)
    {
        var arguments = parameters.ToDictionary(
            pair => pair.Key,
            pair => JsonSerializer.SerializeToElement(pair.Value, ToolTestJsonContext.Default.Object));
        var response = await (_invoker ?? throw new InvalidOperationException("Direct tool tests have not been initialized."))
            .InvokeAsync(command, arguments, cancellationToken);
        return JsonSerializer.SerializeToElement(response, ModelsJsonContext.Default.CommandResponse);
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            await DisposeExecutionAsync();
        }
    }

    private async ValueTask DisposeExecutionAsync()
    {
        try
        {
            if (_services is not null)
            {
                await _services.DisposeAsync();
            }
        }
        finally
        {
            _services = null;
            _invoker = null;
            _environment?.Dispose();
            _environment = null;
        }
    }
}
