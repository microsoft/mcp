// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.Versioning;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure;

public abstract class BaseAzureService(ITenantService? tenantService = null, ILoggerFactory? loggerFactory = null)
{
    private static readonly UserAgentPolicy s_sharedUserAgentPolicy;
    public static readonly string DefaultUserAgent;

    private CustomChainedCredential? _credential;
    private string? _lastTenantId;
    private ArmClient? _armClient;
    private string? _lastArmClientTenantId;
    private RetryPolicyOptions? _lastRetryPolicy;
    private readonly ITenantService? _tenantService = tenantService;
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;

    protected ILoggerFactory LoggerFactory => _loggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

    static BaseAzureService()
    {
        var assembly = typeof(BaseAzureService).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        var platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        DefaultUserAgent = $"azmcp/{version} ({framework}; {platform})";
        s_sharedUserAgentPolicy = new UserAgentPolicy(DefaultUserAgent);
    }

    protected string UserAgent { get; } = DefaultUserAgent;

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

    protected async Task<string?> ResolveTenantIdAsync(string? tenant)
    {
        if (tenant == null || _tenantService == null)
            return tenant;
        return await _tenantService.GetTenantId(tenant);
    }

    protected async Task<TokenCredential> GetCredential(string? tenant = null)
    {
        var tenantId = string.IsNullOrEmpty(tenant) ? null : await ResolveTenantIdAsync(tenant);

        // Return cached credential if it exists and tenant ID hasn't changed
        if (_credential != null && _lastTenantId == tenantId)
        {
            return _credential;
        }

        try
        {
            ILogger<CustomChainedCredential>? logger = _loggerFactory?.CreateLogger<CustomChainedCredential>();
            _credential = new CustomChainedCredential(tenantId, logger);
            _lastTenantId = tenantId;
            return _credential;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get credential: {ex.Message}", ex);
        }
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
            if (retryPolicy.HasDelaySeconds)
            {
                clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            }
            if (retryPolicy.HasMaxDelaySeconds)
            {
                clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            }
            if (retryPolicy.HasMaxRetries)
            {
                clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries;
            }
            if (retryPolicy.HasMode)
            {
                clientOptions.Retry.Mode = retryPolicy.Mode;
            }
            if (retryPolicy.HasNetworkTimeoutSeconds)
            {
                clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
            }
        }

        return clientOptions;
    }

    /// <summary>
    /// Creates an Azure Resource Manager client with optional retry policy
    /// </summary>
    /// <param name="tenant">Optional Azure tenant ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="armClientOptions">Optional ARM client options</param>
    protected async Task<ArmClient> CreateArmClientAsync(string? tenant = null, RetryPolicyOptions? retryPolicy = null, ArmClientOptions? armClientOptions = null)
    {
        var tenantId = await ResolveTenantIdAsync(tenant);

        // Return cached client if parameters match
        if (_armClient != null &&
            _lastArmClientTenantId == tenantId &&
            armClientOptions == null &&
            RetryPolicyOptions.AreEqual(_lastRetryPolicy, retryPolicy))
        {
            return _armClient;
        }

        try
        {
            var credential = await GetCredential(tenantId);
            var options = armClientOptions ?? new ArmClientOptions();
            ConfigureRetryPolicy(AddDefaultPolicies(options), retryPolicy);

            _armClient = new ArmClient(credential, default, options);
            _lastArmClientTenantId = tenantId;
            _lastRetryPolicy = retryPolicy;

            return _armClient;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create ARM client: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates that the provided parameters are not null or empty
    /// </summary>
    /// <param name="parameters">Array of parameters to validate</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty</exception>
    protected static void ValidateRequiredParameters(params string?[] parameters)
    {
        foreach (var param in parameters)
        {
            ArgumentException.ThrowIfNullOrEmpty(param);
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
}
