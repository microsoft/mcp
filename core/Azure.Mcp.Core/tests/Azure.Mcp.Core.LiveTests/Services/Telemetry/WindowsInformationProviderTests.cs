// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.Services.Telemetry;

[SupportedOSPlatform("windows")]
public class WindowsInformationProviderTests
{
    [Fact]
    [Trait("Category", "Live")]
    public async Task GetOrCreateDeviceId_WorksCorrectly()
    {
        Assert.SkipUnless(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "Only supported on Windows.");

        // Arrange
        var _logger = Substitute.For<ILogger<WindowsMachineInformationProvider>>();
        var provider = new WindowsMachineInformationProvider(_logger);

        // Act
        var deviceId = await provider.GetOrCreateDeviceId();

        // Assert
        Assert.NotNull(deviceId);
        Assert.NotEmpty(deviceId);

        // Verify it's persisted by calling again
        var deviceId2 = await provider.GetOrCreateDeviceId();
        Assert.Equal(deviceId, deviceId2);
    }
}
