// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Pricing.Services;

/// <summary>
/// Validates each final Azure Retail Prices request immediately before network transport.
/// </summary>
internal sealed class RetailPricingEndpointValidationPolicy(ArmEnvironment armEnvironment) : PipelinePolicy
{
    private readonly ArmEnvironment _armEnvironment = armEnvironment;

    public override void Process(
        PipelineMessage message,
        IReadOnlyList<PipelinePolicy> pipeline,
        int currentIndex)
    {
        ValidateRequestUri(message.Request.Uri, _armEnvironment);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override ValueTask ProcessAsync(
        PipelineMessage message,
        IReadOnlyList<PipelinePolicy> pipeline,
        int currentIndex)
    {
        ValidateRequestUri(message.Request.Uri, _armEnvironment);
        return ProcessNextAsync(message, pipeline, currentIndex);
    }

    /// <summary>
    /// Validates the final URI produced by the generated Azure Retail Prices client.
    /// </summary>
    /// <param name="requestUri">The request URI that will be passed to network transport.</param>
    /// <param name="armEnvironment">The configured Azure cloud.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the generated pipeline does not provide a request URI.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the pricing endpoint allow-list is not configured.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    /// Thrown when the request URI is not an allowed Azure Retail Prices endpoint for the configured cloud.
    /// </exception>
    private static void ValidateRequestUri(Uri? requestUri, ArmEnvironment armEnvironment)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        // The generated client escapes MCP-controlled values as query parameters on the configured endpoint,
        // but pagination resets the complete URI from the service's NextPageLink. Validate the exact final URI
        // before transport so neither query construction nor a compromised continuation can replace the host.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: requestUri.AbsoluteUri,
            serviceType: "pricing",
            armEnvironment: armEnvironment,
            executingToolNamespaceName: "pricing");
    }
}
