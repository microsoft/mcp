// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubDeviceService(
    IIoTHubHostnameResolver hostnameResolver,
    IHttpClientFactory httpClientFactory,
    IAzureService azureService)
    : BaseAzureService(azureService), IIoTHubDeviceService
{
    private readonly IIoTHubHostnameResolver _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    // Upper bound for a single IoT Hub operation (control-plane + data-plane).
    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);

    // API version for the IoT Hub device registry data-plane REST API.
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

    public async Task<DeviceListResult> ListDevices(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        int? maxCount = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(hubName), hubName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        return await ExecuteWithTimeoutAsync(async ct =>
        {
            var hostname = await _hostnameResolver.GetHostnameAsync(hubName, resourceGroup, subscription, tenant, ct);

            // The registry API has no continuation token, so fetch one more than the requested max
            // (top = maxCount + 1). If the hub returns the extra device we know more devices exist
            // beyond the page; otherwise a full page simply means the hub holds exactly maxCount
            // devices. This avoids a false "truncated" flag when the count equals the max.
            var maxCountParam = maxCount.HasValue ? $"&top={maxCount.Value + 1}" : string.Empty;
            var requestUri = $"https://{hostname}/devices?api-version={RegistryApiVersion}{maxCountParam}";

            // Send the request through an Azure.Core pipeline with the SDK's default retry behavior.
            using var httpClient = _httpClientFactory.CreateClient();
            var clientOptions = AddDefaultPolicies(new IoTHubClientOptions());
            clientOptions.Transport = new HttpClientTransport(httpClient);
            var pipeline = HttpPipelineBuilder.Build(clientOptions);

            var credential = await GetCredential(tenant, ct);
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext([IoTHubTokenScope]), ct);

            using var request = pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.Uri.Reset(new Uri(requestUri));
            request.Headers.Add("Authorization", $"Bearer {accessToken.Token}");

            using var response = await pipeline.SendRequestAsync(request, ct);
            var content = response.Content.ToString();
            if (response.IsError)
            {
                var statusCode = (HttpStatusCode)response.Status;
                var explanation = ExplainStatusCode(statusCode);
                var detail = string.IsNullOrWhiteSpace(content) ? "No additional error details were returned." : content;
                throw new HttpRequestException(
                    $"The IoT Hub device registry request failed with status {response.Status} ({statusCode}). {explanation} {detail}",
                    inner: null,
                    statusCode: statusCode);
            }

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

    // explanation for a failed IoT Hub device registry response
    private static string ExplainStatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized =>
            "Authentication failed: the Microsoft Entra ID token was rejected. Verify you are signed in to the tenant that owns the hub and that the token has not expired.",
        HttpStatusCode.Forbidden =>
            "Authorization failed: the signed-in identity lacks data-plane access. Assign the 'IoT Hub Data Reader' role (or another role that grants device registry read) on the hub.",
        HttpStatusCode.NotFound =>
            "Not found: the IoT Hub or endpoint could not be located. Verify the hub name and that it exists in the specified subscription and resource group.",
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
