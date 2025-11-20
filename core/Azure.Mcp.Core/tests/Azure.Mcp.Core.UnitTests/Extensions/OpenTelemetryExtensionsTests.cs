// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class OpenTelemetryExtensionsTests
{
    [Theory]
    [InlineData("none")]
    [InlineData("NONE")]
    [InlineData("None")]
    public void ConfigureOpenTelemetry_WithLogLevelNone_DoesNotCreateAzureSdkEventSourceLogForwarder(string logLevel)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<ServiceStartOptions>()
            .Configure(options =>
            {
                options.LogLevel = logLevel;
            });
        services.AddLogging();

        // Act
        services.ConfigureOpenTelemetry();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var forwarder = serviceProvider.GetService<AzureSdkEventSourceLogForwarder>();
        Assert.Null(forwarder);
    }
}
