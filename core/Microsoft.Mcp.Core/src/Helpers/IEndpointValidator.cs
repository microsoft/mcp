// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Authorizes endpoint URLs using the host's <see cref="SsrfProtectionPolicy"/>
/// and the currently executing command's registered namespace.
/// </summary>
/// <remarks>
/// Resolve <see cref="CommandContext.ToolNamespaceName"/> through
/// <see cref="ICommandContextAccessor.CurrentContext"/> on every validation call.
/// Never capture an invocation's namespace or override decision when constructing a validator or client.
/// A missing context or a <see langword="null"/>, empty, or whitespace namespace keeps protection enabled,
/// including when <see cref="SsrfProtectionPolicy.AllNamespaces"/> is configured.
/// Proxy routing does not disable endpoint domain validation.
/// </remarks>
public interface IEndpointValidator
{
    /// <summary>
    /// Validates that an endpoint belongs to an allowed Azure service domain for the specified cloud.
    /// </summary>
    /// <param name="endpoint">The completed absolute URL to authorize before network-capable work.</param>
    /// <param name="serviceType">
    /// The endpoint allow-list key, such as <c>storage-blob</c> or <c>keyvault</c>.
    /// This is independent of <see cref="CommandContext.ToolNamespaceName"/>.
    /// </param>
    /// <param name="armEnvironment">The configured Azure cloud against which the endpoint is authorized.</param>
    /// <exception cref="ArgumentException">
    /// Protection is enabled and <paramref name="endpoint"/> is <see langword="null"/>, empty, or whitespace,
    /// or <paramref name="serviceType"/> does not identify a configured service allow-list.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Protection is enabled and <paramref name="endpoint"/> is malformed, is not HTTPS,
    /// or its host is outside the service allow-list for <paramref name="armEnvironment"/>.
    /// </exception>
    void ValidateAzureServiceEndpoint(string endpoint, string serviceType, ArmEnvironment armEnvironment);

    /// <summary>
    /// Validates a deliberately arbitrary public HTTP or HTTPS target, including its resolved IP addresses.
    /// </summary>
    /// <param name="url">The completed target URL to authorize before network-capable work.</param>
    /// <exception cref="ArgumentException">
    /// Protection is enabled and <paramref name="url"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Protection is enabled and the URL or resolved destination is unsafe or unresolvable.
    /// </exception>
    void ValidatePublicTargetUrl(string url);
}
