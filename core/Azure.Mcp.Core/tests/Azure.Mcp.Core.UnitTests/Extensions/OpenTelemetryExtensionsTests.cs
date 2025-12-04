// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class OpenTelemetryExtensionsTests : IDisposable
{
    private readonly string? _originalCollectTelemetry;

    public OpenTelemetryExtensionsTests()
    {
        // Store the original environment variable value
        _originalCollectTelemetry = Environment.GetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY");
    }

    public void Dispose()
    {
        // Restore the original environment variable value
        if (_originalCollectTelemetry == null)
        {
            Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", null);
        }
        else
        {
            Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", _originalCollectTelemetry);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithSupportLoggingFolder_DisablesTelemetry()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", null);
        var services = new ServiceCollection();
        var serviceStartOptions = new ServiceStartOptions
        {
            SupportLoggingFolder = "/tmp/logs"
        };
        services.Configure<ServiceStartOptions>(options =>
        {
            options.SupportLoggingFolder = serviceStartOptions.SupportLoggingFolder;
        });

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;

        // Assert
        Assert.False(config.IsTelemetryEnabled, "Telemetry should be disabled when support logging folder is set");
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithoutSupportLoggingFolder_EnablesTelemetryByDefault()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", null);
        var services = new ServiceCollection();
        services.Configure<ServiceStartOptions>(_ => { });

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;

        // Assert
        Assert.True(config.IsTelemetryEnabled, "Telemetry should be enabled by default when support logging folder is not set");
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithSupportLoggingFolderAndEnvVarTrue_StillDisablesTelemetry()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", "true");
        var services = new ServiceCollection();
        services.Configure<ServiceStartOptions>(options =>
        {
            options.SupportLoggingFolder = "/tmp/logs";
        });

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;

        // Assert
        Assert.False(config.IsTelemetryEnabled, "Telemetry should be disabled when support logging folder is set, regardless of environment variable");
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithEnvVarFalse_DisablesTelemetry()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", "false");
        var services = new ServiceCollection();
        services.Configure<ServiceStartOptions>(_ => { });

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;

        // Assert
        Assert.False(config.IsTelemetryEnabled, "Telemetry should be disabled when AZURE_MCP_COLLECT_TELEMETRY is false");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigureOpenTelemetry_WithEmptyOrWhitespaceSupportLoggingFolder_EnablesTelemetry(string? folderPath)
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY", null);
        var services = new ServiceCollection();
        services.Configure<ServiceStartOptions>(options =>
        {
            options.SupportLoggingFolder = folderPath;
        });

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;

        // Assert
        Assert.True(config.IsTelemetryEnabled, $"Telemetry should be enabled when support logging folder is '{folderPath}'");
    }
}
