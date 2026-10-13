// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// Service interface for Azure Monitor metrics operations
/// </summary>
public interface IMonitorMetricsService
{
    /// <summary>
    /// Queries metrics for the specified resource
    /// </summary>
    /// <param name="subscription">The subscription ID</param>
    /// <param name="resourceGroup">The resource group name (optional)</param>
    /// <param name="resourceType">The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="metricNames">List of metric names to query</param>
    /// <param name="startTime">Optional start time for the query in ISO format</param>
    /// <param name="endTime">Optional end time for the query in ISO format</param>
    /// <param name="interval">Optional time interval for data points</param>
    /// <param name="aggregation">Optional aggregation type (Average, Maximum, Minimum, Total, Count)</param>
    /// <param name="filter">Optional OData filter to apply</param>
    /// <param name="metricNamespace">Required metric namespace</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metric results with time series data</returns>
    Task<List<MetricResult>> QueryMetricsAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string metricNamespace,
        IEnumerable<string> metricNames,
        string? startTime = null,
        string? endTime = null,
        string? interval = null,
        string? aggregation = null,
        string? filter = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists metric definitions for the specified resource
    /// </summary>
    /// <param name="subscription">The subscription ID</param>
    /// <param name="resourceGroup">The resource group name (optional)</param>
    /// <param name="resourceType">The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="metricNamespace">Optional metric namespace</param>
    /// <param name="searchString">Optional search string to filter metric definitions by name and description</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metric definitions</returns>
    Task<List<MetricDefinition>> ListMetricDefinitionsAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? metricNamespace = null,
        string? searchString = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists metric namespaces for the specified resource
    /// </summary>
    /// <param name="subscription">The subscription ID</param>
    /// <param name="resourceGroup">The resource group name (optional)</param>
    /// <param name="resourceType">The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="searchString">Optional search string to filter namespaces</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metric namespaces</returns>
    Task<List<MetricNamespace>> ListMetricNamespacesAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? searchString = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries metrics for multiple resources in a single batch request. All resources must belong to the same
    /// subscription, Azure region, and resource type.
    /// </summary>
    /// <param name="subscription">The subscription ID</param>
    /// <param name="resourceGroup">The resource group name (optional, applied to all resources)</param>
    /// <param name="resourceType">The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts', applied to all resources)</param>
    /// <param name="resources">The names or resource IDs of the resources to query metrics for</param>
    /// <param name="metricNamespace">Required metric namespace</param>
    /// <param name="metricNames">List of metric names to query</param>
    /// <param name="startTime">Optional start time for the query in ISO format</param>
    /// <param name="endTime">Optional end time for the query in ISO format</param>
    /// <param name="interval">Optional time interval for data points</param>
    /// <param name="aggregation">Optional comma-separated aggregation types (Average, Maximum, Minimum, Total, Count)</param>
    /// <param name="filter">Optional OData filter to apply</param>
    /// <param name="orderBy">Optional sort order, only valid when <paramref name="filter"/> is specified</param>
    /// <param name="top">Optional maximum number of time series to retrieve per resource per metric, only valid when <paramref name="filter"/> is specified</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metric results per resource, with time series data</returns>
    Task<List<ResourceMetricsResult>> QueryMetricsBatchAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        IEnumerable<string> resources,
        string metricNamespace,
        IEnumerable<string> metricNames,
        string? startTime = null,
        string? endTime = null,
        string? interval = null,
        string? aggregation = null,
        string? filter = null,
        string? orderBy = null,
        int? top = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
