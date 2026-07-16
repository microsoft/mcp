// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class DefaultMachineInformationProviderTests
{
    [Fact]
    public async Task ReturnsNullDeviceId()
    {
        var logger = Substitute.For<ILogger<DefaultMachineInformationProvider>>();
        var provider = new DefaultMachineInformationProvider(logger);

        var result = await provider.GetOrCreateDeviceId();

        Assert.Null(result);
    }
}
