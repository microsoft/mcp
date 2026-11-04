// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

/// <summary>Records real tool commands without starting an MCP server or client.</summary>
public abstract class RecordedToolTestsBase(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedTestsBase(output, fixture)
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
        Host.Configure(Settings, TestMode, proxy, CustomArguments);
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
