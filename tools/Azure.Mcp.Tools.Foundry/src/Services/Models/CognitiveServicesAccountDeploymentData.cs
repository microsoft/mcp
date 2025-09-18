// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Services.Models;

/// <summary>
/// A class representing the CognitiveServicesAccountDeployment data model.
/// Cognitive Services account deployment.
/// </summary>
internal sealed class CognitiveServicesAccountDeploymentData
{
    /// <summary> The resource model definition representing SKU. </summary>
    public CognitiveServicesSku? Sku { get; set; }
    /// <summary> Properties of Cognitive Services account deployment. </summary>
    public CognitiveServicesAccountDeploymentProperties? Properties { get; set; }
}
