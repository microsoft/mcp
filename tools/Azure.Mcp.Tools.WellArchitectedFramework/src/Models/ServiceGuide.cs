// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Models;

internal sealed class ServiceGuide
{
    public required string[] ServiceNameVariationsNormalized { get; set; }
    public required string ServiceGuideUrl { get; set; }
}
