// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Models;

public class AiResourceInformation
{
    public string? ResourceName { get; set; }
    public string? ResourceGroup { get; set; }
    public string? SubscriptionName { get; set; }
    public string? Location { get; set; }
    public string? Endpoint { get; set; }
    public string? Kind { get; set; }
    public string? SkuName { get; set; }
    public List<DeploymentInformation>? Deployments { get; set; }
}

public class DeploymentInformation
{
    public string? DeploymentName { get; set; }
    public string? ModelName { get; set; }
    public string? ModelVersion { get; set; }
    public string? ModelFormat { get; set; }
    public string? SkuName { get; set; }
    public int? SkuCapacity { get; set; }
    public string? ScaleType { get; set; }
    public string? ProvisioningState { get; set; }
}
