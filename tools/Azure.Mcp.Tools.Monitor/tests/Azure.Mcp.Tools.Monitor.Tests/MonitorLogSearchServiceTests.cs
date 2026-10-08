// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Models.Log;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests;

public sealed class MonitorLogSearchServiceTests
{
    private const string Subscription = "subscription";
    private const string SubscriptionId = "11111111-1111-1111-1111-111111111111";
    private const string ResourceGroup = "resource-group";
    private const string Workspace = "workspace";
    private const string WorkspaceId = "22222222-2222-2222-2222-222222222222";
    private const string Table = "McpBasic_CL";
    private const string Query = "| where Message has 'test'";
    private const string Timespan = "P1D";

    [Fact]
    public async Task SearchWorkspaceLogs_SendsExpectedAuthenticatedRequestAndPreservesCellTypes()
    {
        var fixture = CreateFixture(
            "Auxiliary",
            DataResponse("""
                {
                  "tables": [{
                    "name": "PrimaryResult",
                    "columns": [
                      { "name": "Count", "type": "long" },
                      { "name": "Enabled", "type": "bool" },
                      { "name": "Message", "type": "string" }
                    ],
                    "rows": [[42, true, null]]
                  }]
                }
                """));

        var result = await fixture.SearchAsync(
            TestContext.Current.CancellationToken,
            timespan: "2026-09-02T00:00:00Z/2026-09-03T00:00:00Z",
            limit: 7,
            tenant: "tenant");

        Assert.Equal("Auxiliary", result.Plan);
        Assert.Equal(1, result.RowCount);
        Assert.Equal(["Count", "Enabled", "Message"], result.Columns.Select(column => column.Name).ToArray());
        Assert.Equal(["long", "bool", "string"], result.Columns.Select(column => column.Type).ToArray());
        Assert.Equal(JsonValueKind.Number, result.Rows[0][0].ValueKind);
        Assert.Equal(42, result.Rows[0][0].GetInt32());
        Assert.Equal(JsonValueKind.True, result.Rows[0][1].ValueKind);
        Assert.Equal(JsonValueKind.Null, result.Rows[0][2].ValueKind);
        Assert.False(result.IsPartial);
        Assert.Null(result.Error);

        Assert.Equal(1, fixture.DataHandler.CallCount);
        Assert.Equal(2, fixture.ArmHandler.CallCount);
        Assert.Contains(
            fixture.ArmHandler.RequestUris,
            uri => uri.AbsolutePath.Contains(
                $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}/providers/Microsoft.OperationalInsights/workspaces/{Workspace}",
                StringComparison.OrdinalIgnoreCase));
        Assert.Equal(HttpMethod.Post, fixture.DataHandler.Method);
        Assert.Equal(
            $"https://api.loganalytics.io/v1/workspaces/{WorkspaceId}/search?timespan=2026-09-02T00%3A00%3A00Z%2F2026-09-03T00%3A00%3A00Z",
            fixture.DataHandler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", fixture.DataHandler.AuthorizationScheme);
        Assert.Equal("logs-token", fixture.DataHandler.AuthorizationParameter);
        Assert.Equal("application/json", fixture.DataHandler.ContentType);
        Assert.Equal("wait=180", fixture.DataHandler.Prefer);
        using var body = JsonDocument.Parse(fixture.DataHandler.RequestBody!);
        Assert.Equal(
            $"{Table} {Query}\n| take 7",
            body.RootElement.GetProperty("query").GetString());

        Assert.NotNull(fixture.LogsCredential.Scopes);
        Assert.Equal(
            "https://api.loganalytics.io/.default",
            Assert.Single(fixture.LogsCredential.Scopes));
        Assert.Equal(
            TestContext.Current.CancellationToken,
            fixture.LogsCredential.CancellationToken);
    }

