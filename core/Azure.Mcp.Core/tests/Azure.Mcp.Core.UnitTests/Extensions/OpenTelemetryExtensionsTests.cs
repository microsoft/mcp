// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class OpenTelemetryExtensionsTests
{
    /// <summary>
    /// ConfigureOpenTelemetry registers the telemetry services.
    /// Note: ConfigureOpenTelemetry does NOT configure IsTelemetryEnabled based on SupportLoggingFolder -
    /// that logic is handled by ServiceCollectionExtensions.InitializeConfigurationAndOptions.
    /// For tests that verify SupportLoggingFolder -> IsTelemetryEnabled behavior,
    /// see ServiceCollectionExtensionsSerializedTests.
    /// </summary>
    [Fact]
    public void ConfigureOpenTelemetry_RegistersTelemetryService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureOpenTelemetry();

        // Assert - Verify that the telemetry service descriptor is registered
        Assert.Contains(services, sd => sd.ServiceType == typeof(ITelemetryService));
    }
}
