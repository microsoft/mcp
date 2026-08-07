// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.Versioning;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Core.Services.Azure;

public abstract class BaseAzureService
{
    private const int MaxAllowedRetries = 10;
    private const double MaxAllowedNetworkTimeoutSeconds = 300;
    private const double MaxAllowedDelaySeconds = 60;
    private const double MinAllowedDelaySeconds = 0.1;
    private static volatile bool s_retryLimitsDisabled = false;
    private static UserAgentPolicy s_sharedUserAgentPolicy;
    private static string? s_userAgent;
    private static volatile bool s_initialized = false;
    private static readonly object s_initializeLock = new();
    private readonly ITenantService? _tenantServiceDoNotUseDirectly;

    // Cache assembly metadata to avoid repeated reflection
    private static readonly string s_version;
    private static readonly string s_framework;
    private static readonly string s_platform;
    private static readonly string s_defaultUserAgent;
    private static readonly TimeSpan? s_defaultPollInterval = null;

    static BaseAzureService()
    {
        var assembly = typeof(BaseAzureService).Assembly;
        s_version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        s_framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        s_platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        // Initialize the default user agent policy without transport type
        s_defaultUserAgent = $"azmcp/{s_version} ({s_framework}; {s_platform})";
        s_sharedUserAgentPolicy = new UserAgentPolicy(s_defaultUserAgent);

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
    public static void InitializeUserAgentPolicy(string transportType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transportType, nameof(transportType));

        // Ensure this method is called only once
        lock (s_initializeLock)
        {
            if (s_initialized)
            {
                return;
            }

            s_userAgent = $"azmcp/{s_version} azmcp-{transportType}/{s_version} ({s_framework}; {s_platform})";
            s_sharedUserAgentPolicy = new UserAgentPolicy(s_userAgent);

            s_initialized = true;
        }
    }

    /// <summary>
    /// Disables upper bounds enforcement on retry policy values (delays, timeouts, max retries).
    /// This method should be called once during application startup when the --dangerously-disable-retry-limits flag is set.
    /// </summary>
    public static void DisableRetryLimits() => s_retryLimitsDisabled = true;

    /// <summary>
    /// Resets the retry limits flag. For testing only.
    /// </summary>
    internal static void ResetRetryLimits() => s_retryLimitsDisabled = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAzureService"/> class.
    /// </summary>
    /// <param name="tenantService">
    /// An <see cref="ITenantService"/> used for Azure API calls.
    /// </param>
    protected BaseAzureService(ITenantService tenantService)
    {
        ArgumentNullException.ThrowIfNull(tenantService, nameof(tenantService));
        TenantService = tenantService;
        UserAgent = s_userAgent ?? s_defaultUserAgent;
    }

    /// <summary>
    /// DO NOT USE THIS CONSTRUCTOR.
    /// </summary>
    /// <remarks>
    /// This is only to be used by <see cref="Tenant.TenantService"/> to overcome a circular dependency on itself.</remarks>
    internal BaseAzureService()
    {
        UserAgent = s_userAgent ?? s_defaultUserAgent;
    }

    protected string UserAgent { get; }

    /// <summary>
    /// Gets or initializes the tenant service for resolving tenant IDs and obtaining credentials.
    /// </summary>
    /// <remarks>
    /// Do not <see langword="init"/> this. The initializer is just for <see cref="Tenant.TenantService"/>
    /// to overcome a circular dependency on itself. In all other cases, pass the constructor
    /// a non-null <see cref="ITenantService"/>.
    /// </remarks>
    protected ITenantService TenantService
    {
        get
        {
            return _tenantServiceDoNotUseDirectly
                ?? throw new InvalidOperationException($"{nameof(TenantService)} is not set. This is a code bug. Use the {nameof(BaseAzureService)} constructor with a non-null {nameof(ITenantService)}.");
        }

        init
        {
            _tenantServiceDoNotUseDirectly = value;
        }
    }

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

