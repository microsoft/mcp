// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class AzureRMDocsResult
{
    public string ResourceType { get; set; } = string.Empty;
    public string DocumentationUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<ArgumentDetail> Arguments { get; set; } = [];
    public List<AttributeDetail> Attributes { get; set; } = [];
    public List<string> Examples { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
