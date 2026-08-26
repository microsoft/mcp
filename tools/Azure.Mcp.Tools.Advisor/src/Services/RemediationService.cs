// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed class RemediationService(IAzureService azureService)
    : BaseAzureService(azureService), IRemediationService
{
    // NOTE: The Microsoft.Advisor/remediationTypes ARM API is a proposed contract and not yet live.
    // Confirm the final api-version with the Advisor / ARM API Modeling team before shipping.
    private const string ApiVersion = "2025-01-01-preview";

    public async Task<RemediationPackage> GetRemediationAsync(
        string recommendationTypeId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(recommendationTypeId), recommendationTypeId));

        var managementEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var url = BuildRemediationUrl(managementEndpoint, recommendationTypeId);

        using var httpClient = AzureService.GetClient();
        var clientOptions = AddDefaultPolicies(new RemediationClientOptions());
        clientOptions.Transport = new HttpClientTransport(httpClient);

        var pipeline = HttpPipelineBuilder.Build(clientOptions);

        var armToken = await GetArmAccessTokenAsync(null, cancellationToken);
        var accessToken = armToken.Token;
        ValidateRequiredParameters((nameof(accessToken), accessToken));

        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Get;
        request.Uri.Reset(new Uri(url));
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        using var response = await pipeline.SendRequestAsync(request, cancellationToken);

        if (!response.IsError)
        {
            var result = JsonSerializer.Deserialize(
                response.Content.ToStream(),
                AdvisorJsonContext.Default.RemediationPackage);
            return result ?? throw new JsonException("Advisor remediation response deserialized to null.");
        }

        throw new HttpRequestException(
            $"Advisor remediation request failed with status {response.Status}: {response.ReasonPhrase}",
            null,
            (HttpStatusCode)response.Status);
    }

    private static string BuildRemediationUrl(string managementEndpoint, string recommendationTypeId)
    {
        var queryString = $"api-version={ApiVersion}";
        return $"{managementEndpoint}/providers/Microsoft.Advisor/remediationTypes/{Uri.EscapeDataString(recommendationTypeId)}?{queryString}";
    }
}
