// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Resource;
using Microsoft.Mcp.Core.Models.ResourceGroup;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Security.AntiSSRF;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Provides operations for basic Azure concepts such as tenants, subscriptions, resource groups, generic resources, and access.
/// </summary>
public interface IAzureService
{
    #region General

    /// <summary>
    /// Gets the Azure cloud configuration for the current environment.
    /// </summary>
    IAzureCloudConfiguration CloudConfiguration { get; }

    /// <summary>
    /// Validates an Azure endpoint through the host's injected <see cref="IEndpointValidator"/>.
    /// </summary>
    /// <param name="endpoint">The completed absolute URL to authorize before network-capable work.</param>
    /// <param name="serviceType">
    /// The Azure endpoint allow-list key, which is independent of the executing command's registered namespace.
    /// </param>
    /// <remarks>
    /// Validates against <see cref="IAzureCloudConfiguration.ArmEnvironment"/> from <see cref="CloudConfiguration"/>;
    /// callers cannot override the configured cloud.
    /// Uses the same immutable <see cref="SsrfProtectionPolicy"/> as shared HTTP transports
    /// and namespace override startup telemetry.
    /// The validator resolves the executing namespace through
    /// <see cref="ICommandContextAccessor.CurrentContext"/> on every call.
    /// A missing context or unresolved namespace cannot enable an override,
    /// including <see cref="SsrfProtectionPolicy.AllNamespaces"/>.
    /// Validation exceptions propagate unchanged. Proxy routing does not exempt endpoint domain checks.
    /// </remarks>
    /// <exception cref="ArgumentException">The endpoint is empty or the service allow-list is unavailable.</exception>
    /// <exception cref="SecurityException">The endpoint's format, scheme, or host is rejected.</exception>
    void ValidateAzureServiceEndpoint(
        string endpoint,
        string serviceType);

    /// <summary>
    /// Validates a deliberately arbitrary public target using the host's <see cref="SsrfProtectionPolicy"/>.
    /// </summary>
    /// <param name="url">The completed HTTP or HTTPS target URL.</param>
    /// <remarks>
    /// Uses the host's <see cref="IEndpointValidator"/> and its current command context.
    /// A missing context or unresolved namespace keeps protection enabled,
    /// including when <see cref="SsrfProtectionPolicy.AllNamespaces"/> is configured.
    /// </remarks>
    /// <exception cref="ArgumentException">The URL is empty.</exception>
    /// <exception cref="SecurityException">The URL or resolved destination is unsafe or unresolvable.</exception>
    void ValidatePublicTargetUrl(string url);

    /// <summary>
    /// Configures shared ARM client options with the cloud, HTTP transport, and command-aware endpoint policy.
    /// </summary>
    /// <param name="armClientOptions">
    /// The mutable options to configure before constructing an <see cref="ArmClient"/>.
    /// </param>
    /// <remarks>
    /// Called by <see cref="AzureHelper.CreateArmClientAsync"/> during shared ARM client creation.
    /// Replaces the transport and environment with the configured cloud and a factory-created
    /// ARM transport, while preserving other caller-supplied options. Configure once per
    /// options instance before constructing the client, not on each request from a cached client.
    /// <para>
    /// Adds endpoint validation at <see cref="HttpPipelinePosition.BeforeTransport"/>.
    /// The policy's <see cref="IEndpointValidator"/> reads
    /// <see cref="ICommandContextAccessor.CurrentContext"/>
    /// on every send, including retries, paging, and long-running-operation polling. It does not
    /// capture the namespace present when the client is created.
    /// </para>
    /// <para>
    /// Every send validates, even without an active command execution scope. A missing context
    /// or <see langword="null"/> namespace cannot enable a namespace
    /// bypass, including <see cref="SsrfProtectionPolicy.AllNamespaces"/>.
    /// Server registration disables automatic redirects for the dedicated ARM transport.
    /// Independently, its HTTP transport applies external-only
    /// AntiSSRF checks, except for configured proxies or the current namespace's explicit override.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="armClientOptions"/> is <see langword="null"/>.
    /// </exception>
    void ConfigureArmClientOptions(ArmClientOptions armClientOptions);

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
    /// <item><description>Automatic redirects disabled</description></item>
    /// <item><description>
    /// <see cref="PolicyConfigOptions.ExternalOnlyLatest"/> AntiSSRF protection,
    /// unless overridden by proxy configuration or the executing namespace.
    /// </description></item>
    /// </list>
    /// <para>Do:</para>
    /// <list type="bullet">
    /// <item><description>Utilize the client for a single method or MCP tool invocation.</description></item>
    /// <item><description>Add request-specific configuration that is scoped to the current operation.</description></item>
    /// </list>
    /// <para>Don't:</para>
    /// <list type="bullet">
    /// <item><description>Cache a namespace override decision in a reusable SDK client.</description></item>
    /// </list>
    /// <para>
    /// Namespace overrides are evaluated on each send using the original registered command
    /// namespace, including when SDK clients are reused. Without an active context, or when
    /// its namespace is unresolved, no namespace override applies.
    /// </para>
    /// <para>
    /// Configured HTTP and debug recording proxies take precedence over transport protection.
    /// This exception does not disable explicit <see cref="IEndpointValidator"/> domain checks.
    /// </para>
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
    /// Thrown when the tenant has a <see langword="null"/> <see cref="TenantData.TenantId"/>.
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
    /// Thrown when the tenant has a <see langword="null"/> <see cref="TenantData.TenantId"/>.
    /// </exception>
    Task<string> GetTenantIdByName(string tenantName, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the tenant ID from a given tenant name or ID. If the input is already a valid tenant GUID, it is returned as-is.
    /// If the input is a tenant name, the corresponding tenant ID is retrieved.
    /// </summary>
    /// <param name="tenant">The tenant ID or name to resolve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing the resolved tenant ID.
    /// </returns>
    Task<string?> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken);

