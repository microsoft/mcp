// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync.Models;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Upload or download activity details for a server endpoint sync session.
/// </summary>
public sealed record ServerEndpointSyncActivityStatusSchema(
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp = null,
    [property: JsonPropertyName("perItemErrorCount")] long? PerItemErrorCount = null,
    [property: JsonPropertyName("appliedItemCount")] long? AppliedItemCount = null,
    [property: JsonPropertyName("totalItemCount")] long? TotalItemCount = null,
    [property: JsonPropertyName("appliedBytes")] long? AppliedBytes = null,
    [property: JsonPropertyName("totalBytes")] long? TotalBytes = null,
    [property: JsonPropertyName("syncMode")] string? SyncMode = null,
    [property: JsonPropertyName("sessionMinutesRemaining")] int? SessionMinutesRemaining = null,
    [property: JsonPropertyName("remainingFileCount")] long? RemainingFileCount = null,
    [property: JsonPropertyName("remainingDirectoryCount")] long? RemainingDirectoryCount = null,
    [property: JsonPropertyName("remainingDeleteCount")] long? RemainingDeleteCount = null,
    [property: JsonPropertyName("remainingLogicalSizeBytes")] long? RemainingLogicalSizeBytes = null,
    [property: JsonPropertyName("isRemainingFinal")] bool? IsRemainingFinal = null,
    [property: JsonPropertyName("recentItemsPerSecond")] double? RecentItemsPerSecond = null,
    [property: JsonPropertyName("recentMegabytesPerSecond")] double? RecentMegabytesPerSecond = null,
    [property: JsonPropertyName("inProgressLargeFilePath")] string? InProgressLargeFilePath = null,
    [property: JsonPropertyName("inProgressLargeFileSizeBytes")] long? InProgressLargeFileSizeBytes = null,
    [property: JsonPropertyName("inProgressLargeFilePercentComplete")] int? InProgressLargeFilePercentComplete = null,
    [property: JsonPropertyName("warning")] string? Warning = null)
{
    public static ServerEndpointSyncActivityStatusSchema? FromSdkObject(ServerEndpointSyncActivityStatus? activity)
    {
        if (activity == null)
        {
            return null;
        }

        return new(
            activity.Timestamp,
            activity.PerItemErrorCount,
            activity.AppliedItemCount,
            activity.TotalItemCount,
            activity.AppliedBytes,
            activity.TotalBytes,
            activity.SyncMode?.ToString(),
            activity.SessionMinutesRemaining,
            activity.RemainingFileCount,
            activity.RemainingDirectoryCount,
            activity.RemainingDeleteCount,
            activity.RemainingLogicalSizeBytes,
            activity.IsRemainingFinal,
            activity.RecentItemsPerSecond,
            activity.RecentMegabytesPerSecond,
            activity.InProgressLargeFilePath,
            activity.InProgressLargeFileSizeBytes,
            activity.InProgressLargeFilePercentComplete,
            activity.Warning?.ToString());
    }
}
