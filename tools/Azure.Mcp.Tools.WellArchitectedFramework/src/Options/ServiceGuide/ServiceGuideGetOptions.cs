// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;

public sealed class ServiceGuideGetOptions
{
    [Option(Description = WellArchitectedFrameworkOptionDescriptions.Service)]
    public string? Service { get; set; }
}
