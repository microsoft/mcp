// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Commands.StorageSyncService;

/// <summary>
/// Unit tests for StorageSyncServiceCreateCommand.
/// </summary>
public class StorageSyncServiceCreateCommandTests : SubscriptionCommandUnitTestsBase<StorageSyncServiceCreateCommand, IStorageSyncService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--enable-public-network-access true", true)]
    public async Task ExecuteAsync_PublicAccessRequiresOptIn(string options, bool publicAccess)
    {
        var response = await ExecuteCommandAsync($"--subscription sub --resource-group rg --name test-service --location eastus {options}");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
        Assert.Equal(publicAccess, Assert.Single(Service.ReceivedCalls()).GetArguments()[6]);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.Equal("create", Command.Name);
        Assert.Equal("Create Storage Sync Service", Command.Title);
    }
}

