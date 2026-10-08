// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Owns one host's immutable emergency namespace overrides for endpoint and transport protection.
/// </summary>
/// <param name="dangerouslyDisabledNamespaces">
/// The startup-configured namespaces, or <see langword="null"/> for no overrides.
/// Values are copied and matched case-insensitively without trimming.
/// <see cref="AllNamespaces"/> matches every resolved executing namespace.
/// </param>
/// <remarks>
/// Register one instance per host, shared by <see cref="EndpointValidator"/>, transports, and startup telemetry.
/// Never mutate configuration or retain a request's namespace or bypass decision in this policy.
/// Independent hosts and test providers can safely use different policies concurrently.
/// </remarks>
public sealed class SsrfProtectionPolicy(IEnumerable<string>? dangerouslyDisabledNamespaces)
{
    /// <summary>
    /// The configured marker for overriding all resolved tool namespaces.
    /// </summary>
    public const string AllNamespaces = "ALL";

    private readonly FrozenSet<string> _disabledNamespaces = (dangerouslyDisabledNamespaces ?? [])
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the number of distinct, non-blank configured overrides for startup telemetry.
    /// </summary>
    internal int NamespaceOverrideCount => _disabledNamespaces.Count;

    /// <summary>
    /// Gets the bounded override scope without disclosing namespace values.
    /// </summary>
    internal string NamespaceOverrideScope => _disabledNamespaces.Count == 0 ? "none"
        : _disabledNamespaces.Contains(AllNamespaces) ? "all"
        : "selected";

    /// <summary>
    /// Determines whether this host's namespace policy keeps SSRF protection enabled.
    /// </summary>
    /// <param name="executingToolNamespaceName">
    /// The original registered tool namespace, not the endpoint service key or caller-provided routing name.
    /// A <see langword="null"/>, empty, or whitespace namespace cannot enable an override,
    /// including <see cref="AllNamespaces"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> unless <paramref name="executingToolNamespaceName"/> matches
    /// an emergency override. Missing or unresolved namespaces always keep protection enabled,
    /// including under <see cref="AllNamespaces"/>.
    /// </returns>
    /// <remarks>
    /// Evaluate on every request using the current command context. This does not account for
    /// transport-specific proxy exceptions, which do not disable endpoint domain validation.
    /// </remarks>
    public bool AreSsrfProtectionsEnabled(string? executingToolNamespaceName)
    {
        if (string.IsNullOrWhiteSpace(executingToolNamespaceName))
        {
            // Always enable SSRF protections when we don't know the
            // executing tool/command.
            return true;
        }

        // Enable SSRF protections 
        return !_disabledNamespaces.Contains(AllNamespaces) &&
            !_disabledNamespaces.Contains(executingToolNamespaceName);
    }
}
