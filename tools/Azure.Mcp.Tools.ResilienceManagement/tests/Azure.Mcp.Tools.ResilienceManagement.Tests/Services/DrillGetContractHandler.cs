// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

internal sealed class DrillGetContractHandler(string monitoringProperty) : HttpMessageHandler
{
    internal const string DrillId =
        "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/drills/drill1";
    internal const string ModelId =
        "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.CloudHealth/healthModels/model1";
    internal const string SliId =
        "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/slis/sli1";

    internal List<(Uri Uri, string? RoutingLocation, bool HasContent)> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            throw new InvalidOperationException($"Drill GET must not submit {request.Method}.");
        }

        Uri uri = request.RequestUri!;
        Requests.Add((uri,
            request.Headers.TryGetValues("x-ms-rhub-routing-location", out var locations)
                ? locations.Single()
                : null,
            request.Content is not null));
        bool currentContract = System.Web.HttpUtility.ParseQueryString(uri.Query)["api-version"]
            == "2026-10-31-preview";
        var properties = new JsonObject
        {
            ["provisioningState"] = "Succeeded",
            ["executionReadiness"] = "Ready",
            ["monitoringProperties"] = null,
            ["attentionReason"] = new JsonObject
            {
                ["initialConfig"] = "Complete",
                ["includedResourceInDrill"] = "Exists",
            },
        };
        if (currentContract)
        {
            properties["attentionReason"]!["healthModelExists"] = "Exists";
            properties["attentionReason"]!["monitoringSourceNotConfigured"] = false;
            properties[monitoringProperty] = monitoringProperty == "healthModelMonitoringProperties"
                ? new JsonObject
                {
                    ["healthModelId"] = ModelId,
                    ["identity"] = new JsonObject { ["type"] = "SystemAssigned" },
                }
                : new JsonObject
                {
                    ["slis"] = new JsonArray(new JsonObject
                    {
                        ["sliId"] = SliId,
                        ["type"] = "Availability",
                    }),
                    ["identity"] = new JsonObject { ["type"] = "SystemAssigned" },
                };
        }

        string content = new JsonObject
        {
            ["id"] = DrillId,
            ["name"] = "drill1",
            ["type"] = "Microsoft.AzureResilienceManagement/drills",
            ["location"] = "eastus2euap",
            ["properties"] = properties,
        }.ToJsonString();
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        });
    }
}
