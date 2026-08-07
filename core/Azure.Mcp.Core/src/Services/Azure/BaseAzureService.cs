// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Core.Services.Azure;

public abstract class BaseAzureService(IAzureService azureService)
{
    private static readonly TimeSpan? s_defaultPollInterval = null;

    static BaseAzureService()
    {
#if DEBUG
        if (EnvironmentHelpers.IsPlaybackTesting())
        {
            s_defaultPollInterval = TimeSpan.Zero;
        }
#endif
    }

    /// <summary>
    /// Initializes the user agent policy to include the transport type for all Azure service calls.
    /// This method must be called once during application startup before creating any <see cref="BaseAzureService"/> instances.
    /// Subsequent calls will be safely ignored to ensure the policy is initialized only once.
    /// </summary>
    /// <param name="transportType">The transport type (e.g., "stdio", "http"). Cannot be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="transportType"/> is null or empty.</exception>
    /// <remarks>
    /// The user agent string will be formatted as: azmcp/{version} azmcp-{transport}/{version} ({framework}; {platform})
    /// </remarks>
    public static void InitializeUserAgentPolicy(string transportType) => AzureHelper.InitializeUserAgentPolicy(transportType);

    /// <summary>
    /// Disables upper bounds enforcement on retry policy values (delays, timeouts, max retries).
    /// This method should be called once during application startup when the --dangerously-disable-retry-limits flag is set.
    /// </summary>
    public static void DisableRetryLimits() => AzureHelper.DisableRetryLimits();

    /// <summary>
    /// Resets the retry limits flag. For testing only.
    /// </summary>
    internal static void ResetRetryLimits() => AzureHelper.ResetRetryLimits();

    protected string UserAgent { get; } = AzureHelper.UserAgent;

    /// <summary>
    /// Gets the Azure service for interacting with Azure resources and obtaining credentials.
    /// </summary>
    protected IAzureService AzureService { get; } = azureService ?? throw new ArgumentNullException(nameof(azureService));

    /// <summary>
    /// Escapes a string value for safe use in KQL queries to prevent injection attacks.
    /// </summary>
    /// <param name="value">The string value to escape</param>
    /// <returns>The escaped string safe for use in KQL queries</returns>
    protected static string EscapeKqlString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Replace single quotes with double single quotes to escape them in KQL
        // Also escape backslashes to prevent escape sequence issues
        return value.Replace("\\", "\\\\").Replace("'", "''");
    }

    protected async Task<TokenCredential> GetCredential(string? tenant, CancellationToken cancellationToken)
    {
        var tenantId = string.IsNullOrEmpty(tenant) ? null : await AzureService.ResolveTenantIdAsync(tenant, cancellationToken);

        try
        {
            return await AzureService.GetTokenCredentialAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get credential: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets an ARM access token for the given tenant using the ARM default scope.
    /// </summary>
    /// <param name="tenant">Optional tenant ID or name to authenticate against.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="AccessToken"/> representing the ARM access token.</returns>
    protected async Task<AccessToken> GetArmAccessTokenAsync(string? tenant, CancellationToken cancellationToken)
    {
        var credential = await GetCredential(tenant, cancellationToken);
        return await credential.GetTokenAsync(
            new([AzureService.CloudConfiguration.ArmEnvironment.DefaultScope]),
            cancellationToken);
    }

    public static T AddDefaultPolicies<T>(T clientOptions) where T : ClientOptions =>
        AzureHelper.AddDefaultPolicies(clientOptions);

    /// <summary>
    /// Configures retry policy options on the provided client options
    /// </summary>
    /// <typeparam name="T">Type of client options that inherits from ClientOptions</typeparam>
    /// <param name="clientOptions">The client options to configure</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>The configured client options</returns>
    protected static T ConfigureRetryPolicy<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions =>
        AzureHelper.ConfigureRetryPolicy(clientOptions, retryPolicy);

    /// <summary>
    /// Creates an Azure Resource Manager client with an optional retry policy.
    /// </summary>
    /// <param name="tenantIdOrName">Optional Azure tenant ID or name.</param>
    /// <param name="retryPolicy">Optional retry policy configuration.</param>
    /// <param name="armClientOptions">Optional ARM client options.</param>
    protected async Task<ArmClient> CreateArmClientAsync(
        string? tenantIdOrName = null,
        RetryPolicyOptions? retryPolicy = null,
        ArmClientOptions? armClientOptions = null,
        CancellationToken cancellationToken = default) =>
        await AzureHelper.CreateArmClientAsync(AzureService, tenantIdOrName, retryPolicy, armClientOptions, cancellationToken);

    /// <summary>
    /// Validates that the provided named parameters are not null or empty
    /// </summary>
    /// <param name="namedParameters">Array of tuples containing parameter names and values to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    public static void ValidateRequiredParameters(params (string name, string? value)[] namedParameters) =>
        AzureHelper.ValidateRequiredParameters(namedParameters);

    /// <summary>
    /// Waits for the completion of a long-running operation, periodically polling the operation status until it completes.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="operation">The long-running operation.</param>
    /// <param name="cancellationToken">The cancellation token that can cancel the request.</param>
    /// <returns>The response once the long-running operation completes.</returns>
    protected static async Task WaitForLroCompletionAsync<T>(Operation<T> operation, CancellationToken cancellationToken = default) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (s_defaultPollInterval.HasValue)
        {
            await WaitForLroCompletionInternalAsync(operation, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await operation.WaitForCompletionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Waits for the completion of a long-running operation, periodically polling the operation status until it completes.
    /// </summary>
    /// <param name="operation">The long-running operation.</param>
    /// <param name="cancellationToken">The cancellation token that can cancel the request.</param>
    /// <returns>The response once the long-running operation completes.</returns>
    protected static async Task WaitForLroCompletionAsync(Operation operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (s_defaultPollInterval.HasValue)
        {
            await WaitForLroCompletionInternalAsync(operation, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await operation.WaitForCompletionResponseAsync(cancellationToken);
        }
    }

    private static async Task<Response> WaitForLroCompletionInternalAsync(Operation operation, CancellationToken cancellationToken)
    {
        while (true)
        {
            _ = await operation.UpdateStatusAsync(cancellationToken);
            if (operation.HasCompleted)
            {
                return operation.GetRawResponse();
            }

            await Task.Delay(s_defaultPollInterval!.Value, cancellationToken).ConfigureAwait(false);
        }
    }

}
