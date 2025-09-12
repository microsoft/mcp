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
    /// </summary>
    /// <param name="question">The diagnostic question from the user.</param>
    /// <param name="resource">The name of the Azure resource to diagnose.</param>
    /// <param name="subscription">The subscription of the Azure resource to diagnose.</param>
    /// <param name="resourceGroup">The resource group of the Azure resource to diagnose.</param>
    /// <param name="resourceType">The resource type of the Azure resource to diagnose.</param>
    /// <returns>A diagnostic result containing insights and solutions.</returns>
    Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resource,
        string subscription,
        string? resourceGroup = null,
        string? resourceType = null,
        string? tenantId = null);
}
