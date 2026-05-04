// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.RegisteredServer;

public class RegisteredServerUpdateCommandTests : CommandUnitTestsBase<RegisteredServerUpdateCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", CommandDefinition.Name);
        Assert.Equal("update", Command.Name);
    }
}


