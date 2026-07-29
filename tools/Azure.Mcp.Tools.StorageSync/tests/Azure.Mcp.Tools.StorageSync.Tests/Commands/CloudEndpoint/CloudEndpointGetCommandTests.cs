// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.CloudEndpoint;

public class CloudEndpointGetCommandTests : SubscriptionCommandUnitTestsBase<CloudEndpointGetCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.Equal("get", Command.Name);
    }
}


