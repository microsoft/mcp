// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Result containing file share usage data.
/// </summary>
public sealed record FileShareUsageDataResult(LiveSharesUsageData LiveShares);

/// <summary>
/// Usage data for live (active) file shares.
/// </summary>
public sealed record LiveSharesUsageData(int FileShareCount);
