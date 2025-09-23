// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Services.Models;

/// <summary> Properties of Cognitive Services account deployment. </summary>
internal sealed class CognitiveServicesAccountDeploymentProperties
{
    /// <summary> Properties of Cognitive Services account deployment model. </summary>
    public CognitiveServicesAccountDeploymentModel? Model { get; set; }
    /// <summary> Properties of Cognitive Services account deployment model. (Deprecated, please use Deployment.sku instead.). </summary>
    public CognitiveServicesAccountDeploymentScaleSettings? ScaleSettings { get; set; }
}
