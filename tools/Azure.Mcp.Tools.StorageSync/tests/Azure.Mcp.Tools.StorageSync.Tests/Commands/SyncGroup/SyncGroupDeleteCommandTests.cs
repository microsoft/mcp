// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.SyncGroup;

public class SyncGroupDeleteCommandTests : SubscriptionCommandUnitTestsBase<SyncGroupDeleteCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.Equal("delete", Command.Name);
    }
}


