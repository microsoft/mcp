// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Services.Models;

/// <summary> Properties of Cognitive Services account deployment model. (Deprecated, please use Deployment.sku instead.). </summary>
internal sealed class CognitiveServicesAccountDeploymentScaleSettings
{
    /// <summary> Deployment scale type. </summary>
    public string? ScaleType { get; set; }
    /// <summary> Deployment capacity. </summary>
    public int? Capacity { get; set; }
}
