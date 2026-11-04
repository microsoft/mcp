// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

/// <summary>Executes live-only tool commands without an MCP process or recording session.</summary>
public abstract class DirectToolTestsBase(ITestOutputHelper output) : ToolTestsBase(output)
{
    private DirectToolTestHost? _host;
    private DirectToolTestHost Host => _host ??= new(Area, ConfigureServices);

    protected abstract IAreaSetup Area { get; }

    public override async ValueTask InitializeAsync()
    {
        Host.InitializeEnvironment();
        try
        {
            await base.InitializeAsync();
        }
        catch
        {
            await Host.DisposeAsync();
            throw;
        }
    }

    protected override async ValueTask LoadSettingsAsync()
    {
        await base.LoadSettingsAsync();
        Settings = Settings.Clone();
    }

    protected override ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
    {
        if (TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Live)
        {
            throw new InvalidOperationException("Direct live-only tests require Live mode. Use RecordedToolTestsBase for recording or playback.");
        }
        Host.Configure(Settings, TestMode, null, CustomArguments);
        return ValueTask.CompletedTask;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected override Task<JsonElement> InvokeToolAsync(
        string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken)
        => Host.InvokeAsync(command, parameters, cancellationToken);

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            if (_host is not null)
            {
                await _host.DisposeAsync();
            }
        }
    }
}
