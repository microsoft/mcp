// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record UnprotectedResourceInfo(
    string? Id,
    string? Name,
    string? ResourceType,
    string? ResourceGroup,
    string? Location,
    IDictionary<string, string>? Tags);
