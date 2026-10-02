// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;

namespace Azure.Mcp.Tools.StorageSync.Tests.Services;

internal sealed class StorageSyncCreateRequestHandler(string? existingPolicy) : HttpMessageHandler
{
    public int ServiceLookupCount { get; private set; }
    public int CreateCount { get; private set; }
    public string? CreateRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (request.Method == HttpMethod.Get && path.EndsWith("/resourceGroups/testrg", StringComparison.OrdinalIgnoreCase))
        {
            return JsonResponse(HttpStatusCode.OK, """
                {"id":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/testrg","name":"testrg","type":"Microsoft.Resources/resourceGroups","location":"eastus","properties":{"provisioningState":"Succeeded"}}
                """);
        }

        if (path.EndsWith("/providers/Microsoft.StorageSync/storageSyncServices/test-service", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Method == HttpMethod.Get)
            {
                ServiceLookupCount++;
                return existingPolicy switch
                {
                    "AllowAllTraffic" => JsonResponse(HttpStatusCode.OK, """
                        {"id":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/testrg/providers/Microsoft.StorageSync/storageSyncServices/test-service","name":"test-service","type":"Microsoft.StorageSync/storageSyncServices","location":"eastus","properties":{"incomingTrafficPolicy":"AllowAllTraffic"}}
                        """),
                    "AllowVirtualNetworksOnly" => JsonResponse(HttpStatusCode.OK, """
                        {"id":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/testrg/providers/Microsoft.StorageSync/storageSyncServices/test-service","name":"test-service","type":"Microsoft.StorageSync/storageSyncServices","location":"eastus","properties":{"incomingTrafficPolicy":"AllowVirtualNetworksOnly"}}
                        """),
                    _ => JsonResponse(HttpStatusCode.NotFound, """{"error":{"code":"ResourceNotFound","message":"Service not found"}}""")
                };
            }

            if (request.Method == HttpMethod.Put)
            {
                CreateCount++;
                CreateRequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
                return JsonResponse(HttpStatusCode.BadRequest, """{"error":{"code":"BadRequest","message":"Request captured"}}""");
            }
        }

        throw new InvalidOperationException($"Unexpected ARM request: {request.Method} {path}");
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}
