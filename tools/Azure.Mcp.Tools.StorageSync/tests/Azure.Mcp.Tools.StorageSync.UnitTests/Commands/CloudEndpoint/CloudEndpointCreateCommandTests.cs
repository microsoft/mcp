// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.CloudEndpoint;

public class CloudEndpointCreateCommandTests : CommandUnitTestsBase<CloudEndpointCreateCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.Equal("create", Command.Name);
    }
}


