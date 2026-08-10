// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Storage.Services;

public interface IStorageIntelligenceService
{
    Task<JsonElement> DiagnoseDiskAsync(
        string? resourceId,
        string? subscription = null,
        string? resourceGroup = null,
        string? vm = null,
        string[]? diskNames = null,
        string? startTime = null,
        string? endTime = null,
        bool includeHostLatency = false,
        CancellationToken cancellationToken = default);
}
