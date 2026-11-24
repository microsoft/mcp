// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Exposes an <see cref="ArmClient"/>'s configured pipeline (authentication, ARM endpoint validation, retries and
/// test recording) for raw ARM requests whose responses the SDK models cannot deserialize.
/// </summary>
internal sealed class ArmPipelineResource(ArmClient client, ResourceIdentifier id) : ArmResource(client, id)
{
    internal HttpPipeline HttpPipeline => Pipeline;

    internal Uri BaseUri => Endpoint;
}
