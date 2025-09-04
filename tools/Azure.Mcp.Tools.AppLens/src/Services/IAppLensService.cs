// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
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
    /// <param name="resourceName">The name of the Azure resource to diagnose.</param>
    /// <param name="subscriptionNameOrId">Optional subscription name or ID for disambiguation.</param>
    /// <param name="resourceGroup">Optional resource group for disambiguation.</param>
    /// <param name="resourceType">Optional resource type for disambiguation.</param>
    /// <param name="retryPolicy">Optional retry policy options.</param>
    /// <returns>A diagnostic result containing insights and solutions.</returns>
    Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resourceName,
        string? subscriptionNameOrId = null,
        string? resourceGroup = null,
        string? resourceType = null,
        RetryPolicyOptions? retryPolicy = null);
}
