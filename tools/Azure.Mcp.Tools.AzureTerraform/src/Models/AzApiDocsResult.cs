// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed class AzApiDocsResult
{
    public string ResourceType { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public string ParentResourceType { get; set; } = string.Empty;
    public string WritableScopes { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<AzApiExample>? Examples { get; set; }
}
