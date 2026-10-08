// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Models;
using Azure.ResourceManager.StorageSync.Models;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Models;

public class ServerEndpointSyncActivityStatusSchemaTests
{
    [Fact]
    public void FromSdkObject_MapsSyncProgressFields()
    {
        var activity = ArmStorageSyncModelFactory.ServerEndpointSyncActivityStatus(
            remainingFileCount: 11,
            remainingDirectoryCount: 12,
            remainingDeleteCount: 13,
            remainingLogicalSizeBytes: 14,
            isRemainingFinal: true,
            recentItemsPerSecond: 15.5,
            recentMegabytesPerSecond: 16.5,
            inProgressLargeFilePath: @"C:\large-file.bin",
            inProgressLargeFileSizeBytes: 17,
            inProgressLargeFilePercentComplete: 18,
            warning: ServerEndpointSyncSessionWarningType.BlockedByLargeFile);

        var result = ServerEndpointSyncActivityStatusSchema.FromSdkObject(activity);

        Assert.NotNull(result);
        Assert.Equal(11, result.RemainingFileCount);
        Assert.Equal(12, result.RemainingDirectoryCount);
        Assert.Equal(13, result.RemainingDeleteCount);
        Assert.Equal(14, result.RemainingLogicalSizeBytes);
        Assert.True(result.IsRemainingFinal);
        Assert.Equal(15.5, result.RecentItemsPerSecond);
        Assert.Equal(16.5, result.RecentMegabytesPerSecond);
        Assert.Equal(@"C:\large-file.bin", result.InProgressLargeFilePath);
        Assert.Equal(17, result.InProgressLargeFileSizeBytes);
        Assert.Equal(18, result.InProgressLargeFilePercentComplete);
        Assert.Equal("BlockedByLargeFile", result.Warning);
    }
}