    protected async Task<string?> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken)
    {
        if (tenant == null)
            return tenant;
        return await TenantService.GetTenantId(tenant, cancellationToken);
    }

    protected async Task<TokenCredential> GetCredential(CancellationToken cancellationToken)
    {
        // TODO @vukelich: separate PR for cancellationToken to be required, not optional default
        return await GetCredential(null, cancellationToken);
    }

    protected async Task<TokenCredential> GetCredential(string? tenant, CancellationToken cancellationToken)
    {
        // TODO @vukelich: separate PR for cancellationToken to be required, not optional default
        var tenantId = string.IsNullOrEmpty(tenant) ? null : await ResolveTenantIdAsync(tenant, cancellationToken);

        try
        {
            return await TenantService.GetTokenCredentialAsync(tenantId, cancellationToken);
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
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task<AccessToken> GetArmAccessTokenAsync(string? tenant, CancellationToken cancellationToken)
    {
        var credential = await GetCredential(tenant, cancellationToken);
        return await credential.GetTokenAsync(
            new TokenRequestContext([TenantService.CloudConfiguration.ArmEnvironment.DefaultScope]),
            cancellationToken);
    }

    protected static T AddDefaultPolicies<T>(T clientOptions) where T : ClientOptions
    {
        clientOptions.AddPolicy(s_sharedUserAgentPolicy, HttpPipelinePosition.BeforeTransport);
        return clientOptions;
    }

    /// <summary>
    /// Configures retry policy options on the provided client options
    /// </summary>
    /// <typeparam name="T">Type of client options that inherits from ClientOptions</typeparam>
    /// <param name="clientOptions">The client options to configure</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>The configured client options</returns>
    protected static T ConfigureRetryPolicy<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions
    {
        if (retryPolicy != null)
        {
            if (retryPolicy.DelaySeconds is { } delaySeconds)
            {
                clientOptions.Retry.Delay = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(delaySeconds)
                    : TimeSpan.FromSeconds(Math.Clamp(delaySeconds, MinAllowedDelaySeconds, MaxAllowedDelaySeconds));
            }
            if (retryPolicy.MaxDelaySeconds is { } maxDelaySeconds)
            {
                clientOptions.Retry.MaxDelay = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(maxDelaySeconds)
                    : TimeSpan.FromSeconds(Math.Clamp(maxDelaySeconds, MinAllowedDelaySeconds, MaxAllowedDelaySeconds));
            }
            if (retryPolicy.MaxRetries is { } maxRetries)
            {
                clientOptions.Retry.MaxRetries = s_retryLimitsDisabled
                    ? maxRetries
                    : Math.Min(MaxAllowedRetries, maxRetries);
            }
            if (retryPolicy.Mode is { } mode)
            {
                clientOptions.Retry.Mode = mode;
            }
            if (retryPolicy.NetworkTimeoutSeconds is { } networkTimeoutSeconds)
            {
                clientOptions.Retry.NetworkTimeout = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(networkTimeoutSeconds)
                    : TimeSpan.FromSeconds(Math.Min(MaxAllowedNetworkTimeoutSeconds, networkTimeoutSeconds));
            }
        }

        return clientOptions;
    }

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
        CancellationToken cancellationToken = default)
    {
        var tenantId = await ResolveTenantIdAsync(tenantIdOrName, cancellationToken);

        try
        {
            TokenCredential credential = await GetCredential(tenantId, cancellationToken);
            ArmClientOptions options = armClientOptions ?? new();
            options.Transport = new HttpClientTransport(TenantService.GetClient());
            options.Environment = TenantService.CloudConfiguration.ArmEnvironment;
            ConfigureRetryPolicy(AddDefaultPolicies(options), retryPolicy);

            ArmClient armClient = new(credential, defaultSubscriptionId: default, options);
            return armClient;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create ARM client: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that the provided named parameters are not null or empty
    /// </summary>
    /// <param name="namedParameters">Array of tuples containing parameter names and values to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    protected static void ValidateRequiredParameters(params (string name, string? value)[] namedParameters)
    {
        var missingParams = namedParameters
            .Where(param => string.IsNullOrEmpty(param.value))
            .Select(param => param.name)
            .ToArray();

        if (missingParams.Length > 0)
        {
            throw new ArgumentException(
                $"Required parameter{(missingParams.Length > 1 ? "s are" : " is")} null or empty: {string.Join(", ", missingParams)}");
        }
    }

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
    private static async Task<Response> WaitForLroCompletionInternalAsync(Operation operation, IProgress<string>? progress, CancellationToken cancellationToken)
    {
        // Base case: No progress, no default poll interval. Just wait for the operation to complete.
        if (progress == null && !s_defaultPollInterval.HasValue)
        {
            return await operation.WaitForCompletionResponseAsync(cancellationToken);
        }
        else
            if (s_defaultPollInterval.HasValue)
            {
                // Next case: Loop polling the async operation status at a fixed interval until it completes. 
                // This is used when a default poll interval is set (e.g., during playback testing) to work around 
                // default behavior of the Azure SDK that waits for 1 second before polling the operation status.
                while (true)
                {
                    // We want to poll the operation status at a fixed interval, either because a default poll interval is set or because progress reporting is requested.
                    Response response = await operation.UpdateStatusAsync(cancellationToken);
                    if (operation.HasCompleted)
                    {
                        return response;
                    }
                    await Task.Delay(s_defaultPollInterval.Value, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (progress != null)
            {
                // Finally, if progress is specified, we will report progress at a fixed interval while waiting for the operation to complete.

                // Capture the operation completion task and the start time to calculate elapsed time for progress reporting.
                Task<Response> completionTask = operation.WaitForCompletionResponseAsync(cancellationToken).AsTask();
                var startTime = DateTime.UtcNow; // Initialize elapsed time to zero
                using PeriodicTimer progressTimer = new(s_progressPollInterval);

                // While the completion task is not completed, we will wait for either the completion task or the progress timer to tick.
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
    }
}
