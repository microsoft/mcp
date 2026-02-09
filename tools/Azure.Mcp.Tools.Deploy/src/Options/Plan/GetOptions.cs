// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Plan;

public sealed class GetOptions : SubscriptionOptions
{
    [JsonPropertyName("workspaceFolder")]
    public string WorkspaceFolder { get; set; } = string.Empty;

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("targetAppService")]
    public string TargetAppService { get; set; } = string.Empty;

    [JsonPropertyName("provisioningTool")]
    public string ProvisioningTool { get; set; } = string.Empty;

    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("deployOption")]
    public string DeployOption { get; set; } = string.Empty;

    [JsonPropertyName("iacOptions")]
    public string? IacOptions { get; set; } = string.Empty;

    [JsonPropertyName("resourceGroupName")]
    public string? ResourceGroupName { get; set; } = string.Empty;
}
