// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.ResourceManager;

namespace Azure.Mcp.Core.Services.Azure;

public abstract class BaseAzureService
{
    private static UserAgentPolicy s_sharedUserAgentPolicy;
    private static string? s_userAgent;
    private static volatile bool s_initialized = false;
    private static readonly object s_initializeLock = new();
    private static HttpPipelineTransport? s_sharedHttpTransport;
    private static volatile bool s_httpTransportInitialized;
    private static readonly object s_httpTransportLock = new();
    private readonly ITenantService? _tenantServiceDoNotUseDirectly;

    // Cache assembly metadata to avoid repeated reflection
    private static readonly string s_version;
    private static readonly string s_framework;
    private static readonly string s_platform;
    private static readonly string s_defaultUserAgent;

    static BaseAzureService()
    {
        var assembly = typeof(BaseAzureService).Assembly;
        s_version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        s_framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        s_platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        // Initialize the default user agent policy without transport type
        s_defaultUserAgent = $"azmcp/{s_version} ({s_framework}; {s_platform})";
        s_sharedUserAgentPolicy = new UserAgentPolicy(s_defaultUserAgent);
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
        if (s_sharedHttpTransport != null && (clientOptions.Transport == null || ReferenceEquals(clientOptions.Transport, HttpClientTransport.Shared)))
        {
            clientOptions.Transport = s_sharedHttpTransport;
        }
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
        var tenantId = await ResolveTenantIdAsync(tenantIdOrName, cancellationToken);

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

    /// <summary>
    /// Initializes the shared HTTP transport to ensure Azure SDK clients reuse the same configured <see cref="HttpClient"/> pipeline.
    /// </summary>
    /// <param name="httpClientService">The <see cref="IHttpClientService"/> that provides the shared <see cref="HttpClient"/>.</param>
    public static void InitializeHttpClientTransport(IHttpClientService httpClientService)
    {
        ArgumentNullException.ThrowIfNull(httpClientService);

        lock (s_httpTransportLock)
        {
            if (s_httpTransportInitialized)
            {
                return;
            }

            s_sharedHttpTransport = new HttpClientTransport(httpClientService.DefaultClient);
            s_httpTransportInitialized = true;
        }
    }
}
