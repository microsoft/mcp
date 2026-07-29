// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.StorageSyncService;

/// <summary>
/// Unit tests for StorageSyncServiceCreateCommand.
/// </summary>
public class StorageSyncServiceCreateCommandTests : SubscriptionCommandUnitTestsBase<StorageSyncServiceCreateCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create Storage Sync Service", Command.Title);
    }
}

