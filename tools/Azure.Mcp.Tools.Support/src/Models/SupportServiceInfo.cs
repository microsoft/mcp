// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Support.Models;

public sealed record SupportServiceInfo(
    string? Name,
    string? Id,
    string? DisplayName,
    List<string>? ResourceTypes);
