// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

internal sealed class GoalResourceContractHandler(bool includeRegional) : HttpMessageHandler
{
    internal const string ResourceName = "11111111-1111-1111-1111-111111111111";
    internal const string ResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/goalAssignments/goal1/goalResources/" + ResourceName;
    internal List<Uri> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri!);
        bool currentContract = System.Web.HttpUtility.ParseQueryString(request.RequestUri!.Query)["api-version"] == "2026-10-31-preview";
        string regional = currentContract && includeRegional
            ? ""","regionalResiliency":{"goalParticipation":"Included","attestationStatus":"NotAttested","userConfirmation":null}"""
            : string.Empty;
        string body = $$"""
            {"id":"{{ResourceId}}","name":"{{ResourceName}}","type":"Microsoft.AzureResilienceManagement/goalAssignments/goalResources",
             "properties":{"provisioningState":"Succeeded",
             "resourceArmId":"/subscriptions/sub/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/workload",
             "zonalResiliency":{"goalParticipation":"Excluded","attestationStatus":"NotAttested","userConfirmation":[]}{{regional}} } }
            """;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
    }
}
