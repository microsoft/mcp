// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.StorageSyncService;

/// <summary>
/// Unit tests for StorageSyncServiceCreateCommand.
/// </summary>
public class StorageSyncServiceCreateCommandTests : CommandUnitTestsBase<StorageSyncServiceCreateCommand, IStorageSyncService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create Storage Sync Service", Command.Title);
    }
}

