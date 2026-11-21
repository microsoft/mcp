// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Support.Models;

public sealed record SupportProblemClassification(
    string? Name,
    string? Id,
    string? DisplayName);
