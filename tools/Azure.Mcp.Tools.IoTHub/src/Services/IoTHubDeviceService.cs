// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubDeviceService(
    IIoTHubHostnameResolver hostnameResolver,
    IHttpClientFactory httpClientFactory,
    IAzureService azureService,
    ILogger<IoTHubDeviceService> logger)
    : BaseAzureService(azureService), IIoTHubDeviceService
{
    private readonly IIoTHubHostnameResolver _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<IoTHubDeviceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Upper bound for a single IoT Hub operation (control-plane + data-plane). If exceeded the
    // caller gets a clear timeout error instead of appearing to hang indefinitely.
    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);
    private static readonly TimeSpan s_queryRunTimeout = TimeSpan.FromSeconds(30);
    private const string RegistryApiVersion = "2021-04-12";

    // Microsoft Entra ID scope for the IoT Hub service (data-plane) REST API.
    private const string IoTHubTokenScope = "https://iothubs.azure.net/.default";

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
                $"The IoT Hub '{operationName}' operation timed out after {operationTimeout.TotalSeconds:N0} seconds.");
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

    // Resolve the hub's data-plane hostname (briefly cached control-plane lookup) and mint a fresh
    // Entra ID bearer token for the operation. Data-plane calls authenticate with the caller's Entra
    // ID token and RBAC role assignments; no shared access keys are fetched, held, or cached.
    private async Task<(string Hostname, string Token)> ResolveHubAccessAsync(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var hostname = await _hostnameResolver.GetHostnameAsync(hubName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var credential = await GetCredential(tenant, cancellationToken);
        var accessToken = await credential.GetTokenAsync(new TokenRequestContext([IoTHubTokenScope]), cancellationToken);
        return (hostname, accessToken.Token);
    }

    // Build an Azure.Core pipeline for a data-plane call so the caller's RetryPolicyOptions govern
    // transient-failure retries (408/429/5xx and network errors), consistent with the ARM control-plane.
    private static HttpPipeline BuildDataPlanePipeline(RetryPolicyOptions? retryPolicy, HttpClient httpClient)
    {
        var clientOptions = ConfigureRetryPolicy(AddDefaultPolicies(new IoTHubClientOptions()), retryPolicy);
        clientOptions.Transport = new HttpClientTransport(httpClient);
        return HttpPipelineBuilder.Build(clientOptions);
    }

    // Translate a failed IoT Hub data-plane response into an actionable exception. The thrown
    // HttpRequestException carries the status code so the command surfaces the matching HTTP status.
    private static void EnsureSuccessOrThrow(Response response)
    {
        if (!response.IsError)
        {
            return;
        }

        var statusCode = (HttpStatusCode)response.Status;
        var detail = response.Content?.ToString();
        detail = string.IsNullOrWhiteSpace(detail) ? "No additional error details were returned." : detail;

        throw new HttpRequestException(
            $"The IoT Hub request failed with status {response.Status} ({statusCode}). {ExplainStatusCode(statusCode)} {detail}",
            inner: null,
            statusCode: statusCode);
    }

    public async Task<DeviceListResult> ListDevices(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        int? maxCount = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(hubName), hubName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var (hostname, token) = await ResolveHubAccessAsync(hubName, resourceGroup, subscription, tenant, retryPolicy, ct);

            // The registry API has no continuation token, so fetch one more than the requested max
            // (top = maxCount + 1). If the hub returns the extra device we know more devices exist
            // beyond the page; otherwise a full page simply means the hub holds exactly maxCount
            // devices. This avoids a false "truncated" flag when the count equals the max.
            var maxCountParam = maxCount.HasValue ? $"&top={maxCount.Value + 1}" : string.Empty;
            var requestUri = $"https://{hostname}/devices?api-version={RegistryApiVersion}{maxCountParam}";

            using var httpClient = _httpClientFactory.CreateClient();
            var pipeline = BuildDataPlanePipeline(retryPolicy, httpClient);

            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {token}");

            using var response = await pipeline.SendRequestAsync(request, ct);
            EnsureSuccessOrThrow(response);

            var content = response.Content.ToString();
            var devices = JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.ListDeviceIdentity) ?? [];

            // We requested maxCount + 1: if the hub returned more than maxCount there are additional
            // devices, so drop the extra device and flag the result as truncated.
            var truncated = false;
            if (maxCount.HasValue && devices.Count > maxCount.Value)
            {
                truncated = true;
                devices = devices.GetRange(0, maxCount.Value);
            }

            return new DeviceListResult(devices, truncated);
        }, "list devices", cancellationToken);
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
            var (hostname, token) = await ResolveHubAccessAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);

            using var httpClient = _httpClientFactory.CreateClient();
            var pipeline = BuildDataPlanePipeline(retryPolicy, httpClient);
            var requestUri = $"https://{hostname}/devices/{Uri.EscapeDataString(deviceId)}?api-version={RegistryApiVersion}";

            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {token}");

            using var response = await pipeline.SendRequestAsync(request, ct);
            EnsureSuccessOrThrow(response);

            var content = response.Content.ToString();
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
            var (hostname, token) = await ResolveHubAccessAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);

            using var httpClient = _httpClientFactory.CreateClient();
            var pipeline = BuildDataPlanePipeline(retryPolicy, httpClient);
            var requestUri = $"https://{hostname}/twins/{Uri.EscapeDataString(deviceId)}?api-version={RegistryApiVersion}";

            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {token}");

            using var response = await pipeline.SendRequestAsync(request, ct);
            EnsureSuccessOrThrow(response);

            var content = response.Content.ToString();
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

            var (hostname, token) = await ResolveHubAccessAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);
            _logger.LogInformation("IoT Hub query connection resolved. Hub={HubName}, ElapsedMs={ElapsedMs}.", name, stopwatch.ElapsedMilliseconds);

            using var httpClient = _httpClientFactory.CreateClient();
            var pipeline = BuildDataPlanePipeline(retryPolicy, httpClient);
            var requestUri = $"https://{hostname}/devices/query?api-version={RegistryApiVersion}";

            var queryObject = new IoTHubQueryRequest(query);
            var queryJson = JsonSerializer.Serialize(queryObject, IoTHubJsonContext.Default.IoTHubQueryRequest);

            // IoT Hub query paging uses request/response headers: x-ms-max-item-count sets the page
            // size and x-ms-continuation carries the cursor for the next page.
            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Post;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Content-Type", "application/json");
            if (maxCount.HasValue)
            {
                request.Headers.Add("x-ms-max-item-count", maxCount.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(continuationToken))
            {
                request.Headers.Add("x-ms-continuation", continuationToken);
            }
            request.Content = RequestContent.Create(Encoding.UTF8.GetBytes(queryJson));

            _logger.LogInformation("Sending IoT Hub query request. Hub={HubName}, ElapsedMs={ElapsedMs}.", name, stopwatch.ElapsedMilliseconds);
            using var response = await pipeline.SendRequestAsync(request, ct);
            _logger.LogInformation(
                "Received IoT Hub query response headers. Hub={HubName}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs}.",
                name,
                response.Status,
                stopwatch.ElapsedMilliseconds);
            EnsureSuccessOrThrow(response);

            var nextContinuationToken = response.Headers.TryGetValue("x-ms-continuation", out var continuationValue)
                ? continuationValue
                : null;

            var responseContent = response.Content.ToString();
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
            var (hostname, token) = await ResolveHubAccessAsync(name, resourceGroup, subscription, tenant, retryPolicy, ct);

            using var httpClient = _httpClientFactory.CreateClient();
            var pipeline = BuildDataPlanePipeline(retryPolicy, httpClient);
            var requestUri = $"https://{hostname}/statistics/devices?api-version={RegistryApiVersion}";

            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {token}");

            using var response = await pipeline.SendRequestAsync(request, ct);
            EnsureSuccessOrThrow(response);

            var content = response.Content.ToString();
            return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.IoTHubRegistryStatistics) ?? throw new InvalidOperationException("Failed to deserialize device statistics");
        }, "get device statistics", cancellationToken);
    }

    // explanation for a failed IoT Hub device registry response
    private static string ExplainStatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized =>
            "Authentication failed: the Microsoft Entra ID token was rejected. Verify you are signed in to the tenant that owns the hub and that the token has not expired.",
        HttpStatusCode.Forbidden =>
            "Authorization failed: the signed-in identity lacks data-plane access. Assign the 'IoT Hub Data Reader' role (or another role that grants device registry read) on the hub.",
        HttpStatusCode.NotFound =>
            "Not found: the IoT Hub, device, or endpoint could not be located. Verify the hub name, device ID, and that the resource exists in the specified subscription and resource group.",
        HttpStatusCode.TooManyRequests =>
            "Throttled: the IoT Hub identity registry rate limit was exceeded. Wait and retry, and consider a smaller --max-count.",
        HttpStatusCode.RequestTimeout =>
            "The request timed out; the hub may be busy. Retry the operation.",
        HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout =>
            "The IoT Hub service is temporarily unavailable. This is usually transient; retry the operation.",
        _ =>
            "The request could not be completed.",
    };
}
