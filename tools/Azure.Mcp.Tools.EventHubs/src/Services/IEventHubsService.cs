// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Services;

public interface IEventHubsService
{
    Task<List<Namespace>> GetNamespacesAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Namespace> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Namespace> UpdateNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? location = null,
        string? skuName = null,
        string? skuTier = null,
        int? skuCapacity = null,
        bool? isAutoInflateEnabled = null,
        int? maximumThroughputUnits = null,
        bool? kafkaEnabled = null,
        bool? zoneRedundant = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<bool> DeleteNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<EventHubInfo>> ListEventHubsAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<EventHubInfo?> GetEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<EventHubInfo> CreateOrUpdateEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        int? partitionCount = null,
        long? messageRetentionInHours = null,
        string? status = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<bool> DeleteEventHubAsync(
        string eventHubName,
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
