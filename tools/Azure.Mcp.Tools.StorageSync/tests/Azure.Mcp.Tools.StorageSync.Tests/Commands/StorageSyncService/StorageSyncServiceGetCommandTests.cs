// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.StorageSyncService;

public class StorageSyncServiceGetCommandTests : SubscriptionCommandUnitTestsBase<StorageSyncServiceGetCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.Equal("get", Command.Name);
    }
}

