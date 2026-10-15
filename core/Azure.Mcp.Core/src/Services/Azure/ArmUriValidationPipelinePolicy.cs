// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Validates the final SDK ARM request before transport and recording-proxy rewriting.
/// </summary>
/// <param name="armEnvironment">The configured Azure cloud against which ARM endpoints are validated.</param>
/// <param name="endpointValidator">The host's validator shared with explicit Azure-service endpoint checks.</param>
/// <remarks>
/// Add at <see cref="HttpPipelinePosition.BeforeTransport"/> to inspect the final request URI
/// on each transport attempt, including retries, paging, and long-running-operation polling.
/// Validation occurs before HTTP handlers rewrite valid requests to a recording proxy.
/// <para>
/// This policy can live as long as a cached ARM client: it stores <see cref="IEndpointValidator"/>, not an invocation's
/// namespace. Every send validates, including direct CLI and background work using this pipeline.
/// The validator resolves the current command context on every call.
/// A missing context or unresolved namespace cannot use a namespace-scoped bypass,
/// including <see cref="SsrfProtectionPolicy.AllNamespaces"/>. Independently constructed ARM clients and
/// raw HTTP requests without this policy are outside its scope.
/// </para>
/// </remarks>
internal sealed class ArmUriValidationPipelinePolicy(
    ArmEnvironment armEnvironment,
    IEndpointValidator endpointValidator) : HttpPipelineSynchronousPolicy
{
    /// <summary>
    /// Validates this attempt's ARM URI using the current command's original tool namespace.
    /// </summary>
    /// <param name="message">The pipeline message containing the final request to send.</param>
    /// <remarks>
    /// Called for synchronous and asynchronous sends by <see cref="HttpPipelineSynchronousPolicy"/>.
    /// Uses endpoint service type <c>arm</c> independently of the command's tool namespace.
    /// Validation exceptions propagate to command error handling without sending the denied request.
    /// </remarks>
    /// <exception cref="System.Security.SecurityException">
    /// The endpoint is rejected by ARM validation and no applicable namespace bypass is configured.
    /// </exception>
    public override void OnSendingRequest(HttpMessage message)
    {
        endpointValidator.ValidateAzureServiceEndpoint(
            endpoint: message.Request.Uri.ToUri().AbsoluteUri,
            serviceType: "arm",
            armEnvironment: armEnvironment);
    }
}
