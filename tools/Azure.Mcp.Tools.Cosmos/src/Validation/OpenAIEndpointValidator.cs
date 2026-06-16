// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Validation;

/// <summary>
/// Validates the caller-supplied Azure OpenAI endpoint passed to an <c>AzureOpenAIClient</c>.
/// A valid endpoint must be an HTTPS URI on a trusted Azure first-party host.
/// </summary>
internal static class OpenAIEndpointValidator
{
    /// <summary>
    /// Trusted Azure first-party host suffixes for Azure OpenAI and Azure AI / Cognitive Services
    /// endpoints across the public, US Government, and China sovereign clouds. Each entry begins with a
    /// leading dot so a resource subdomain is required and look-alike hosts such as
    /// <c>contoso.openai.azure.com.other.com</c> are rejected.
    /// </summary>
    internal static readonly string[] AllowedHostSuffixes =
    [
        // Public cloud
        ".openai.azure.com",
        ".cognitiveservices.azure.com",
        ".services.ai.azure.com",
        // US Government
        ".openai.azure.us",
        ".cognitiveservices.azure.us",
        // China
        ".openai.azure.cn",
        ".cognitiveservices.azure.cn",
    ];

    /// <summary>
    /// Returns <c>true</c> when the value is an absolute HTTPS URI whose host is a trusted Azure OpenAI
    /// or Cognitive Services endpoint. When invalid, <paramref name="error"/> contains a caller-facing
    /// message explaining why.
    /// </summary>
    public static bool IsValid(string? value, out string? error)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            error = "An Open AI endpoint is required.";
            return false;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            error = $"The provided Open AI endpoint must be a valid absolute URI. Received: '{value}'.";
            return false;
        }

        if (!uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            error = "The provided Open AI endpoint must use the HTTPS scheme.";
            return false;
        }

        var host = uri.Host;
        foreach (var suffix in AllowedHostSuffixes)
        {
            // Require a non-empty resource label before the suffix (host.Length > suffix.Length),
            // which also blocks bare apex hosts like "openai.azure.com".
            if (host.Length > suffix.Length && host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                error = null;
                return true;
            }
        }

        error = $"The provided Open AI endpoint must be a trusted Azure OpenAI or Cognitive Services endpoint " +
            $"(e.g., https://<resource>.openai.azure.com/). The host '{host}' is not allowed.";
        return false;
    }
}
