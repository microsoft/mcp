// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubDeviceService(
    IIoTHubService ioTHubService,
    IHttpClientFactory httpClientFactory,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<IoTHubDeviceService> logger)
    : BaseAzureService(tenantService), IIoTHubDeviceService
{
    private readonly IIoTHubService _ioTHubService = ioTHubService ?? throw new ArgumentNullException(nameof(ioTHubService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<IoTHubDeviceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Resolving the hub hostname and shared access keys requires two ARM control-plane round-trips
    // (a hub GET and a listKeys POST). Cache the result briefly so repeated data-plane calls
    // (list, query, twin) against the same hub don't pay that cost every time.
    private const string ConnectionCacheGroup = "iothub-device-connection";
    private static readonly TimeSpan s_connectionCacheDuration = TimeSpan.FromMinutes(15);

    // Upper bound for a single IoT Hub operation (control-plane + data-plane). If exceeded the
    // caller gets a clear timeout error instead of appearing to hang indefinitely.
    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);
    private static readonly TimeSpan s_queryRunTimeout = TimeSpan.FromSeconds(30);

    private sealed record HubConnection(string Hostname, List<IoTHubKey> Keys);

    private async Task<HubConnection> GetHubConnectionAsync(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant,
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
        var hub = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var hostname = hub.HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);

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

    private static TimeSpan GetOperationTimeout(RetryPolicyOptions? retryPolicy, TimeSpan defaultTimeout)
    {
        if (retryPolicy?.HasNetworkTimeoutSeconds == true && retryPolicy.NetworkTimeoutSeconds > 0)
        {
            return TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds.Value);
        }

        return defaultTimeout;
    }

    public async Task<DeviceIdentity> GetDevice(
        string deviceId,
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(deviceId), deviceId),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var connection = await GetHubConnectionAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);
            var hostname = connection.Hostname;
            var key = SelectKey(connection, "RegistryRead");

            using var httpClient = _httpClientFactory.CreateClient();
            var apiVersion = "2021-04-12";
            var requestUri = $"https://{hostname}/devices/{Uri.EscapeDataString(deviceId)}?api-version={apiVersion}";

            var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

            using var response = await httpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.DeviceIdentity) ?? throw new InvalidOperationException("Failed to deserialize device identity");
        }, "get device", cancellationToken);
    }

    public async Task<DeviceTwin> GetDeviceTwin(
        string deviceId,
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(deviceId), deviceId),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var connection = await GetHubConnectionAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);
            var hostname = connection.Hostname;
            var key = SelectKey(connection, "ServiceConnect");

            using var httpClient = _httpClientFactory.CreateClient();
            var apiVersion = "2021-04-12";
            var requestUri = $"https://{hostname}/twins/{Uri.EscapeDataString(deviceId)}?api-version={apiVersion}";

            var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

            using var response = await httpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.DeviceTwin) ?? throw new InvalidOperationException("Failed to deserialize device twin");
        }, "get device twin", cancellationToken);
    }

    public async Task<IoTHubQueryPage> RunQuery(
        string query,
        string name,
        string resourceGroup,
        string subscription,
        int? maxCount = null,
        string? continuationToken = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(query), query),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "IoT Hub query run started. Hub={HubName}, ResourceGroup={ResourceGroup}, MaxCount={MaxCount}, HasInputContinuationToken={HasInputContinuationToken}.",
                name,
                resourceGroup,
                maxCount,
                !string.IsNullOrEmpty(continuationToken));

            var connection = await GetHubConnectionAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);
            var hostname = connection.Hostname;
            var key = SelectKey(connection, "ServiceConnect");
            _logger.LogInformation("IoT Hub query connection resolved. Hub={HubName}, ElapsedMs={ElapsedMs}.", name, stopwatch.ElapsedMilliseconds);

            using var httpClient = _httpClientFactory.CreateClient();
            var apiVersion = "2021-04-12";
            var requestUri = $"https://{hostname}/devices/query?api-version={apiVersion}";

            var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);

            var queryObject = new IoTHubQueryRequest(query);
            var queryJson = JsonSerializer.Serialize(queryObject, IoTHubJsonContext.Default.IoTHubQueryRequest);

            // IoT Hub query paging uses request/response headers: x-ms-max-item-count sets the page
            // size and x-ms-continuation carries the cursor for the next page.
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(queryJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);
            if (maxCount.HasValue)
            {
                request.Headers.TryAddWithoutValidation("x-ms-max-item-count", maxCount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(continuationToken))
            {
                request.Headers.TryAddWithoutValidation("x-ms-continuation", continuationToken);
            }

            _logger.LogInformation("Sending IoT Hub query request. Hub={HubName}, ElapsedMs={ElapsedMs}.", name, stopwatch.ElapsedMilliseconds);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            _logger.LogInformation(
                "Received IoT Hub query response headers. Hub={HubName}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}.",
                name,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds);
            response.EnsureSuccessStatusCode();

            var nextContinuationToken = response.Headers.TryGetValues("x-ms-continuation", out var continuationValues)
                ? continuationValues.FirstOrDefault()
                : null;

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogInformation(
                "Read IoT Hub query response body. Hub={HubName}, ResponseLength={ResponseLength}, HasOutputContinuationToken={HasOutputContinuationToken}, ElapsedMs={ElapsedMs}.",
                name,
                responseContent.Length,
                !string.IsNullOrEmpty(nextContinuationToken),
                stopwatch.ElapsedMilliseconds);
            var items = JsonSerializer.Deserialize(responseContent, IoTHubJsonContext.Default.ListJsonElement) ?? [];

            _logger.LogInformation(
                "Deserialized IoT Hub query response. Hub={HubName}, Returned={Returned}, ElapsedMs={ElapsedMs}.",
                name,
                items.Count,
                stopwatch.ElapsedMilliseconds);

            return new IoTHubQueryPage(items, nextContinuationToken);
        }, "query run", cancellationToken, GetOperationTimeout(retryPolicy, s_queryRunTimeout));
    }

    public async Task<IoTHubRegistryStatistics> GetDeviceStatistics(
        string name,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var connection = await GetHubConnectionAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);
            var hostname = connection.Hostname;
            var key = SelectKey(connection, "RegistryRead");

            using var httpClient = _httpClientFactory.CreateClient();
            var apiVersion = "2021-04-12";
            var requestUri = $"https://{hostname}/statistics/devices?api-version={apiVersion}";

            var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

            using var response = await httpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.IoTHubRegistryStatistics) ?? throw new InvalidOperationException("Failed to deserialize device statistics");
        }, "get device statistics", cancellationToken);
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
