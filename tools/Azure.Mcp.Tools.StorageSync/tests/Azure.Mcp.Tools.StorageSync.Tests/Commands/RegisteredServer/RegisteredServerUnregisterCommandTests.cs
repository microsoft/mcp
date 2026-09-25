// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Services;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.RegisteredServer;

public class RegisteredServerUnregisterCommandTests : SubscriptionCommandUnitTestsBase<RegisteredServerUnregisterCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("unregister", CommandDefinition.Name);
        Assert.Equal("unregister", Command.Name);
    }
}


