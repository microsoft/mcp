// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Result containing file share usage data.
/// </summary>
public class FileShareUsageDataResult
{
    public LiveSharesUsageData LiveShares { get; set; } = new();
}

/// <summary>
/// Usage data for live (active) file shares.
/// </summary>
public class LiveSharesUsageData
{
    public int FileShareCount { get; set; }
}
