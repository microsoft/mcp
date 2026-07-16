// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubDeviceService(
    IIoTHubService ioTHubService,
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientService httpClientService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<IoTHubDeviceService> logger)
    : BaseAzureService(tenantService), IIoTHubDeviceService
{
    private readonly IIoTHubService _ioTHubService = ioTHubService ?? throw new ArgumentNullException(nameof(ioTHubService));
    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<IoTHubDeviceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Resolving the hub hostname and shared access keys requires two ARM control-plane round-trips
    // (a hub GET and a listKeys POST). Cache the result briefly so repeated data-plane calls
    // against the same hub don't pay that cost every time.
    private const string ConnectionCacheGroup = "iothub-device-connection";
    private static readonly TimeSpan s_connectionCacheDuration = TimeSpan.FromMinutes(15);

    // Upper bound for a single IoT Hub operation (control-plane + data-plane). If exceeded the
    // caller gets a clear timeout error instead of appearing to hang indefinitely.
    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);

    private sealed record HubConnection(string Hostname, List<IoTHubKey> Keys);

    private async Task<HubConnection> GetHubConnectionAsync(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{subscription}/{resourceGroup}/{name}";
        var cached = await _cacheService.GetAsync<HubConnection>(ConnectionCacheGroup, cacheKey, s_connectionCacheDuration, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached IoT Hub connection details for hub {HubName} in resource group {ResourceGroup}.", name, resourceGroup);
            return cached;
        }

        _logger.LogInformation("Resolving IoT Hub connection details for hub {HubName} in resource group {ResourceGroup}.", name, resourceGroup);
        var hubDetails = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        if (hubDetails.Count == 0)
        {
            throw new InvalidOperationException($"IoT Hub '{name}' not found in resource group '{resourceGroup}'");
        }

        var hostname = hubDetails[0].HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, retryPolicy, cancellationToken);

        _logger.LogInformation("Resolved IoT Hub connection details for hub {HubName}. Hostname={Hostname}, KeyCount={KeyCount}.", name, hostname, keys.Count);
        var connection = new HubConnection(hostname, keys);
        await _cacheService.SetAsync(ConnectionCacheGroup, cacheKey, connection, s_connectionCacheDuration, cancellationToken);
        return connection;
    }

    private static IoTHubKey SelectKey(HubConnection connection, string requiredRight)
        => connection.Keys.FirstOrDefault(k => k.Rights?.Contains(requiredRight) == true)
            ?? throw new InvalidOperationException($"No key with {requiredRight} rights found");

    private async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        var operationTimeout = timeout ?? s_operationTimeout;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(operationTimeout);
        try
        {
            return await operation(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"The IoT Hub '{operationName}' operation timed out after {operationTimeout.TotalSeconds:N0} seconds. " +
                "The hub may be unavailable or the request too large. Try again, narrow your query, or reduce --max-count.");
        }
    }

    public async Task<DeviceListResult> ListDevices(
        string name,
        string resourceGroup,
        string subscription,
        int? maxCount = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var connection = await GetHubConnectionAsync(name, resourceGroup, subscription, retryPolicy, ct);
            var hostname = connection.Hostname;
            var key = SelectKey(connection, "RegistryRead");

            using var httpClient = _httpClientService.CreateClient();
            var apiVersion = "2021-04-12";
            // The registry API has no continuation/paging support, so over-fetch by one device to
            // detect whether more devices exist beyond the requested page size.
            var fetchCount = maxCount.HasValue ? maxCount.Value + 1 : (int?)null;
            var maxCountParam = fetchCount.HasValue ? $"&top={fetchCount.Value}" : string.Empty;
            var requestUri = $"https://{hostname}/devices?api-version={apiVersion}{maxCountParam}";

            var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

            var response = await httpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            var devices = JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.ListDeviceIdentity) ?? [];

            var truncated = false;
            if (maxCount.HasValue && devices.Count > maxCount.Value)
            {
                truncated = true;
                devices = devices.GetRange(0, maxCount.Value);
            }

            return new DeviceListResult(devices, truncated);
        }, "list devices", cancellationToken);
    }

    private static string GetSasToken(string hostname, string policyName, string sharedAccessKey)
    {
        // Generate SAS token for IoT Hub authentication
        var expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var resourceUri = Uri.EscapeDataString(hostname);
        var toSign = $"{resourceUri}\n{expiry}";

        var keyBytes = Convert.FromBase64String(sharedAccessKey);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        var signature = Uri.EscapeDataString(Convert.ToBase64String(hash));

        return $"sr={resourceUri}&sig={signature}&se={expiry}&skn={policyName}";
    }
}
