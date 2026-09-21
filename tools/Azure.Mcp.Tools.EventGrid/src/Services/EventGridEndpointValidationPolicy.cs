// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.EventGrid.Services;

/// <summary>
/// Validates each final Azure EventGrid request immediately before network transport.
/// </summary>
internal sealed class EventGridEndpointValidationPolicy(ArmEnvironment armEnvironment) : HttpPipelinePolicy
{
    private readonly ArmEnvironment _armEnvironment = armEnvironment;

    public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        ValidateRequestUri(message.Request.Uri.ToUri(), _armEnvironment);
        return ProcessNextAsync(message, pipeline);
    }

    public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        ValidateRequestUri(message.Request.Uri.ToUri(), _armEnvironment);
        ProcessNext(message, pipeline);
    }

    /// <summary>
    /// Validates the final URI produced by the generated Azure EventGrid client.
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
    /// Thrown when the request URI is not an allowed Azure EventGrid endpoint for the configured cloud.
    /// </exception>
    private static void ValidateRequestUri(Uri? requestUri, ArmEnvironment armEnvironment)
        => EventGridService.ValidateEventGridEndpoint(requestUri, armEnvironment);
}
