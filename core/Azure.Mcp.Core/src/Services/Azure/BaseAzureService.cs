// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
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
    /// <remarks>
    /// Obtain HTTP clients through <see cref="IAzureService.GetClient"/>. Shared factory
    /// transports apply the current command's namespace override at send time, including
    /// clients used by reusable SDK objects. Do not cache an override decision in a service.
    /// </remarks>
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
    /// Creates an Azure Resource Manager client.
    /// </summary>
    /// <param name="tenantIdOrName">Optional Azure tenant ID or name.</param>
    /// <param name="armClientOptions">Optional ARM client options.</param>
    protected async Task<ArmClient> CreateArmClientAsync(
        string? tenantIdOrName = null,
        ArmClientOptions? armClientOptions = null,
        CancellationToken cancellationToken = default) =>
        await AzureHelper.CreateArmClientAsync(AzureService, tenantIdOrName, armClientOptions: armClientOptions, cancellationToken: cancellationToken);

    /// <summary>
    /// Validates that the provided parameter is not null or empty.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <exception cref="ArgumentException">Thrown when the parameter is null or empty.</exception>
    public static void ValidateRequiredParameter(string name, [NotNull] string? value) =>
        AzureHelper.ValidateRequiredParameter(name, value);

    /// <summary>
    /// Validates that the provided named parameters are not null or empty
    /// </summary>
    /// <param name="namedParameters">Array of tuples containing parameter names and values to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    public static void ValidateRequiredParameters(params (string name, string? value)[] namedParameters) =>
        AzureHelper.ValidateRequiredParameters(namedParameters);

    /// <summary>
    /// Interval at which <see cref="WaitForLroCompletionAsync{T}(Operation{T}, IProgress{string}, CancellationToken)"/>
    /// reports progress while the Azure SDK waits for the operation to complete. This keeps the caller (and any MCP client
    /// watching for session activity) informed while a genuinely long-running Azure operation is still in progress,
    /// instead of appearing idle for the entire duration of the wait.
    /// </summary>
    private static readonly TimeSpan s_progressPollInterval = TimeSpan.FromSeconds(15);

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
            await WaitForLroCompletionInternalAsync(operation, null, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await operation.WaitForCompletionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Waits for the completion of a long-running operation, periodically polling the operation status until it completes
    /// while reporting progress to keep the caller informed during long waits.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="operation">The long-running operation.</param>
    /// <param name="progress">
    /// Progress reporter. A status message is reported at a fixed interval while the operation is still running so that
    /// callers can surface liveness (e.g. via MCP progress notifications) during long waits.
    /// </param>
    /// <param name="cancellationToken">The cancellation token that can cancel the request.</param>
    /// <returns>The response once the long-running operation completes.</returns>
    protected static async Task WaitForLroCompletionAsync<T>(Operation<T> operation, IProgress<string> progress, CancellationToken cancellationToken = default) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(progress);

        await WaitForLroCompletionInternalAsync(operation, progress, cancellationToken).ConfigureAwait(false);
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

        await WaitForLroCompletionInternalAsync(operation, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the completion of a long-running operation, periodically polling the operation status until it completes
    /// while reporting progress to keep the caller informed during long waits.
    /// </summary>
    /// <param name="operation">The long-running operation.</param>
    /// <param name="progress">
    /// Progress reporter. A status message is reported at a fixed interval while the operation is still running so that
    /// callers can surface liveness (e.g. via MCP progress notifications) during long waits.
    /// </param>
    /// <param name="cancellationToken">The cancellation token that can cancel the request.</param>
    /// <returns>The response once the long-running operation completes.</returns>
    protected static async Task WaitForLroCompletionAsync(Operation operation, IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(progress);

        await WaitForLroCompletionInternalAsync(operation, progress, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the completion of a long-running operation, periodically polling the operation status until it completes.
    ///
    /// There are three cases for how this method behaves:
    /// 1. If no progress reporter is provided and no default poll interval is set, the method will simply wait for the operation to complete without reporting progress.
    /// 2. If a progress reporter is provided, the method will report progress at a fixed interval while waiting for the operation to complete.
    /// 3. If no progress reporter is provided but a default poll interval is set, the method will wait for the default poll interval before checking the status again.
    /// </summary>
    ///
    /// <param name="operation">The long-running operation.</param>
    /// <param name="progress">A progress callback that reports the status of the operation at regular intervals.</param>
    /// <param name="cancellationToken">The cancellation token that can cancel the request.</param>
    /// <returns>The response once the long-running operation completes.</returns>
    /// 
    /// <remarks>
    /// The s_defaultPollInterval value is set to TimeSpan.Zero during playback testing to avoid unnecessary delays in test execution. In production, it is null, and the method will wait for the operation to complete without polling unless a progress reporter is provided.
    /// </remarks>
    private static Task<Response> WaitForLroCompletionInternalAsync(
        Operation operation,
        IProgress<string>? progress,
        CancellationToken cancellationToken) =>
        WaitForLroCompletionInternalAsync(operation, progress, s_defaultPollInterval, s_progressPollInterval, cancellationToken);

    internal static async Task<Response> WaitForLroCompletionInternalAsync(
        Operation operation,
        IProgress<string>? progress,
        TimeSpan? defaultPollInterval,
        TimeSpan progressPollInterval,
        CancellationToken cancellationToken)
    {
        if (defaultPollInterval.HasValue)
        {
            // Loop polling the async operation status at a fixed interval until it completes.
            // This is used during playback testing to avoid the Azure SDK's default polling delay.
            while (true)
            {
                Response response = await operation.UpdateStatusAsync(cancellationToken).ConfigureAwait(false);
                if (operation.HasCompleted)
                {
                    return response;
                }

                await Task.Delay(defaultPollInterval.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        else if (progress is not null)
        {
            Task<Response> completionTask = operation.WaitForCompletionResponseAsync(cancellationToken).AsTask();
            var startTime = DateTime.UtcNow;
            using PeriodicTimer progressTimer = new(progressPollInterval);

            while (!completionTask.IsCompleted)
            {
                Task<bool> progressTimerTask = progressTimer.WaitForNextTickAsync(cancellationToken).AsTask();
                if (await Task.WhenAny(completionTask, progressTimerTask).ConfigureAwait(false) == completionTask)
                {
                    break;
                }

                if (await progressTimerTask.ConfigureAwait(false))
                {
                    var currentElapsed = DateTime.UtcNow - startTime;
                    progress.Report($"Still waiting for the operation to complete ({currentElapsed:mm\\:ss} elapsed)...");
                }
            }

            return await completionTask.ConfigureAwait(false);
        }
        else
        {
            return await operation.WaitForCompletionResponseAsync(cancellationToken);
        }
    }

}