    [Theory]
    [InlineData("Basic")]
    [InlineData("Auxiliary")]
    public async Task SearchWorkspaceLogs_AcceptsSupportedPlans(string plan)
    {
        var fixture = CreateFixture(plan, DataResponse("""{"tables":[]}"""));

        var result = await fixture.SearchAsync(TestContext.Current.CancellationToken);

        Assert.Equal(plan, result.Plan);
        Assert.Equal(0, result.RowCount);
        Assert.False(result.IsPartial);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_UnsupportedCloud_FailsBeforeAzureRequests()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""{"tables":[]}"""),
            cloud: AzureCloudConfiguration.AzureCloud.AzureChinaCloud);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotImplemented, exception.StatusCode);
        Assert.Equal("UnsupportedCloud", exception.Code);
        Assert.Equal(0, fixture.ArmHandler.CallCount);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Theory]
    [InlineData("Basic", null)]
    [InlineData("Basic", "")]
    [InlineData("Auxiliary", null)]
    [InlineData("Auxiliary", "   ")]
    public async Task SearchWorkspaceLogs_RejectsMissingOrBlankLastPlanModifiedDate(
        string plan,
        string? lastPlanModified)
    {
        var fixture = CreateFixture(
            plan,
            DataResponse("""{"tables":[]}"""),
            lastPlanModified);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("InvalidTableMetadata", exception.Code);
        Assert.Contains("metadata was incomplete", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsMalformedLastPlanModifiedDate()
    {
        var fixture = CreateFixture(
            "Auxiliary",
            DataResponse("""{"tables":[]}"""),
            "not-a-timestamp");

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("InvalidTableMetadata", exception.Code);
        Assert.Contains("transition metadata was invalid", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Theory]
    [InlineData("2000-01-01T00:00:00Z")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-timestamp")]
    public async Task SearchWorkspaceLogs_RejectsAnalyticsBeforeDataRequest(string? lastPlanModified)
    {
        var fixture = CreateFixture("Analytics", DataResponse("""{"tables":[]}"""), lastPlanModified);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("UnsupportedTablePlan", exception.Code);
        Assert.Contains("monitor_workspace_log_query", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsMissingTablePlan()
    {
        var fixture = CreateFixture(string.Empty, DataResponse("""{"tables":[]}"""));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("InvalidTableMetadata", exception.Code);
        Assert.Contains("metadata was incomplete", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsMoreThanThirtyDaysBeforeAzureRequests()
    {
        var fixture = CreateFixture("Auxiliary", DataResponse("""{"tables":[]}"""));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken, timespan: "P31D"));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("30 days", exception.Message);
        Assert.Equal(0, fixture.ArmHandler.CallCount);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsInvalidLimitBeforeAzureRequests()
    {
        var fixture = CreateFixture("Basic", DataResponse("""{"tables":[]}"""));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken, limit: 101));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("between 1 and 100", exception.Message);
        Assert.Equal(0, fixture.ArmHandler.CallCount);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsBasicRangeStartingMoreThanThirtyDaysAgo()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""{"tables":[]}"""),
            "1990-01-01T00:00:00Z");

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(
                TestContext.Current.CancellationToken,
                timespan: "2000-01-01T00:00:00Z/2000-01-02T00:00:00Z"));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("BasicTimespanTooOld", exception.Code);
        Assert.Contains("cannot start more than 30 days ago", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsRangeCrossingPlanTransition()
    {
        var fixture = CreateFixture(
            "Auxiliary",
            DataResponse("""{"tables":[]}"""),
            "2026-09-02T12:00:00Z");

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(
                TestContext.Current.CancellationToken,
                timespan: "2026-09-02T00:00:00Z/2026-09-03T00:00:00Z"));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("TablePlanTransition", exception.Code);
        Assert.Contains("split", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RejectsRangeWhollyBeforePlanTransition()
    {
        var fixture = CreateFixture(
            "Auxiliary",
            DataResponse("""{"tables":[]}"""),
            "2026-09-03T00:00:00Z");

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(
                TestContext.Current.CancellationToken,
                timespan: "2026-09-01T00:00:00Z/2026-09-02T00:00:00Z"));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("HistoricalTablePlanRange", exception.Code);
        Assert.Contains("entirely before", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_NoContent_ReturnsCompleteEmptyResult()
    {
        var fixture = CreateFixture(
            "Basic",
            () => new HttpResponseMessage(HttpStatusCode.NoContent));

        var result = await fixture.SearchAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
        Assert.Equal(0, result.RowCount);
        Assert.False(result.IsPartial);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_PartialError_ReturnsRowsAndExplicitError()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""
                {
                  "tables": [{
                    "name": "PrimaryResult",
                    "columns": [{ "name": "Value", "type": "long" }],
                    "rows": [[1]]
                  }],
                  "error": {
                    "code": "PartialError",
                    "message": "backend details",
                    "details": [{ "code": "Service.Code", "message": "sensitive details" }]
                  }
                }
                """));

        var result = await fixture.SearchAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsPartial);
        Assert.Equal(1, result.RowCount);
        Assert.Equal("PartialError", result.Error?.Code);
        Assert.Equal("Service.Code", result.Error?.Details.Single().Code);
        Assert.DoesNotContain("sensitive", result.Error?.Details.Single().Message);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_EmptyPartialResult_PreservesError()
    {
        var fixture = CreateFixture("Basic", DataResponse(
            """{"tables":[],"error":{"code":"PartialError","details":[]}}"""));

        var result = await fixture.SearchAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
        Assert.Equal(0, result.RowCount);
        Assert.True(result.IsPartial);
        Assert.Equal("PartialError", result.Error?.Code);
    }

    [Theory]
    [InlineData("""{"tables":[{"name":"First"},{"name":"Second"}]}""")]
    [InlineData("""{"tables":[{"name":"PrimaryResult"},{"name":"PrimaryResult"}]}""")]
    public async Task SearchWorkspaceLogs_AmbiguousResultTables_ReturnsBadGateway(string body)
    {
        var fixture = CreateFixture("Basic", DataResponse(body));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(
            () => fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("MalformedLogsResponse", exception.Code);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_FatalEmbeddedError_ThrowsWithoutResult()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""
                {
                  "tables": [],
                  "error": { "code": "SemanticError", "message": "backend details" }
                }
                """));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("FatalLogsError", exception.Code);
        Assert.Contains("SemanticError", exception.Message);
        Assert.DoesNotContain("backend details", exception.Message);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_MalformedResponse_ThrowsBadGateway()
    {
        var fixture = CreateFixture("Basic", DataResponse("{not-json"));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("MalformedLogsResponse", exception.Code);
        Assert.Contains("malformed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_RowWidthMismatch_ThrowsBadGateway()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""
                {
                  "tables": [{
                    "name": "PrimaryResult",
                    "columns": [
                      { "name": "First", "type": "string" },
                      { "name": "Second", "type": "string" }
                    ],
                    "rows": [["only-one-cell"]]
                  }]
                }
                """));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("InvalidRowShape", exception.Code);
        Assert.Contains("row shape", exception.Message);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_ResponseOverOneMiB_ThrowsPayloadTooLarge()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse(new string('x', (1024 * 1024) + 1)));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, exception.StatusCode);
        Assert.Equal("ResponseTooLarge", exception.Code);
        Assert.Contains("1 MiB", exception.Message);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_ThrottlePreservesRetryAfterAndDoesNotRetry()
    {
        var fixture = CreateFixture(
            "Basic",
            () =>
            {
                var response = DataResponse(
                    """{"code":"TooManyRequests","message":"backend details"}""",
                    (HttpStatusCode)429)();
                response.Headers.RetryAfter = new(TimeSpan.FromSeconds(17));
                return response;
            });

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Contains("17 seconds", exception.Message);
        Assert.DoesNotContain("backend details", exception.Message);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Theory]
    [InlineData(false, "Retry later.")]
    [InlineData(true, "Retry after 0 seconds.")]
    public async Task SearchWorkspaceLogs_ThrottleWithMissingOrPastRetryDate_ReturnsGuidance(
        bool includePastDate,
        string expectedGuidance)
    {
        var fixture = CreateFixture("Basic", () =>
        {
            var response = DataResponse("""{"code":"TooManyRequests"}""", (HttpStatusCode)429)();
            if (includePastDate)
            {
                response.Headers.RetryAfter = new(DateTimeOffset.UtcNow.AddMinutes(-1));
            }

            return response;
        });

        var exception = await Assert.ThrowsAsync<CommandValidationException>(
            () => fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Contains(expectedGuidance, exception.Message);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_GatewayTimeoutDoesNotRetry()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse(
                """{"code":"GatewayTimeout","message":"backend details"}""",
                HttpStatusCode.GatewayTimeout));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.GatewayTimeout, exception.StatusCode);
        Assert.Contains("shorter timespan", exception.Message);
        Assert.DoesNotContain("backend details", exception.Message);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_BodyReadTimeout_ReturnsStructuredGatewayTimeoutAndDisposesResponse()
    {
        var content = new TrackingHttpContent(new TimeoutCancellationStream());
        var fixture = CreateFixture(
            "Basic",
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.GatewayTimeout, exception.StatusCode);
        Assert.Equal("LogsSearchTimeout", exception.Code);
        Assert.Contains("shorter timespan", exception.Message);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_CallerCancellationDuringBodyRead_Propagates()
    {
        using var cancellationSource = new CancellationTokenSource();
        var stream = new BlockingUntilCanceledStream(cancellationSource.Cancel);
        var content = new TrackingHttpContent(stream);
        var fixture = CreateFixture(
            "Basic",
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fixture.SearchAsync(cancellationSource.Token));

        Assert.True(cancellationSource.IsCancellationRequested);
        Assert.True(exception.CancellationToken.IsCancellationRequested);
        Assert.True(stream.ReadStarted);
        Assert.True(stream.ObservedCancellation);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_PropagatesCancellationToHttpSend()
    {
        using var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;
        var fixture = CreateFixture(
            "Basic",
            (_, requestToken) =>
            {
                cancellationSource.Cancel();
                Assert.True(requestToken.IsCancellationRequested);
                return Task.FromCanceled<HttpResponseMessage>(requestToken);
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fixture.SearchAsync(token));

        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound, "WorkspaceNotFound", "workspace was not found")]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, "MetadataAuthenticationFailed", "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, "MetadataAuthorizationFailed", "Authorization failed")]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict, "MetadataConflict", "conflict")]
    [InlineData((HttpStatusCode)429, (HttpStatusCode)429, "MetadataThrottled", "throttled")]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, "MetadataRequestFailed", "could not complete")]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, "MetadataRequestRejected", "rejected")]
    public async Task SearchWorkspaceLogs_WorkspaceMetadataFailure_IsMappedSafely(
        HttpStatusCode workspaceStatus,
        HttpStatusCode expectedStatus,
        string expectedCode,
        string expectedMessageFragment)
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""{"tables":[]}"""),
            workspaceStatus: workspaceStatus);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(expectedStatus, exception.StatusCode);
        Assert.Equal(expectedCode, exception.Code);
        Assert.Contains(expectedMessageFragment, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sensitive ARM details", exception.Message);
        Assert.DoesNotContain(
            fixture.ArmHandler.RequestUris,
            uri => uri.AbsolutePath.Contains("/tables/", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "TableNotFound", "table was not found")]
    [InlineData(HttpStatusCode.Forbidden, "MetadataAuthorizationFailed", "Authorization failed")]
    public async Task SearchWorkspaceLogs_TableMetadataFailure_IsMappedSafelyAndStopsBeforeData(
        HttpStatusCode tableStatus,
        string expectedCode,
        string expectedMessageFragment)
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse("""{"tables":[]}"""),
            tableStatus: tableStatus);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(tableStatus, exception.StatusCode);
        Assert.Equal(expectedCode, exception.Code);
        Assert.Contains(expectedMessageFragment, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sensitive ARM details", exception.Message);
        Assert.Contains(
            fixture.ArmHandler.RequestUris,
            uri => uri.AbsolutePath.Contains("/tables/", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_DataQueryForbidden_IsSanitizedAndDoesNotRetry()
    {
        var fixture = CreateFixture(
            "Basic",
            DataResponse(
                """{"code":"AccessDenied","message":"sensitive downstream details"}""",
                HttpStatusCode.Forbidden));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("AccessDenied", exception.Code);
        Assert.Equal("Authorization failed for the Logs data query.", exception.Message);
        Assert.DoesNotContain("sensitive downstream details", exception.Message);
        Assert.Equal(1, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_TenantResolutionFailure_IsSanitized()
    {
        var fixture = CreateFixture("Basic", DataResponse("""{"tables":[]}"""));
        fixture.AzureService.ResolveTenantIdAsync(
                "tenant",
                Arg.Any<CancellationToken>())
            .Returns<Task<string?>>(_ => throw new RequestFailedException(
                403,
                "sensitive tenant details",
                "TenantDenied",
                null));

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken, tenant: "tenant"));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Equal("TenantResolutionFailed", exception.Code);
        Assert.DoesNotContain("sensitive tenant details", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    [Fact]
    public async Task SearchWorkspaceLogs_CredentialFailure_IsSanitized()
    {
        var fixture = CreateFixture("Basic", DataResponse("""{"tables":[]}"""));
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>())
            .Returns<ValueTask<AccessToken>>(_ =>
                throw new AuthenticationFailedException("sensitive credential details"));
        fixture.AzureService.GetTokenCredentialAsync(
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(credential);

        var exception = await Assert.ThrowsAsync<CommandValidationException>(() =>
            fixture.SearchAsync(TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("LogsAuthenticationFailed", exception.Code);
        Assert.DoesNotContain("sensitive credential details", exception.Message);
        Assert.Equal(0, fixture.DataHandler.CallCount);
    }

    private static ServiceFixture CreateFixture(
        string plan,
        Func<HttpResponseMessage> responseFactory,
        string? lastPlanModified = "2000-01-01T00:00:00Z",
        HttpStatusCode workspaceStatus = HttpStatusCode.OK,
        HttpStatusCode tableStatus = HttpStatusCode.OK,
        AzureCloudConfiguration.AzureCloud cloud = AzureCloudConfiguration.AzureCloud.AzurePublicCloud) =>
        CreateFixture(
            plan,
            (_, _) => Task.FromResult(responseFactory()),
            lastPlanModified,
            workspaceStatus,
            tableStatus,
            cloud);

    private static ServiceFixture CreateFixture(
        string plan,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory,
        string? lastPlanModified = "2000-01-01T00:00:00Z",
        HttpStatusCode workspaceStatus = HttpStatusCode.OK,
        HttpStatusCode tableStatus = HttpStatusCode.OK,
        AzureCloudConfiguration.AzureCloud cloud = AzureCloudConfiguration.AzureCloud.AzurePublicCloud)
    {
        var armHandler = new CapturingHttpMessageHandler((request, _) =>
            Task.FromResult(CreateArmResponse(
                request.RequestUri!,
                plan,
                lastPlanModified,
                workspaceStatus,
                tableStatus)));
        var armCredential = Substitute.For<TokenCredential>();
        armCredential.GetToken(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>())
            .Returns(new AccessToken("arm-token", DateTimeOffset.UtcNow.AddHours(1)));
        armCredential.GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("arm-token", DateTimeOffset.UtcNow.AddHours(1))));
        var armOptions = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(armHandler))
        };

        // Keep transient-failure mappings deterministic and fast: assert the first response, never a retry.
        armOptions.Retry.MaxRetries = 0;
        var armClient = new ArmClient(armCredential, SubscriptionId, armOptions);
        var resourceGroup = armClient.GetResourceGroupResource(
            ResourceGroupResource.CreateResourceIdentifier(SubscriptionId, ResourceGroup));

        var azureService = Substitute.For<IAzureService>();
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(cloud);
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.GetResourceGroupResource(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(resourceGroup);
        azureService.ResolveTenantIdAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<string>(0));

        var logsCredential = new CapturingTokenCredential("logs-token");
        azureService.GetTokenCredentialAsync(
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(logsCredential);

        var dataHandler = new CapturingHttpMessageHandler(responseFactory);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(dataHandler, disposeHandler: false));
        var service = new MonitorLogSearchService(azureService, httpClientFactory);

        return new(service, azureService, armHandler, dataHandler, logsCredential);
    }

    private static HttpResponseMessage CreateArmResponse(
        Uri requestUri,
        string plan,
        string? lastPlanModified,
        HttpStatusCode workspaceStatus,
        HttpStatusCode tableStatus)
    {
        string content;
        if (requestUri.AbsolutePath.EndsWith(
            $"/workspaces/{Workspace}",
            StringComparison.OrdinalIgnoreCase))
        {
            if (workspaceStatus != HttpStatusCode.OK)
            {
                return ArmErrorResponse(workspaceStatus);
            }

            content = $$"""
                {
                  "id": "/subscriptions/{{SubscriptionId}}/resourceGroups/{{ResourceGroup}}/providers/Microsoft.OperationalInsights/workspaces/{{Workspace}}",
                  "name": "{{Workspace}}",
                  "type": "Microsoft.OperationalInsights/workspaces",
                  "location": "westus",
                  "properties": {
                    "customerId": "{{WorkspaceId}}",
                    "provisioningState": "Succeeded"
                  }
                }
                """;
        }
        else if (requestUri.AbsolutePath.Contains("/tables/", StringComparison.OrdinalIgnoreCase))
        {
            if (tableStatus != HttpStatusCode.OK)
            {
                return ArmErrorResponse(tableStatus);
            }

            var table = Uri.UnescapeDataString(requestUri.Segments[^1]);
            var requestedPlan = table.Equals(Table, StringComparison.OrdinalIgnoreCase)
                ? plan
                : "Analytics";
            var transitionProperty = lastPlanModified is null
                ? string.Empty
                : $", \"lastPlanModifiedDate\": {JsonSerializer.Serialize(lastPlanModified)}";
            content = $$"""
                {
                  "id": "/subscriptions/{{SubscriptionId}}/resourceGroups/{{ResourceGroup}}/providers/Microsoft.OperationalInsights/workspaces/{{Workspace}}/tables/{{table}}",
                  "name": "{{table}}",
                  "type": "Microsoft.OperationalInsights/workspaces/tables",
                  "properties": {
                    "plan": "{{requestedPlan}}"{{transitionProperty}},
                    "schema": {
                      "name": "{{table}}",
                      "columns": [],
                      "standardColumns": []
                    }
                  }
                }
                """;
        }
        else
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        return DataResponse(content)();
    }

    private static HttpResponseMessage ArmErrorResponse(HttpStatusCode status) =>
        DataResponse(
            """{"error":{"code":"ArmFailure","message":"sensitive ARM details"}}""",
            status)();

    private static Func<HttpResponseMessage> DataResponse(
        string body,
        HttpStatusCode status = HttpStatusCode.OK) =>
        () => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed record ServiceFixture(
        MonitorLogSearchService Service,
        IAzureService AzureService,
        CapturingHttpMessageHandler ArmHandler,
        CapturingHttpMessageHandler DataHandler,
        CapturingTokenCredential LogsCredential)
    {
        public Task<WorkspaceLogSearchResult> SearchAsync(
            CancellationToken cancellationToken,
            string timespan = Timespan,
            int limit = 20,
            string? tenant = null) =>
            Service.SearchWorkspaceLogs(
                Subscription,
                ResourceGroup,
                Workspace,
                Table,
                Query,
                timespan,
                limit,
                tenant,
                cancellationToken);
    }

    private sealed class CapturingTokenCredential(string token) : TokenCredential
    {
        public string[]? Scopes { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            Capture(requestContext, cancellationToken);
            return CreateToken();
        }

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            Capture(requestContext, cancellationToken);
            return ValueTask.FromResult(CreateToken());
        }

        private void Capture(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            Scopes = [.. requestContext.Scopes];
            CancellationToken = cancellationToken;
        }

        private AccessToken CreateToken() =>
            new(token, DateTimeOffset.UtcNow.AddHours(1));
    }

    private sealed class TrackingHttpContent(Stream stream) : HttpContent
    {
        public bool IsDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(
            Stream target,
            TransportContext? context) =>
            stream.CopyToAsync(target);

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult(stream);

        protected override Task<Stream> CreateContentReadStreamAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(stream);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TimeoutCancellationStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<int>(new OperationCanceledException(cancellationToken));

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    private sealed class BlockingUntilCanceledStream(Action cancelCaller) : Stream
    {
        public bool ReadStarted { get; private set; }
        public bool ObservedCancellation { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ReadStarted = true;
            cancelCaller();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return 0;
            }
            catch (OperationCanceledException)
            {
                ObservedCancellation = true;
                throw;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    private sealed class CapturingHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public List<Uri> RequestUris { get; } = [];
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? ContentType { get; private set; }
        public string? Prefer { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            if (request.RequestUri is not null)
            {
                RequestUris.Add(request.RequestUri);
            }
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Prefer = request.Headers.TryGetValues("Prefer", out var values)
                ? values.Single()
                : null;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return await responseFactory(request, cancellationToken);
        }
    }
}
