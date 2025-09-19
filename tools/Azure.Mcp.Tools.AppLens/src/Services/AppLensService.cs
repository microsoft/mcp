// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.AppLens.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Azure.Mcp.Tools.AppLens.Services;

/// <summary>
/// Service implementation for AppLens diagnostic operations.
/// </summary>
public class AppLensService(IHttpClientService httpClientService, ISubscriptionService subscriptionService, ITenantService? tenantService = null) : BaseAzureService(tenantService), IAppLensService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly AppLensOptions _options = new AppLensOptions();
    private const string ConversationalDiagnosticsSignalREndpoint = "https://diagnosticschat.azure.com/chatHub";

    /// <inheritdoc />
    public async Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resource,
        string subscription,
        string? resourceGroup = null,
        string? resourceType = null,
        string? tenantId = null)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId);
        // Step 1: Get the resource ID
        var findResult = await FindResourceIdAsync(resource, subscriptionResource.Data.SubscriptionId, resourceGroup, resourceType);

        if (findResult is DidNotFindResourceResult notFound)
        {
            throw new InvalidOperationException(notFound.Message);
        }

        var foundResource = (FoundResourceResult)findResult;

        // Step 2: Get AppLens session
        var session = await GetAppLensSessionAsync(foundResource.ResourceId, tenantId);

        if (session is FailedAppLensSessionResult failed)
        {
            throw new InvalidOperationException(failed.Message);
        }

        var successfulSession = (SuccessfulAppLensSessionResult)session;

        // Step 3: Ask AppLens the diagnostic question
        var insights = await CollectInsightsAsync(successfulSession.Session, question);

        return new DiagnosticResult(
            insights.Insights,
            insights.Solutions,
            foundResource.ResourceId,
            foundResource.ResourceTypeAndKind);
    }

    private Task<FindResourceIdResult> FindResourceIdAsync(
        string resource,
        string? subscription,
        string? resourceGroup,
        string? resourceType)
    {
        // Construct a resource ID from the provided information

        if (string.IsNullOrEmpty(subscription) || string.IsNullOrEmpty(resourceGroup))
        {
            return Task.FromResult<FindResourceIdResult>(new DidNotFindResourceResult($"Subscription ID and Resource Group are required to locate resource '{resource}'. Please provide both --subscription-name-or-id and --resource-group parameters."));
        }

        var resourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resource}";

        return Task.FromResult<FindResourceIdResult>(new FoundResourceResult(resourceId, resourceType ?? "Unknown", null));
    }

    private async Task<GetAppLensSessionResult> GetAppLensSessionAsync(string resourceId, string? tenantId = null)
    {
        try
        {
            // Get Azure credential using BaseAzureService
            var credential = await GetCredential(tenantId);

            // Get ARM token
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/user_impersonation"]),
                CancellationToken.None);

            // Call the AppLens token endpoint
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://management.azure.com/{resourceId}/detectors/GetToken-db48586f-7d94-45fc-88ad-b30ccd3b571c?api-version=2015-08-01");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            var response = await _httpClientService.DefaultClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new FailedAppLensSessionResult("The specified resource could not be found or does not support diagnostics.");
                }
                return new FailedAppLensSessionResult($"Failed to create diagnostics session for resource {resourceId}, http response code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            AppLensSession? appLensSession;
            appLensSession = ParseGetTokenResponse(content);

            return new SuccessfulAppLensSessionResult(
                appLensSession with { ResourceId = resourceId });
        }
        catch (Exception ex)
        {
            return new FailedAppLensSessionResult($"Failed to create AppLens session: {ex.Message}");
        }
    }

    /// <summary>
    /// Asks the AppLens API a single <paramref name="question"/> about a resource associated with the given <paramref name="session"/> and returns the response as stream of asynchronous messages.
    /// </summary>
    /// <param name="question">The question or query to pose to AppLens.</param>
    /// <param name="session">The <see cref="AppLensSession"/> representing the overall conversation.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    public async IAsyncEnumerable<ChatMessageResponseBody> AskAppLensAsync(
        AppLensSession session,
        string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // agent using signal r client to stream answers
        Channel<ChatMessageResponseBody> channel = Channel.CreateUnbounded<ChatMessageResponseBody>(new()
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

        await using HubConnection connection = new HubConnectionBuilder()
            .WithUrl(ConversationalDiagnosticsSignalREndpoint, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(session.Token)!;
                options.Headers.Add("origin", "https://appservice-diagnostics.trafficmanager.net");
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync(cancellationToken);

        connection.On<string>("MessageReceived", async (response) =>
        {
            ChatMessageResponseBody responseBody = ChatMessageResponseBody.FromJson(response);
            await channel.Writer.WriteAsync(responseBody);
        });

        connection.On<string>("MessageCancelled", async (response) =>
        {
            // If the API cancels the request just treat it as the conversation being complete.
            ChatMessageResponseBody responseBody = new()
            {
                Error = response,
                SessionId = "",
                Message = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayMessage = "Request cancelled",
                    Message = "Request cancelled",
                    MessageDisplayDate = DateTime.UtcNow.ToString("O"),
                    UserFeedback = ""
                },
                SuggestedPrompts = [],
                DebugTrace = "",
                ResponseStatus = QueryResponseStatus.Finished,
                ResponseType = MessageResponseType.SystemMessage
            };

            await channel.Writer.WriteAsync(responseBody);
        });

        bool completed = false;

        // Read the token so we can get the user ID
        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(session.Token);
        string userId = jsonToken.Claims.First(claim => claim.Type == "user").Value;

        // fill this from context and request
        ChatMessageRequestBody appLensRequest = new()
        {
            ResourceId = session.ResourceId,
            SessionId = session.SessionId,
            Message = question,
            UserId = userId,
            ResourceKind = "app",
            StartTime = "",
            EndTime = "",
            IsDiagnosticsCall = true,
            ClientName = _options.ClientName
        };

        // fire request
        Task request = connection.InvokeAsync("sendMessage", JsonSerializer.Serialize(appLensRequest, AppLensJsonContext.Default.ChatMessageRequestBody), cancellationToken: cancellationToken);

        bool waitForRequestToFinish = true;
        while (!completed)
        {
            // Ending session if we don't get any messages for some period of time.
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.MessageTimeOutInSeconds));

            try
            {
                await channel.Reader.WaitToReadAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Cancellation has been triggered by the timeout waiting for the service to respond. Finish up as if it had completed normally
                // rather than let the exception propagate.
                completed = true;
                // We're bailing out early, so we don't need to wait for the request to finish.
                waitForRequestToFinish = false;
            }

            while (channel.Reader.TryRead(out ChatMessageResponseBody? message))
            {
                completed |= message.ResponseStatus == QueryResponseStatus.Finished;

                yield return message;
            }
        }

        if (waitForRequestToFinish || request.IsFaulted)
        {
            await request;
        }

        await connection.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Collects insights from AppLens diagnostic conversation.
    /// </summary>
    /// <param name="session">The AppLens session.</param>
    /// <param name="question">The diagnostic question.</param>
    /// <returns>A task containing diagnostic insights and solutions.</returns>
    private async Task<DiagnosticResult> CollectInsightsAsync(AppLensSession session, string question)
    {
        var insights = new List<string>();
        var solutions = new List<string>();

        await foreach (var message in AskAppLensAsync(session, question))
        {
            if (message.ResponseType == MessageResponseType.SystemMessage && !string.IsNullOrEmpty(message.Message?.Message))
            {
                insights.Add(message.Message.Message);
            }

            if (message.ResponseType == MessageResponseType.LoadingMessage && !string.IsNullOrEmpty(message.Message?.Message))
            {
                solutions.Add(message.Message.Message);
            }
        }

        return new DiagnosticResult(insights, solutions, session.ResourceId, "Resource");
    }

    private static AppLensSession ParseGetTokenResponse(string rawResponse)
    {
        JsonDocument jsonResponse = JsonDocument.Parse(rawResponse);

        AppLensSession? session = null;

        // parse response
        const int SessionIdColumnIndex = 0;
        const int TokenColumnIndex = 1;
        const int ExpiresInColumnIndex = 2;

        if (!jsonResponse.RootElement.TryGetProperty("properties", out JsonElement propertiesElement))
        {
            if (jsonResponse.RootElement.ValueKind == JsonValueKind.Object)
            {
                IEnumerable<string> propertyNames = jsonResponse.RootElement.EnumerateObject().Select(property => property.Name);
                string joinedPropertyNames = string.Join(", ", propertyNames);
                throw new Exception($"The top-level property named 'properties' not found. The actual top-level properties are: {joinedPropertyNames}.");
            }

            throw new Exception("'properties' not found. Root element is not a JSON object.");
        }

        if (!propertiesElement.TryGetProperty("dataset", out JsonElement datasetElement))
        {
            throw new Exception("'properties.dataset' not found.");
        }

        foreach (JsonElement datasetItem in datasetElement.EnumerateArray())
        {
            if (!datasetItem.TryGetProperty("table", out JsonElement tableElement))
            {
                throw new Exception("'properties.dataset.table' not found.");
            }

            if (!tableElement.TryGetProperty("rows", out JsonElement rowsElement))
            {
                throw new Exception("'properties.dataset.table.rows' not found.");
            }

            foreach (JsonElement rowJsonElement in rowsElement.EnumerateArray())
            {
                JsonElement[] row = rowJsonElement.Deserialize(AppLensJsonContext.Default.JsonElementArray)!;

                session = new(
                    SessionId: row[SessionIdColumnIndex].GetString()!,
                    ResourceId: "", // Filled in by the caller
                    Token: row[TokenColumnIndex].GetString()!,
                    ExpiresIn: row[ExpiresInColumnIndex].GetInt32()
                );

                break;
            }
        }

        if (session is null)
        {
            throw new Exception("session is null.");
        }

        return session;
    }
}
