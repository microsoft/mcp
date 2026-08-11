// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed class RemediationService(IAzureService azureService)
    : BaseAzureService(azureService), IRemediationService
{
    // NOTE: The Microsoft.Advisor/remediationTypes ARM API is a proposed contract and not yet live.
    // Confirm the final api-version with the Advisor / ARM API Modeling team before shipping.
    private const string ApiVersion = "2025-01-01-preview";

    public async Task<RemediationPackage> GetRemediationAsync(
        string recommendationId,
        string[]? artifactTypes = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(recommendationId), recommendationId));

        var managementEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var url = BuildRemediationUrl(managementEndpoint, recommendationId, artifactTypes);

        using var httpClient = AzureService.GetClient();
        var clientOptions = ConfigureRetryPolicy(
            AddDefaultPolicies(new RemediationClientOptions()),
            retryPolicy);
        clientOptions.Transport = new HttpClientTransport(httpClient);

        var pipeline = HttpPipelineBuilder.Build(clientOptions);

        var accessToken = (await GetArmAccessTokenAsync(tenant, cancellationToken)).Token;
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

    private static string BuildRemediationUrl(string managementEndpoint, string recommendationId, string[]? artifactTypes)
    {
        var queryParams = new List<string> { $"api-version={ApiVersion}" };

        if (artifactTypes is { Length: > 0 })
        {
            var joined = string.Join(",", artifactTypes);
            queryParams.Add($"artifactTypes={Uri.EscapeDataString(joined)}");
        }

        var queryString = string.Join("&", queryParams);
        return $"{managementEndpoint}/providers/Microsoft.Advisor/remediationTypes/{Uri.EscapeDataString(recommendationId)}?{queryString}";
    }
}
