// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Core.Services.Time;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class MonitorTracesServiceTests()
{
    private const string ResourceGroup = "rg";

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    public async Task ListAccountsAsync_PreservesDiscoveryTruncationAfterProbing(int workspaceCount, bool areResultsTruncated)
    {
        using var handler = new MonitorTracesTestHandler(workspaceCount, areResultsTruncated);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        ResourceQueryResults<TraceAccountInfo> result = await service.ListAccountsAsync(
            MonitorTracesTestHandler.Subscription, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(areResultsTruncated, result.AreResultsTruncated);
        Assert.Equal((workspaceCount + 1) / 2, result.Results.Count);
        Assert.All(result.Results, account => Assert.Equal(3600, account.TraceDurationWindowInSeconds));
    }

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("https://amw.data.monitor.azure.com.evil.example.com")]
    [InlineData("http://amw.data.monitor.azure.com")]
    public async Task ListFieldNamesAsync_RejectsEndpointBeforeDataPlaneTokenOrRequest(string endpoint)
    {
        using var handler = new MonitorTracesTestHandler(1, false, endpoint);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);

        await Assert.ThrowsAsync<SecurityException>(() => service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.DataPlaneCalls);
        TokenCredential credential = await azureService.GetTokenCredentialAsync(null, TestContext.Current.CancellationToken);
        await credential.DidNotReceive().GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.Contains(MonitorTracesService.TraceDataPlaneScope)),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("rg-a")]
    [InlineData("rg-b")]
    public async Task ListFieldNamesAsync_UsesScopedWorkspaceAndValidatesEndpoints(string resourceGroup)
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);
        await service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, resourceGroup, "amw0", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(handler.LastGraphQuery);
        Assert.Equal(
            $"/subscriptions/{MonitorTracesTestHandler.Subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Monitor/accounts/amw0",
            handler.LastWorkspaceUri!.AbsolutePath);
        Assert.Contains("api-version=2025-10-03-preview", handler.LastWorkspaceUri.Query);
        await azureService.Received(1).GetResourceGroupResource(
            MonitorTracesTestHandler.Subscription, resourceGroup, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        azureService.Received().ValidateAzureServiceEndpoint(
            "https://amw.data.monitor.azure.com", "monitor-traces");
        azureService.Received(1).ValidateAzureServiceEndpoint(
            handler.LastDataPlaneUri!.AbsoluteUri, "monitor-traces");
        Assert.Equal(1, handler.DataPlaneCalls);
    }

    [Theory]
    [InlineData("/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw0")]
    [InlineData("https://evil.example.com")]
    [InlineData("amw0/traceContainers/default")]
    [InlineData("amw0?api-version=bad")]
    [InlineData("amw0#fragment")]
    [InlineData("amw0\\other")]
    public async Task ListFieldNamesAsync_RejectsNonNameWorkspaceBeforeAzureRequests(string workspace)
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService azureService) = CreateService(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ListFieldNamesAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, workspace, cancellationToken: TestContext.Current.CancellationToken));

        await azureService.DidNotReceive().GetResourceGroupResource(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        Assert.Equal(0, handler.DataPlaneCalls);
    }

    [Theory]
    [InlineData("", "unknown")]
    [InlineData(" ", "unknown")]
    [InlineData("{\"status\":\"Cancelled\"}", "Cancelled")]
    public async Task CancelQueryAsync_DistinguishesAbsentBodyFromStatus(string body, string expectedStatus)
    {
        using var handler = new MonitorTracesTestHandler(1, false, cancelBody: body);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        TraceQueryCancelResult result = await service.CancelQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"status\":")]
    public async Task CancelQueryAsync_RejectsMalformedNonemptyBody(string body)
    {
        using var handler = new MonitorTracesTestHandler(1, false, cancelBody: body);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.CancelQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task QueryAndResume_KeepSubmissionAndPollSettingsSeparate()
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        DateTimeOffset end = DateTimeOffset.Parse("2026-10-12T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        const string query = "Traces | where trace_id == '\"quoted\"'";

        await service.RunQueryAsync(MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", query,
            end.AddMinutes(-30), end, 1000, true, cancellationToken: TestContext.Current.CancellationToken);
        using JsonDocument submit = JsonDocument.Parse(handler.LastSubmitBody!);
        Assert.Equal(query, submit.RootElement.GetProperty("query").GetString());
        Assert.Equal(1000, submit.RootElement.GetProperty("options").GetProperty("maxResults").GetInt32());
        Assert.Contains("top=1000&", handler.LastDataPlaneUri!.Query);

        await service.ResumeQueryAsync(MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            10, false,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.SubmitCalls);
        Assert.Contains("top=10&", handler.LastDataPlaneUri!.Query);
        Assert.Contains("includeSpans=False", handler.LastDataPlaneUri.Query);
        Assert.DoesNotContain("pageSize=", handler.LastDataPlaneUri.Query);
        Assert.Contains("api-version=2026-01-10-preview", handler.LastDataPlaneUri.Query);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task QueryAndResume_DrainAllPagesInOneCall(bool resume, bool finalPageIsEmpty)
    {
        using var firstPage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[{"trace_id":"first","spans":[null,{"duration_nano":9007199254740993,"flags":[true,null]}]}],"nextLink":"https://evil.example/ignored","schema":["trace_id"]}""")
        };
        using var pending = new HttpResponseMessage(HttpStatusCode.Accepted);
        pending.Headers.Add("Retry-After", "1");
        using var secondPage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(finalPageIsEmpty
                ? """{"value":[{"trace_id":"second"}],"nextLink":"/ignored"}"""
                : """{"value":[{"trace_id":"second"}],"metadata":{"RowsFound":2},"isPartialResult":true}""")
        };
        using var finalPage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[],"metadata":{"RowsFound":2}}""")
        };
        using var handler = new MonitorTracesTestHandler(
            1, false, resultPages: [firstPage, pending, secondPage, finalPage]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        DateTimeOffset end = DateTimeOffset.Parse("2026-10-12T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture);

        var result = resume
            ? await service.ResumeQueryAsync(
                MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
                cancellationToken: TestContext.Current.CancellationToken)
            : await service.RunQueryAsync(
                MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "Traces",
                end.AddMinutes(-30), end, 100, true, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("op-1234", result.OperationId);
        Assert.False(result.OutputLimitReached);
        var payload = result.Payload!.Value;
        Assert.Equal(["first", "second"], payload.GetProperty("value").EnumerateArray()
            .Select(row => row.GetProperty("trace_id").GetString()));
        var spans = payload.GetProperty("value")[0].GetProperty("spans");
        Assert.Equal(JsonValueKind.Null, spans[0].ValueKind);
        Assert.Equal(9007199254740993, spans[1].GetProperty("duration_nano").GetInt64());
        Assert.True(spans[1].GetProperty("flags")[0].GetBoolean());
        Assert.Equal(JsonValueKind.Null, spans[1].GetProperty("flags")[1].ValueKind);
        Assert.Equal("trace_id", payload.GetProperty("schema")[0].GetString());
        Assert.Equal(2, payload.GetProperty("metadata").GetProperty("RowsFound").GetInt32());
        Assert.False(payload.TryGetProperty("nextLink", out _));
        if (!finalPageIsEmpty)
        {
            Assert.True(payload.GetProperty("isPartialResult").GetBoolean());
        }
        Assert.Equal(finalPageIsEmpty ? 4 : 3, handler.PollCalls);
        Assert.Equal(resume ? 0 : 1, handler.SubmitCalls);
        Assert.Equal("amw.data.monitor.azure.com", handler.LastDataPlaneUri!.Host);
    }

    [Theory]
    [InlineData("""{"value":[{"trace_id":"last"}]}""")]
    [InlineData("""{"value":[{"trace_id":"last"}],"nextLink":null}""")]
    [InlineData("""{"value":[{"trace_id":"last"}],"nextLink":""}""")]
    [InlineData("""{"value":[null]}""")]
    public async Task ResumeQueryAsync_CompletesNonemptyFinalPage(string body)
    {
        using var expected = JsonDocument.Parse(body);
        using var page = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var result = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Single(result.Payload!.Value.GetProperty("value").EnumerateArray());
        Assert.True(JsonElement.DeepEquals(expected.RootElement.GetProperty("value"), result.Payload.Value.GetProperty("value")));
        Assert.Equal(1, handler.PollCalls);
    }

    [Theory]
    [InlineData("""{"metadata":{}}""")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[],"nextLink":123}""")]
    public async Task ResumeQueryAsync_RejectsInvalidResultShape(string body)
    {
        using var page = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() => service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
    }

    [Fact]
    public async Task ResumeQueryAsync_EmptyPageWithContinuationIsNotComplete()
    {
        using var page = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[],"nextLink":"/ignored"}""")
        };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var result = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Empty(result.Payload!.Value.GetProperty("value").EnumerateArray());
        Assert.Equal(2, handler.PollCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResumeQueryAsync_TimeBudgetPreservesDataAndDoesNotConsumeNextPage(bool hasData)
    {
        using var page = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(hasData
                ? """{"value":[{"trace_id":"first"}],"nextLink":"/ignored","metadata":{"RowsFound":1}}"""
                : """{"value":[],"nextLink":"/ignored","metadata":{"RowsFound":0}}""")
        };
        using var last = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[{"trace_id":"last"}]}""")
        };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page, last]);
        var clock = Substitute.For<IDateTimeProvider>();
        var now = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        clock.UtcNow.Returns(now, now, now.AddSeconds(180));
        (MonitorTracesService service, IAzureService _) = CreateService(handler, clock);

        var result = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.False(result.OutputLimitReached);
        Assert.Equal(hasData ? 1 : 0, result.Payload!.Value.GetProperty("value").GetArrayLength());
        Assert.Equal(hasData ? 1 : 0, result.Payload.Value.GetProperty("metadata").GetProperty("RowsFound").GetInt32());
        Assert.Equal(1, handler.PollCalls);

        var resumed = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", result.OperationId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Succeeded", resumed.Status);
        Assert.Equal("last", resumed.Payload!.Value.GetProperty("value")[0].GetProperty("trace_id").GetString());
        Assert.Equal(2, handler.PollCalls);
        Assert.Equal(0, handler.SubmitCalls);
    }

    [Fact]
    public async Task ResumeQueryAsync_NoDataBeforeDeadlineReturnsResumableOperation()
    {
        using var pending = new HttpResponseMessage(HttpStatusCode.Accepted);
        pending.Headers.Add("Retry-After", "1");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [pending]);
        var clock = Substitute.For<IDateTimeProvider>();
        var now = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        clock.UtcNow.Returns(now, now, now.AddSeconds(180));
        (MonitorTracesService service, IAzureService _) = CreateService(handler, clock);

        var result = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Equal("op-1234", result.OperationId);
        Assert.Null(result.Payload);
        Assert.Equal(1, handler.PollCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task QueryAndResume_RowBudgetPreservesWholeConsumedPage(bool resume)
    {
        string rows = string.Join(",", Enumerable.Range(0, 1001).Select(index => $"{{\"id\":{index}}}"));
        using var page = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"value\":[{rows}],\"nextLink\":\"/ignored\"}}")
        };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);
        var end = DateTimeOffset.UtcNow;

        var result = resume
            ? await service.ResumeQueryAsync(
                MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234", 1,
                cancellationToken: TestContext.Current.CancellationToken)
            : await service.RunQueryAsync(
                MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "Traces",
                end.AddMinutes(-30), end, 10, true, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.True(result.OutputLimitReached);
        Assert.Equal(1001, result.Payload!.Value.GetProperty("value").GetArrayLength());
        Assert.Equal(1000, result.Payload.Value.GetProperty("value")[1000].GetProperty("id").GetInt32());
        Assert.Equal(1, handler.PollCalls);
    }

    [Theory]
    [InlineData(400, """{"error":"query failed"}""", HttpStatusCode.BadRequest)]
    [InlineData(200, """{"value":{}}""", HttpStatusCode.BadGateway)]
    [InlineData(200, """{"value":[],"nextLink":123}""", HttpStatusCode.BadGateway)]
    [InlineData(200, "not-json", HttpStatusCode.BadGateway)]
    public async Task ResumeQueryAsync_LaterFailureRetainsEarlierRows(int status, string body, HttpStatusCode expected)
    {
        using var first = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[{"trace_id":"first"}],"nextLink":"/ignored"}""")
        };
        using var failure = new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body) };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [first, failure]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var error = await Assert.ThrowsAsync<TraceQueryException>(() => service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(expected, error.StatusCode);
        Assert.Equal("Failed", error.PartialResult.Status);
        Assert.Equal("op-1234", error.PartialResult.OperationId);
        Assert.Equal("first", error.PartialResult.Payload!.Value.GetProperty("value")[0].GetProperty("trace_id").GetString());
        Assert.Equal(2, handler.PollCalls);
    }

    [Theory]
    [InlineData("hasLimitReached")]
    [InlineData("isPartialResult")]
    [InlineData("isSystemLimitHit")]
    public async Task ResumeQueryAsync_PreservesPartialFlagsAcrossPages(string flag)
    {
        using var first = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"value\":[1],\"nextLink\":\"/ignored\",\"{flag}\":true}}")
        };
        using var last = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"value\":[2],\"{flag}\":false}}")
        };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [first, last]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var result = await service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.True(result.Payload!.Value.GetProperty(flag).GetBoolean());
        Assert.Equal(2, result.Payload.Value.GetProperty("value").GetArrayLength());
    }

    [Fact]
    public async Task ResumeQueryAsync_InvalidContinuationStillPreservesReceivedRows()
    {
        using var page = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[{"trace_id":"received"}],"nextLink":123}""")
        };
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        (MonitorTracesService service, IAzureService _) = CreateService(handler);

        var error = await Assert.ThrowsAsync<TraceQueryException>(() => service.ResumeQueryAsync(
            MonitorTracesTestHandler.Subscription, ResourceGroup, "amw0", "op-1234",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal("received", error.PartialResult.Payload!.Value.GetProperty("value")[0].GetProperty("trace_id").GetString());
    }

    private static (MonitorTracesService Service, IAzureService AzureService) CreateService(
        MonitorTracesTestHandler handler, IDateTimeProvider? clock = null)
    {
        var armClient = new HttpClient(handler, disposeHandler: false);
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
        AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, armClient, azureService.CloudConfiguration.ArmEnvironment);
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        SubscriptionResource subscription = Substitute.For<SubscriptionResource>();
        subscription.Data.Returns(ResourceManagerModelFactory.SubscriptionData(
            subscriptionId: MonitorTracesTestHandler.Subscription,
            tenantId: Guid.Parse("00000000-0000-0000-0000-000000000002")));
        azureService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetResourceGroupResource(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                string resourceGroup = call.ArgAt<string>(1);
                ResourceGroupResource group = Substitute.For<ResourceGroupResource>();
                group.Id.Returns(new ResourceIdentifier($"/subscriptions/{MonitorTracesTestHandler.Subscription}/resourceGroups/{resourceGroup}"));
                return group;
            });

        TokenCredential credential = Substitute.For<TokenCredential>();
        var token = new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);

        var service = new MonitorTracesService(
            azureService, Substitute.For<ICacheService>(), NullLogger<MonitorTracesService>.Instance,
            clock ?? new DateTimeProvider());
        return (service, azureService);
    }
}
