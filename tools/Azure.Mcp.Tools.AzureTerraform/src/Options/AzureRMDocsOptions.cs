// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AzureRMDocsOptions
{
    public string? ResourceType { get; set; }
    public string? DocType { get; set; }
    public string? ArgumentName { get; set; }
    public string? AttributeName { get; set; }
}
