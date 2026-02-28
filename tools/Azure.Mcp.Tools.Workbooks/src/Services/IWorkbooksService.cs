// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Models;

namespace Azure.Mcp.Tools.Workbooks.Services;

public interface IWorkbooksService
{
    /// <summary>
    /// Lists workbooks using Azure Resource Graph with optional scope filtering.
    /// When subscriptions and resourceGroups are null, returns all accessible workbooks.
    /// </summary>
    Task<WorkbookListResult> ListWorkbooksAsync(
        IReadOnlyList<string>? subscriptions = null,
        IReadOnlyList<string>? resourceGroups = null,
        WorkbookFilters? filters = null,
        int maxResults = 50,
        bool includeTotalCount = true,
        OutputFormat outputFormat = OutputFormat.Standard,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets full workbook details for one or more workbooks via ARM API.
    /// Supports batch operations with partial success handling.
    /// </summary>
    Task<WorkbookBatchResult> GetWorkbooksAsync(
        IReadOnlyList<string> workbookIds,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workbook in the specified resource group.
    /// </summary>
    Task<WorkbookInfo?> CreateWorkbookAsync(
        string subscription,
        string resourceGroupName,
        string displayName,
        string serializedData,
        string sourceId,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workbook.
    /// </summary>
    Task<WorkbookInfo?> UpdateWorkbookAsync(
        string workbookId,
        string? displayName = null,
        string? serializedContent = null,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes one or more workbooks with partial success handling.
    /// </summary>
    Task<WorkbookDeleteBatchResult> DeleteWorkbooksAsync(
        IReadOnlyList<string> workbookIds,
        RetryPolicyOptions? retryPolicy = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
