// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Models.Resource;
using Microsoft.Mcp.Core.Models.ResourceGroup;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Provices operations for basic Azure concepts such as tenants, subscriptions, resource groups, generic resources, and access.
/// </summary>
public interface IAzureService
{
    #region General

    /// <summary>
    /// Gets the Azure cloud configuration for the current environment.
    /// </summary>
    IAzureCloudConfiguration CloudConfiguration { get; }

    /// <summary>
    /// Gets a new instance of <see cref="HttpClient"/> configured for use with Azure operations.
    /// </summary>
    /// <remarks>
    /// <para>Each instance includes the following configuration:</para>
    /// <list type="bullet">
    /// <item><description>Proxy settings</description></item>
    /// <item><description>Record/playback handler</description></item>
    /// <item><description>Timeout configuration</description></item>
    /// <item><description>User-Agent header</description></item>
    /// </list>
    /// <para>Do:</para>
    /// <list type="bullet">
    /// <item><description>Utilize the client for a single method or MCP tool invocation.</description></item>
    /// <item><description>Add request-specific configuration that is scoped to the current operation.</description></item>
    /// </list>
    /// <para>Don't:</para>
    /// <list type="bullet">
    /// <item><description>Persist the client beyond the lifetime of the invoking tool.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="name">The logical name of the client to create.</param>
    /// <returns>
    /// An <see cref="HttpClient"/> instance configured for use with Azure operations.
    /// </returns>
    HttpClient GetClient(string? name = null);

    /// <summary>
    /// Gets an instance of <see cref="TokenCredential"/>.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a value of <see cref="TokenCredential"/>.
    /// </returns>
    /// <remarks>
    /// Implementors of this method must use <see cref="IAzureTokenCredentialProvider"/> to obtain
    /// the token credential.
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation has been cancelled.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a credential cannot be provided.
    /// </exception>
    Task<TokenCredential> GetTokenCredentialAsync(
        string? tenantId,
        CancellationToken cancellationToken);

    #endregion General

    #region Tenant

    /// <summary>
    /// Gets the list of all available Azure tenants.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a list of <see cref="TenantResource"/>
    /// instances.
    /// </returns>
    Task<List<TenantResource>> GetTenants(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the tenant ID from either a tenant ID or tenant name.
    /// </summary>
    /// <param name="tenantIdOrName">The tenant ID or tenant name.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the tenant ID or <see langword="null"/>
    /// if not found.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a tenant with the specified name is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tenant has a <see langword="null"> TenantId.
    /// </exception>
    Task<string> GetTenantId(string tenantIdOrName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the tenant ID by tenant name.
    /// </summary>
    /// <param name="tenantName">The tenant name.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the tenant ID or <see langword="null"/>
    /// if not found.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a tenant with the specified name is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tenant has a <see langword="null"> TenantId.
    /// </exception>
    Task<string> GetTenantIdByName(string tenantName, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the tenant ID from a given tenant name or ID. If the input is already a valid tenant GUID, it is returned as-is.
    /// If the input is a tenant name, the corresponding tenant ID is retrieved.
    /// </summary>
    /// <param name="tenant">The tenant ID or name to resolve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The resolved tenant ID.</returns>
    Task<string?> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken);

    #endregion Tenant

    #region Subscription

    /// <summary>
    /// Gets the list of all available Azure subscriptions.
    /// </summary>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A list of subscription data representing the found subscriptions.</returns>
    Task<List<SubscriptionData>> GetSubscriptions(
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific Azure subscription by its ID or name.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to get data for.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Subscription data representing the specific subscription.</returns>
    Task<SubscriptionResource> GetSubscription(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the provided subscription string is a valid GUID.
    /// <para>
    /// This method DOES NOT verify that the subscription exists in Azure, only that the string is a valid subscription ID format.
    /// </para>
    /// </summary>
    /// <param name="subscription">The subscription string to verify.</param>
    /// <returns>Whether the subscription string is a GUID, therefore the proper format for an Azure subscription ID (not guaranteeed to be a real subscription).</returns>
    bool IsSubscriptionId(string subscription);

    /// <summary>
    /// Gets the subscription ID for a given subscription name. If the subscription name is not found, an exception is thrown.
    /// </summary>
    /// <param name="subscriptionName">The subscription name to find the ID for.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The found subscription ID.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when a subscription with the specified name is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple subscriptions with the specified name are found.</exception>
    Task<string> GetSubscriptionIdByName(
        string subscriptionName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default subscription ID from the Azure CLI profile (~/.azure/azureProfile.json).
    /// Falls back to the AZURE_SUBSCRIPTION_ID environment variable if the profile is unavailable.
    /// </summary>
    /// <returns>The default subscription ID if found, null otherwise.</returns>
    string? GetDefaultSubscriptionId();

    #endregion Subscription

    #region Resource Group

    /// <summary>
    /// Gets the list of all resource groups for a given subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to list resource groups.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A list of resource group information for the given subscription.</returns>
    Task<List<ResourceGroupInfo>> GetResourceGroups(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific resource group for a given subscription and resource group name.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resource group.</param>
    /// <param name="resourceGroupName">The resource group name to get.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The resource group information for the specific resource group in the subscription.</returns>
    Task<ResourceGroupInfo?> GetResourceGroup(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion Resource Group

    #region Generic Resource

    /// <summary>
    /// Gets a specific resource group resource for a given subscription and resource group.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resource.</param>
    /// <param name="resourceGroupName">The resource group containing the resource.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The resource information.</returns>
    Task<ResourceGroupResource?> GetResourceGroupResource(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of all generic resources for a given subscription and resource group.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resources.</param>
    /// <param name="resourceGroupName">The resource group containing the resources.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="retryPolicy">Retry options for the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns></returns>
    IAsyncEnumerable<GenericResourceInfo> GetGenericResources(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion Generic Resource
}
