// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppLens.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppLens.Services;

/// <summary>
/// Service implementation for AppLens diagnostic operations.
/// </summary>
public class AppLensService : BaseAzureService, IAppLensService
{
    private readonly HttpClient _httpClient;
    private readonly AppLensOptions _options;
    private const string ResourceQuery = """
        resources
        | where name =~ '{0}'
        | project id, subscriptionId, resourceGroup, resourceType = type, resourceKind = kind, resourceName = name
        """;

    public AppLensService(HttpClient httpClient, ITenantService? tenantService = null, ILoggerFactory? loggerFactory = null)
        : base(tenantService, loggerFactory)
    {
        _httpClient = httpClient;
        _options = new AppLensOptions(); // In real implementation, this would come from configuration
    }

    /// <inheritdoc />
    public async Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resourceName,
        string? subscriptionNameOrId = null,
        string? resourceGroup = null,
        string? resourceType = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        // Step 1: Find the resource using Azure Resource Graph
        var findResult = await FindResourceIdAsync(resourceName, subscriptionNameOrId, resourceGroup, resourceType);

        if (findResult is DidNotFindResourceResult notFound)
        {
            throw new InvalidOperationException(notFound.Message);
        }

        var foundResource = (FoundResourceResult)findResult;

        // Step 2: Get AppLens session (simplified - in real implementation this would call the actual AppLens API)
        var session = await GetAppLensSessionAsync(foundResource.ResourceId);

        if (session is FailedAppLensSessionResult failed)
        {
            throw new InvalidOperationException(failed.Message);
        }

        var successfulSession = (SuccessfulAppLensSessionResult)session;

        // Step 3: Ask AppLens the diagnostic question (simplified implementation)
        var insights = await AskAppLensAsync(successfulSession.Session, question);

        return new DiagnosticResult(
            insights.Insights,
            insights.Solutions,
            foundResource.ResourceId,
            foundResource.ResourceTypeAndKind);
    }

    private Task<FindResourceIdResult> FindResourceIdAsync(
        string resourceName,
        string? subscriptionId,
        string? resourceGroup,
        string? resourceType)
    {
        // TODO: Implement Azure Resource Graph query when stable package is available
        // For now, construct a resource ID from the provided information

        if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(resourceGroup))
        {
            return Task.FromResult<FindResourceIdResult>(new DidNotFindResourceResult($"Subscription ID and Resource Group are required to locate resource '{resourceName}'. Please provide both --subscription-name-or-id and --resource-group parameters."));
        }

        // Construct basic resource ID - this is a placeholder implementation
        var resourceId = resourceType switch
        {
            _ when resourceType?.Contains("Microsoft.Web/sites") == true =>
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Web/sites/{resourceName}",
            _ when resourceType?.Contains("Microsoft.Storage/storageAccounts") == true =>
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{resourceName}",
            _ when resourceType?.Contains("Microsoft.Sql/servers") == true =>
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Sql/servers/{resourceName}",
            _ => $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/{resourceType ?? "Microsoft.Resources/resourceGroups"}/{resourceName}"
        };

        return Task.FromResult<FindResourceIdResult>(new FoundResourceResult(resourceId, resourceType ?? "Unknown", null));
    }

    private async Task<GetAppLensSessionResult> GetAppLensSessionAsync(string resourceId)
    {
        try
        {
            // Get Azure credential using BaseAzureService
            var credential = await GetCredential();

            // Get ARM token
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/user_impersonation"]),
                CancellationToken.None);

            // Call the AppLens token endpoint
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://management.azure.com/{resourceId}/detectors/GetToken-db48586f-7d94-45fc-88ad-b30ccd3b571c?api-version=2015-08-01");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new FailedAppLensSessionResult("The specified resource could not be found or does not support diagnostics.");
                }
                return new FailedAppLensSessionResult($"Failed to create diagnostics session: {response.StatusCode}, {response}, {request.RequestUri}, {resourceId}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var sessionData = JsonDocument.Parse(content);

            // Extract session information from the response
            // Note: This is a simplified implementation - the actual response structure would need to be analyzed
            var sessionId = Guid.NewGuid().ToString(); // In real implementation, this would come from the API response
            var sessionToken = sessionData.RootElement.GetProperty("value").GetString() ?? "";

            return new SuccessfulAppLensSessionResult(
                new AppLensSession(sessionId, resourceId, sessionToken, 3600));
        }
        catch (Exception ex)
        {
            return new FailedAppLensSessionResult($"Failed to create AppLens session: {ex.Message}");
        }
    }

    private async Task<(List<string> Insights, List<string> Solutions)> AskAppLensAsync(AppLensSession session, string question)
    {
        // Note: This is a simplified implementation placeholder
        // In the real implementation, this would:
        // 1. Connect to the AppLens SignalR endpoint
        // 2. Send the diagnostic question
        // 3. Receive and process the streaming responses
        // 4. Parse insights and solutions from the response

        await Task.Delay(100); // Simulate API call

        // Return mock data for demonstration
        var insights = new List<string>
        {
            $"Analyzing resource {session.ResourceId} for: {question}",
            "Note: This is a simplified implementation. Full AppLens integration would provide detailed diagnostic insights."
        };

        var solutions = new List<string>
        {
            "To complete this implementation, integrate with the AppLens SignalR endpoint at https://diagnosticschat.azure.com/chatHub",
            "Implement the full conversational diagnostics protocol as documented in the original AppLens plugin"
        };

        return (insights, solutions);
    }
}
