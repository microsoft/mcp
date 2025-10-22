// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.Versioning;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;

namespace Azure.Mcp.Core.Services.Azure;

public abstract class BaseAzureService
{
    private static readonly UserAgentPolicy s_sharedUserAgentPolicy;
    public static readonly string DefaultUserAgent;

    private ITenantService? _tenantServiceDoNotUseDirectly;

    static BaseAzureService()
    {
        var assembly = typeof(BaseAzureService).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        var platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        DefaultUserAgent = $"azmcp/{version} ({framework}; {platform})";
        s_sharedUserAgentPolicy = new UserAgentPolicy(DefaultUserAgent);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAzureService"/> class.
    /// </summary>
    /// <param name="tenantService">
    /// An <see cref="ITenantService"/> used for Azure API calls.
    /// </param>
    protected BaseAzureService(ITenantService tenantService)
    {
        ArgumentNullException.ThrowIfNull(tenantService, nameof(tenantService));
        _tenantServiceDoNotUseDirectly = tenantService;
    }

    /// <summary>
    /// DO NOT USE THIS CONSTRUCTOR.
    /// </summary>
    /// <remarks>
    /// This is only to be used by <see cref="Tenant.TenantService"/> to overcome a circular dependency on itself.</remarks>
    internal BaseAzureService()
    {
    }

    protected string UserAgent { get; } = DefaultUserAgent;

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

    protected async Task<string?> ResolveTenantIdAsync(string? tenant)
    {
        if (tenant == null)
            return tenant;
        return await TenantService.GetTenantId(tenant);
    }

    protected async Task<TokenCredential> GetCredential(CancellationToken cancellationToken = default)
    {
        // TODO @vukelich: separate PR for cancellationToken to be required, not optional default
        return await GetCredential(null, cancellationToken);
    }

    protected async Task<TokenCredential> GetCredential(string? tenant, CancellationToken cancellationToken = default)
    {
        // TODO @vukelich: separate PR for cancellationToken to be required, not optional default
        var tenantId = string.IsNullOrEmpty(tenant) ? null : await ResolveTenantIdAsync(tenant);

        try
        {
            return await TenantService!.GetTokenCredentialAsync(tenantId, cancellationToken);
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
        var tenantId = await ResolveTenantIdAsync(tenantIdOrName);

        try
        {
            TokenCredential credential = await GetCredential(tenantId, cancellationToken);
            ArmClientOptions options = armClientOptions ?? new();
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
}
