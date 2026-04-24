// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.SyncGroup;

public class SyncGroupDeleteCommandTests : CommandUnitTestsBase<SyncGroupDeleteCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.Equal("delete", Command.Name);
    }
}


