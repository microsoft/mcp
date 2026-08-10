// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.Versioning;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Core.Services.Azure.Helpers;

/// <summary>
/// Helper class containing methods for working with Azure services.
/// </summary>
public static class AzureHelper
{
    private const int MaxAllowedRetries = 10;
    private const double MaxAllowedNetworkTimeoutSeconds = 300;
    private const double MaxAllowedDelaySeconds = 60;
    private const double MinAllowedDelaySeconds = 0.1;
    private static volatile bool s_retryLimitsDisabled = false;
    private static UserAgentPolicy s_sharedUserAgentPolicy;
    private static string? s_userAgent;
    private static volatile bool s_initialized = false;
    private static readonly Lock s_initializeLock = new();

    // Cache assembly metadata to avoid repeated reflection
    private static readonly string s_version;
    private static readonly string s_framework;
    private static readonly string s_platform;
    private static readonly string s_defaultUserAgent;

    internal static string UserAgent => s_userAgent ?? s_defaultUserAgent;

    static AzureHelper()
    {
        var assembly = typeof(AzureHelper).Assembly;
        s_version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        s_framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        s_platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        // Initialize the default user agent policy without transport type
        s_defaultUserAgent = $"azmcp/{s_version} ({s_framework}; {s_platform})";
        s_sharedUserAgentPolicy = new UserAgentPolicy(s_defaultUserAgent);
    }

    /// <summary>
    /// Validates that the provided named parameters are not null or empty
    /// </summary>
    /// <param name="namedParameters">Array of tuples containing parameter names and values to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    internal static void ValidateRequiredParameters(params (string name, string? value)[] namedParameters)
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

    internal static T AddDefaultPolicies<T>(T clientOptions) where T : ClientOptions
    {
        clientOptions.AddPolicy(s_sharedUserAgentPolicy, HttpPipelinePosition.BeforeTransport);
        return clientOptions;
    }


    /// <summary>
    /// Disables upper bounds enforcement on retry policy values (delays, timeouts, max retries).
    /// This method should be called once during application startup when the --dangerously-disable-retry-limits flag is set.
    /// </summary>
    internal static void DisableRetryLimits() => s_retryLimitsDisabled = true;

    /// <summary>
    /// Resets the retry limits flag. For testing only.
    /// </summary>
    internal static void ResetRetryLimits() => s_retryLimitsDisabled = false;

    /// <summary>
    /// Configures retry policy options on the provided client options
    /// </summary>
    /// <typeparam name="T">Type of client options that inherits from ClientOptions</typeparam>
    /// <param name="clientOptions">The client options to configure</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>The configured client options</returns>
    internal static T ConfigureRetryPolicy<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions
    {
        if (retryPolicy != null)
        {
            if (retryPolicy.DelaySeconds is double delaySeconds)
            {
                clientOptions.Retry.Delay = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(delaySeconds)
                    : TimeSpan.FromSeconds(Math.Clamp(delaySeconds, MinAllowedDelaySeconds, MaxAllowedDelaySeconds));
            }
            if (retryPolicy.MaxDelaySeconds is double maxDelaySeconds)
            {
                clientOptions.Retry.MaxDelay = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(maxDelaySeconds)
                    : TimeSpan.FromSeconds(Math.Clamp(maxDelaySeconds, MinAllowedDelaySeconds, MaxAllowedDelaySeconds));
            }
            if (retryPolicy.MaxRetries is int maxRetries)
            {
                clientOptions.Retry.MaxRetries = s_retryLimitsDisabled
                    ? maxRetries
                    : Math.Min(MaxAllowedRetries, maxRetries);
            }
            if (retryPolicy.Mode is { } mode)
            {
                clientOptions.Retry.Mode = mode;
            }
            if (retryPolicy.NetworkTimeoutSeconds is double networkTimeoutSeconds)
            {
                clientOptions.Retry.NetworkTimeout = s_retryLimitsDisabled
                    ? TimeSpan.FromSeconds(networkTimeoutSeconds)
                    : TimeSpan.FromSeconds(Math.Min(MaxAllowedNetworkTimeoutSeconds, networkTimeoutSeconds));
            }
        }

        return clientOptions;
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
    internal static void InitializeUserAgentPolicy(string transportType)
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

    /// <inheritdoc/>
    internal static async Task<ArmClient> CreateArmClientAsync(
        IAzureService azureService,
        string? tenantIdOrName = null,
        RetryPolicyOptions? retryPolicy = null,
        ArmClientOptions? armClientOptions = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await azureService.ResolveTenantIdAsync(tenantIdOrName, cancellationToken);

        try
        {
            var credential = await azureService.GetTokenCredentialAsync(tenantId, cancellationToken);
            var options = armClientOptions ?? new();
            options.Transport = new HttpClientTransport(azureService.GetClient());
            options.Environment = azureService.CloudConfiguration.ArmEnvironment;
            ConfigureRetryPolicy(AddDefaultPolicies(options), retryPolicy);

            return new(credential, defaultSubscriptionId: default, options);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create ARM client: {ex.Message}", ex);
        }
    }
}
