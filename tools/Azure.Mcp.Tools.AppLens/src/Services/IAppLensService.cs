// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppLens.Models;

namespace Azure.Mcp.Tools.AppLens.Services;

/// <summary>
/// Service interface for AppLens diagnostic operations.
/// </summary>
public interface IAppLensService
{
    /// <summary>
    /// Diagnoses Azure resource issues using AppLens conversational diagnostics.
    /// Uses Azure Resource Graph to discover the resource when optional parameters are not provided.
    /// </summary>
    /// <param name="question">The diagnostic question from the user.</param>
    /// <param name="resource">The name of the Azure resource to diagnose.</param>
    /// <param name="subscription">Optional subscription to narrow down resource discovery.</param>
    /// <param name="resourceGroup">Optional resource group to narrow down resource discovery.</param>
    /// <param name="resourceType">Optional resource type to narrow down resource discovery.</param>
    /// <param name="tenantId">Optional tenant ID for authentication.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A diagnostic result containing insights and solutions.</returns>
    Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resource,
        string? subscription = null,
        string? resourceGroup = null,
        string? resourceType = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
}
