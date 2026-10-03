// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

internal static class AttachmentPolicy
{
    internal const int MaxFilesPerRequest = 10;
    internal const int MaxRawBytesPerFile = 1434 * 1024;
    internal const int MaxAggregateRawBytes = MaxFilesPerRequest * MaxRawBytesPerFile;

    // Supports four simultaneous maximum-size request batches in one local stdio process.
    internal const int MaxCachedFiles = MaxFilesPerRequest * 4;
    internal const long MaxCachedRawBytes = (long)MaxAggregateRawBytes * 4;
    internal static readonly TimeSpan MaximumLifetime = TimeSpan.FromHours(1);

    internal static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".zip",
            ".csv",
            ".json",
            ".yaml",
            ".tf",
            ".bicep",
            ".md",
            ".docx",
            ".pdf",
            ".vsdx",
            ".jpeg",
            ".png",
            ".svg",
        };
}
