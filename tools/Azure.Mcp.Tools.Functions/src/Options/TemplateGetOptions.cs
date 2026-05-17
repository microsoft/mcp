// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Options;

public sealed class TemplateGetOptions
{
    public string? Language { get; set; }
    public string? Template { get; set; }
    public string? RuntimeVersion { get; set; }
    public TemplateOutput Output { get; set; } = TemplateOutput.New;
}
