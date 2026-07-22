// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Options;

/// <summary>
/// Configuration options for Azure Functions toolset services.
/// </summary>
public sealed class FunctionsOptions
{
    /// <summary>
    /// Gets or sets the primary URL for the Azure Functions templates manifest.
    /// Defaults to the production CDN URL.
    /// </summary>
    public string ManifestUrl { get; set; } = "https://cdn.functions.azure.com/public/templates-manifest/manifest.json";

    /// <summary>
    /// Gets or sets the fallback URL for the Azure Functions templates manifest.
    /// Used when the primary CDN is unavailable. Defaults to the GitHub raw URL.
    /// </summary>
    public string FallbackManifestUrl { get; set; } = "https://raw.githubusercontent.com/Azure/azure-functions-templates/dev/Functions.Templates/Template-Manifest/manifest.json";
}