    #endregion Tenant

    #region Subscription

    /// <summary>
    /// Gets the list of all available Azure subscriptions.
    /// </summary>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing a list of <see cref="SubscriptionData"/> for the found subscriptions.
    /// </returns>
    Task<List<SubscriptionData>> GetSubscriptions(
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific Azure subscription by its ID or name.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to get data for.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing the requested <see cref="SubscriptionResource"/>.
    /// </returns>
    Task<SubscriptionResource> GetSubscription(
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the provided subscription string is a valid GUID.
    /// <para>
    /// This method DOES NOT verify that the subscription exists in Azure, only that the string is a valid subscription ID format.
    /// </para>
    /// </summary>
    /// <param name="subscription">The subscription string to verify.</param>
    /// <returns>
    /// Whether <paramref name="subscription"/> is a <see cref="Guid"/> in the proper format
    /// for an Azure subscription ID; this does not establish that the subscription exists.
    /// </returns>
    bool IsSubscriptionId(string subscription);

    /// <summary>
    /// Gets the subscription ID for a given subscription name. If the subscription name is not found, an exception is thrown.
    /// </summary>
    /// <param name="subscriptionName">The subscription name to find the ID for.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing the found subscription ID.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when a subscription with the specified name is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple subscriptions with the specified name are found.</exception>
    Task<string> GetSubscriptionIdByName(
        string subscriptionName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default subscription ID from the Azure CLI profile (~/.azure/azureProfile.json).
    /// Falls back to the AZURE_SUBSCRIPTION_ID environment variable if the profile is unavailable.
    /// </summary>
    /// <returns>
    /// The default subscription ID if found; otherwise, <see langword="null"/>.
    /// </returns>
    string? GetDefaultSubscriptionId();

    #endregion Subscription

    #region Resource Group

    /// <summary>
    /// Gets the list of all resource groups for a given subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to list resource groups.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing a list of <see cref="ResourceGroupInfo"/> for <paramref name="subscriptionId"/>.
    /// </returns>
    Task<List<ResourceGroupInfo>> GetResourceGroups(
        string subscriptionId,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific resource group for a given subscription and resource group name.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resource group.</param>
    /// <param name="resourceGroupName">The resource group name to get.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing the requested <see cref="ResourceGroupInfo"/>, or <see langword="null"/> if not found.
    /// </returns>
    Task<ResourceGroupInfo?> GetResourceGroup(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    #endregion Resource Group

    #region Generic Resource

    /// <summary>
    /// Gets a specific resource group resource for a given subscription and resource group.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resource.</param>
    /// <param name="resourceGroupName">The resource group containing the resource.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The task producing the requested <see cref="ResourceGroupResource"/>, or <see langword="null"/> if not found.
    /// </returns>
    Task<ResourceGroupResource?> GetResourceGroupResource(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of all generic resources for a given subscription and resource group.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to get the resources.</param>
    /// <param name="resourceGroupName">The resource group containing the resources.</param>
    /// <param name="tenant">An optional tenant to scope the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// An asynchronous sequence of <see cref="GenericResourceInfo"/> for the requested subscription and resource group.
    /// </returns>
    IAsyncEnumerable<GenericResourceInfo> GetGenericResources(
        string subscriptionId,
        string resourceGroupName,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    #endregion Generic Resource
}
