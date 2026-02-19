// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Telemetry;

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
