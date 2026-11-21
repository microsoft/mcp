// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Support.Options.Service;

public class ServiceListOptions
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
