// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands;

// This is intentionally placed after the namespace declaration to avoid
// conflicts with Azure.Mcp.Core.Areas.Server.Options
using Options = Microsoft.Extensions.Options.Options;

public class ServiceCollectionExtensionsSerializedTests
{
    private IServiceCollection SetupBaseServices()
    {
        var services = CommandFactoryHelpers.SetupCommonServices();
        services.AddSingleton<CommandFactory>(sp => CommandFactoryHelpers.CreateCommandFactory(sp));

        return services;
    }

    [Fact]
    public void InitializeConfigurationAndOptions_Defaults()
    {
        // Assert
        var expectedVersion = AssemblyHelper.GetAssemblyVersion(typeof(ServiceCollectionExtensionsTests).Assembly);
        var services = SetupBaseServices();

        // Act
        ServiceCollectionExtensions.InitializeConfigurationAndOptions(services);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>();

        Assert.NotNull(options.Value);

        var actual = options.Value;
        Assert.Equal("Azure.Mcp.Server", actual.Name);
        Assert.Equal("Azure MCP Server", actual.DisplayName);
        Assert.Equal("azmcp", actual.RootCommandGroupName);
        Assert.Equal(expectedVersion, actual.Version);

        Assert.True(actual.IsTelemetryEnabled);
    }

    /// <summary>
    /// When <see cref="TransportTypes.Http"/> is used, telemetry is disabled
    /// even when AZURE_MCP_COLLECT_TELEMETRY is explicitly set to true.
    /// </summary>
    [Fact]
    public void InitializeConfigurationAndOptions_HttpTransport()
    {
        // Assert
        var serviceStartOptions = new ServiceStartOptions
        {
            Transport = TransportTypes.Http,
        };
        var services = SetupBaseServices().AddSingleton(Options.Create(serviceStartOptions));

        // Act
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", "true");
        ServiceCollectionExtensions.InitializeConfigurationAndOptions(services);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>();

        Assert.NotNull(options.Value);

        var actual = options.Value;
        Assert.Equal("Azure.Mcp.Server", actual.Name);
        Assert.Equal("Azure MCP Server", actual.DisplayName);
        Assert.Equal("azmcp", actual.RootCommandGroupName);
        Assert.False(actual.IsTelemetryEnabled);
    }

    [Fact]
    public void InitializeConfigurationAndOptions_Stdio()
    {
        // Assert
        var expectedVersion = AssemblyHelper.GetAssemblyVersion(typeof(ServiceCollectionExtensionsTests).Assembly);
        var services = SetupBaseServices();

        // Act
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", "false");
        ServiceCollectionExtensions.InitializeConfigurationAndOptions(services);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>();

        Assert.NotNull(options.Value);

        var actual = options.Value;
        Assert.Equal("Azure.Mcp.Server", actual.Name);
        Assert.Equal("Azure MCP Server", actual.DisplayName);
        Assert.Equal("azmcp", actual.RootCommandGroupName);
        Assert.Equal(expectedVersion, actual.Version);

        Assert.False(actual.IsTelemetryEnabled);
    }
}
