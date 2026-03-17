// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Options;

public sealed class TemplateGetOptions
{
    public string? Language { get; set; }
    public string? Template { get; set; }
    public string? RuntimeVersion { get; set; }
}
