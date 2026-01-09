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
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AppLens.Services;

/// <summary>
/// Service implementation for AppLens diagnostic operations.
/// </summary>
public class AppLensService(IHttpClientService httpClientService, ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IAppLensService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly AppLensOptions _options = new AppLensOptions();
    private const string ConversationalDiagnosticsSignalREndpoint = "https://diagnosticschat.azure.com/chatHub";

    /// <inheritdoc />
    public async Task<AppLensInsights> DiagnoseResourceAsync(
        string question,
        string resource,
        string subscription,
        string? resourceGroup = null,
        string? resourceType = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, cancellationToken: cancellationToken);
        // Step 1: Get the resource ID
        var findResult = await FindResourceIdAsync(resource, subscriptionResource.Data.SubscriptionId, resourceGroup, resourceType, cancellationToken);

        if (findResult is Failure<FoundResourceData> notFoundResult)
        {
            throw new InvalidOperationException(notFoundResult.Message);
        }

        var successfulResult = (Success<FoundResourceData>)findResult;
        var resourceData = successfulResult.Data;

        // Step 2: Get AppLens session
        var getSessionResult = await GetAppLensSessionAsync(resourceData.ResourceId, tenantId, cancellationToken);

        if (getSessionResult is Failure<AppLensSession> failed)
        {
            throw new InvalidOperationException(failed.Message);
        }

        var successResult = (Success<AppLensSession>)getSessionResult;

        // Step 3: Ask AppLens the diagnostic question
        var insights = await CollectInsightsAsync(successResult.Data, question, cancellationToken);

        return new AppLensInsights(
            insights.Insights,
            insights.Solutions,
            resourceData.ResourceId,
            resourceData.ResourceTypeAndKind);
    }

    private Task<Result<FoundResourceData>> FindResourceIdAsync(
        string resource,
        string? subscription,
        string? resourceGroup,
        string? resourceType,
        CancellationToken cancellationToken)
    {
        // Construct a resource ID from the provided information

        if (string.IsNullOrEmpty(subscription) || string.IsNullOrEmpty(resourceGroup))
        {
            return Task.FromResult<Result<FoundResourceData>>(Result<FoundResourceData>.Failure($"Subscription ID and Resource Group are required to locate resource '{resource}'. Please provide both --subscription-name-or-id and --resource-group parameters."));
        }

        var resourceId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resource}";

        return Task.FromResult<Result<FoundResourceData>>(Result<FoundResourceData>.Success(new FoundResourceData(resourceId, resourceType ?? "Unknown", null)));
    }

    private async Task<Result<AppLensSession>> GetAppLensSessionAsync(string resourceId, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get Azure credential using BaseAzureService
            var credential = await GetCredential(tenantId, cancellationToken);

            // Get ARM token
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/user_impersonation"]),
                cancellationToken);

            // Call the AppLens token endpoint
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://management.azure.com/{resourceId}/detectors/GetToken-db48586f-7d94-45fc-88ad-b30ccd3b571c?api-version=2015-08-01");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            using var response = await _httpClientService.DefaultClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Result<AppLensSession>.Failure("The specified resource could not be found or does not support diagnostics.");
                }
                return Result<AppLensSession>.Failure($"Failed to create diagnostics session for resource {resourceId}, http response code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            AppLensSession? appLensSession;
            appLensSession = ParseGetTokenResponse(content);

            return Result<AppLensSession>.Success(
                appLensSession with { ResourceId = resourceId });
        }
        catch (Exception ex)
        {
            return Result<AppLensSession>.Failure($"Failed to create AppLens session: {ex.Message}");
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
            .AddJsonProtocol(options =>
            {
                // TypeInfo resolver chain needs to be update to use AppLensJsonContext first to resolve App Lens
                // types. Without this it'll attempt to use reflection-based serialization, which will result in a
                // runtime error when using the native AoT server, as that disables reflection.
                // More information about configuring HubConnection JSON serialization can be found here:
                // https://learn.microsoft.com/aspnet/core/signalr/configuration?view=aspnetcore-9.0&tabs=dotnet#jsonmessagepack-serialization-options
                options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppLensJsonContext.Default);
            })
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
            await channel.Writer.WriteAsync(responseBody, cancellationToken);
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

            await channel.Writer.WriteAsync(responseBody, cancellationToken);
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
    private async Task<AppLensInsights> CollectInsightsAsync(AppLensSession session, string question, CancellationToken cancellationToken)
    {
        var insights = new List<string>();
        var solutions = new List<string>();

        await foreach (var message in AskAppLensAsync(session, question, cancellationToken))
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

        return new AppLensInsights(insights, solutions, session.ResourceId, "Resource");
    }

    private static AppLensSession ParseGetTokenResponse(string rawResponse)
    {
        using var jsonDoc = JsonDocument.Parse(rawResponse);

        AppLensSession? session = null;

        // parse response
        const int SessionIdColumnIndex = 0;
        const int TokenColumnIndex = 1;
        const int ExpiresInColumnIndex = 2;

        if (!jsonDoc.RootElement.TryGetProperty("properties", out JsonElement propertiesElement))
        {
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
            {
                IEnumerable<string> propertyNames = jsonDoc.RootElement.EnumerateObject().Select(property => property.Name);
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
