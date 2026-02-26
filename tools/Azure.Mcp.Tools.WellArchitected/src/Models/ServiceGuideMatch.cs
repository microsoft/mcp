// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class ServiceGuideMatch
{
    public string Service { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
