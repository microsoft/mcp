// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobRunTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemJobRunServiceTests()
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("{}", null)]
    [InlineData(null, "[]")]
    [InlineData("""{"opaque":[null,1e30,true,{"x":"text"}]}""", Parameters)]
    public static async Task Run_SendsExactlyOnePostPreservingOptionalInputs(string? execution, string? parameters)
    {
        using var content = new UnreadableHttpContent();
        using var response = Accepted(content: content);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", ActivityId);
        HttpRequestMessage? captured = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            captured = request;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/{JobType}/instances", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            if (execution is null && parameters is null)
            {
                Assert.Null(request.Content);
            }
            else
            {
                Assert.NotNull(request.Content);
                Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
                var body = await request.Content.ReadAsStringAsync(token);
                using var document = JsonDocument.Parse(body);
                Assert.Equal(execution is not null, document.RootElement.TryGetProperty("executionData", out var actualExecution));
                Assert.Equal(parameters is not null, document.RootElement.TryGetProperty("parameters", out var actualParameters));
                if (execution is not null)
                {
                    using var expected = JsonDocument.Parse(execution);
                    Assert.True(JsonElement.DeepEquals(expected.RootElement, actualExecution));
                }
                if (parameters is not null)
                {
                    using var expected = JsonDocument.Parse(parameters);
                    Assert.True(JsonElement.DeepEquals(expected.RootElement, actualParameters));
                }
            }
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var receipt = await new FabricCoreService(client, credential).RunOnDemandItemJobAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            JobType, execution, parameters, TestContext.Current.CancellationToken);

        Assert.Equal("Accepted", receipt.SubmissionStatus);
        Assert.Equal(Guid.Parse(WorkspaceId), receipt.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemId), receipt.ItemId);
        Assert.Equal(JobType, receipt.JobType);
        Assert.Equal(Guid.Parse(JobInstanceId), receipt.JobInstanceId);
        Assert.Equal(60, receipt.RetryAfterSeconds);
        Assert.Null(receipt.RetryAfterUtc);
        Assert.Null(receipt.TrackingIssue);
        Assert.Null(receipt.RetryAfterIssue);
        var output = JsonSerializer.Serialize(receipt, CoreJsonContext.Default.ItemJobRunCommandResult);
        Assert.DoesNotContain("private", output);
        Assert.DoesNotContain("operationId", output);
        Assert.DoesNotContain(ActivityId, output);
        Assert.DoesNotContain("Completed", output);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
        if (captured?.Content is { } sentContent)
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(() => sentContent.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("FutureJob")]
    [InlineData("future/job?query=#fragment")]
    [InlineData("%2F..%2F")]
    [InlineData("../other")]
    [InlineData("https://example.invalid/other")]
    [InlineData("job with spaces")]
    [InlineData("\u00e9-job")]
    public static async Task Run_EncodesTheExplicitJobTypeAsOnePathComponent(string jobType)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/{Uri.EscapeDataString(jobType)}/instances",
                request.RequestUri?.AbsoluteUri);
            Assert.Equal("", request.RequestUri?.Query);
            Assert.Equal("", request.RequestUri?.Fragment);
            return Task.FromResult(Accepted());
        });
        using var client = new HttpClient(handler);

        var result = await new FabricCoreService(client, Credential()).RunOnDemandItemJobAsync(
            WorkspaceId, ItemId, jobType, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(jobType, result.JobType);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("workspace")]
    [InlineData("item")]
    [InlineData("job")]
    [InlineData("empty-workspace")]
    [InlineData("empty-item")]
    [InlineData("execution")]
    [InlineData("parameters")]
    [InlineData("duplicates")]
    [InlineData("size")]
    [InlineData("unicode")]
    public static async Task Run_RejectsInvalidInputsBeforeAuthenticationOrHttp(string failure)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<FabricItemInputException>(() => service.RunOnDemandItemJobAsync(
            failure == "workspace" ? "private-invalid" : failure == "empty-workspace" ? Guid.Empty.ToString() : WorkspaceId,
            failure == "item" ? "../private" : failure == "empty-item" ? Guid.Empty.ToString() : ItemId,
            failure == "job" ? ".." : JobType,
            failure == "execution" ? """{"private":1,"private":2}""" :
                failure == "size" ? new string(' ', ItemJobRunInput.MaxRequestBytes + 1) :
                failure == "unicode" ? """{"value":"\uD800"}""" : null,
            failure == "parameters" ? "[{}]" : failure == "duplicates" ? """[{"name":"x","type":"Text","value":1},{"name":"X","type":"Text","value":2}]""" : null,
            TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(Location)]
    [InlineData("/v1/workspaces/" + WorkspaceId + "/items/" + ItemId + "/jobs/instances/" + JobInstanceId)]
    [InlineData("https://API.FABRIC.MICROSOFT.COM:443/v1/workspaces/" + WorkspaceId + "/items/" + ItemId + "/jobs/instances/CCCCCCCC-CCCC-4CCC-8CCC-CCCCCCCCCCCC")]
    public static async Task Run_TrustsOnlyAnInstanceOnTheExpectedFabricOriginAndContext(string location)
    {
        using var response = Accepted(location);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await RunAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(Guid.Parse(JobInstanceId), result.JobInstanceId);
        Assert.Null(result.TrackingIssue);
        Assert.Equal(1, handler.CallCount);
    }

    public static TheoryData<string?, string> UnavailableTrackingLocations => new()
    {
        { null, "MissingLocation" },
        { "", "InvalidLocation" },
        { "private-invalid", "InvalidLocation" },
        { Location + "/result", "InvalidLocation" },
        { Location + "/", "InvalidLocation" },
        { Location + "?jobType=Execute", "InvalidLocation" },
        { Location + "#private", "InvalidLocation" },
        { Location.Replace(JobInstanceId, Guid.Empty.ToString(), StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace(WorkspaceId, ActivityId, StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace(ItemId, ActivityId, StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("api.fabric.microsoft.com", "example.invalid", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("https://", "http://", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("https://", "https://private@", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("api.fabric.microsoft.com", "api.fabric.microsoft.com:444", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("/v1/", "/v1/../v1/", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("/v1/", "/v1/./", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("/jobs/", "/jobs%2F", StringComparison.Ordinal), "InvalidLocation" },
        { Location.Replace("/jobs/", "\\jobs\\", StringComparison.Ordinal), "InvalidLocation" },
        { "//api.fabric.microsoft.com" + new Uri(Location).AbsolutePath, "InvalidLocation" },
        { " " + Location, "InvalidLocation" },
        { Location + "\r\nprivate", "InvalidLocation" },
        { Location + "," + Location, "InvalidLocation" },
        { new string('x', 4097), "InvalidLocation" },
        { $"{FabricEndpoints.FabricApiBaseUrl}/operations/{JobInstanceId}", "InvalidLocation" },
        { $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances", "CollectionLocation" },
        { $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances?jobType=Execute", "CollectionLocation" }
    };

    [Theory]
    [MemberData(nameof(UnavailableTrackingLocations))]
    public static async Task Run_PreservesAcceptanceWithoutRecoveryReadsOrUnsafeUrls(string? location, string issue)
    {
        using var content = new UnreadableHttpContent();
        using var response = Accepted(location, content: content);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", JobInstanceId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var receipt = await RunAsync(new FabricCoreService(client, Credential()));

        Assert.Equal("Accepted", receipt.SubmissionStatus);
        Assert.Null(receipt.JobInstanceId);
        Assert.Equal(issue, receipt.TrackingIssue);
        Assert.Contains("accepted", receipt.Guidance);
        Assert.Contains("do not resubmit", receipt.Guidance);
        var json = JsonSerializer.Serialize(receipt, CoreJsonContext.Default.ItemJobRunCommandResult);
        Assert.DoesNotContain("private", json);
        Assert.DoesNotContain("https://", json);
        Assert.DoesNotContain(JobInstanceId, json);
        Assert.Equal(60, receipt.RetryAfterSeconds);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task Run_RejectsEvenIdenticalMultipleLocationsWithoutLosingAcceptance()
    {
        using var response = Accepted(location: null);
        response.Headers.TryAddWithoutValidation("Location", [Location, Location]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await RunAsync(new FabricCoreService(client, Credential()));

        Assert.Equal("Accepted", result.SubmissionStatus);
        Assert.Null(result.JobInstanceId);
        Assert.Equal("MultipleLocations", result.TrackingIssue);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("0", 0L)]
    [InlineData("60", 60L)]
    [InlineData("2147483647", 2147483647L)]
    [InlineData("2147483648", null)]
    [InlineData("-1", null)]
    [InlineData("0.5", null)]
    [InlineData("private-invalid", null)]
    [InlineData("60, 120", null)]
    public static async Task Run_ValidatesRetryGuidanceWithoutInventingAnEarlyPollDelay(string? retry, long? expected)
    {
        using var response = Accepted(retryAfter: retry);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await RunAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(expected is null ? "MissingOrInvalidRetryAfter" : null, result.RetryAfterIssue);
        Assert.Contains(expected is null ? "first-read delay is unknown" : $"Wait at least {expected} seconds", result.Guidance);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Run_PreservesHttpDateAndRejectsMultipleRetryValues()
    {
        using var response = Accepted(retryAfter: "Tue, 01 Jan 2030 00:00:00 GMT");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await RunAsync(new FabricCoreService(client, Credential()));
        Assert.Null(result.RetryAfterSeconds);
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Contains("Wait until at least 2030-01-01 00:00:00 UTC", result.Guidance);
        Assert.Equal(1, handler.CallCount);

        using var multiple = Accepted(retryAfter: null);
        multiple.Headers.TryAddWithoutValidation("Retry-After", ["60", "60"]);
        using var secondHandler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(multiple));
        using var secondClient = new HttpClient(secondHandler);
        var second = await RunAsync(new FabricCoreService(secondClient, Credential()));
        Assert.Null(second.RetryAfterSeconds);
        Assert.Null(second.RetryAfterUtc);
        Assert.Equal("MissingOrInvalidRetryAfter", second.RetryAfterIssue);
        Assert.Equal(1, secondHandler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Found, HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public static async Task Run_NeverRetriesOrReadsFailureBodies(HttpStatusCode status, HttpStatusCode expected)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.TryAddWithoutValidation("Location", "https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "90");
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<ItemJobRunRequestException>(() => RunAsync(new FabricCoreService(client, Credential())));

        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(90), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task Run_FeatureNotAvailableDoesNotStripParametersOrResubmit()
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            var body = await request.Content!.ReadAsStringAsync(token);
            Assert.Contains("\"parameters\"", body);
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"errorCode":"FeatureNotAvailable","message":"Parameter is not allowed for this item type or this item job type","private":"payload"}""")
            };
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobRunRequestException>(() =>
            RunAsync(new FabricCoreService(client, Credential()), parameters: Parameters));
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Run_PreservesPreSubmissionCancellationIncludingLateCredentialCompletion(bool duringAuth)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new AccessToken("job-test-token", DateTimeOffset.MaxValue);
        });
        if (!duringAuth)
        {
            await cancellation.CancelAsync();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RunAsync(new FabricCoreService(client, credential), token: cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task Run_AuthenticationFailureDoesNotSend()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private-auth")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => RunAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Run_NetworkFailureAndCancellationAfterPossibleSubmissionNeverRetry(bool cancel)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (cancel)
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(token);
            }
            throw new HttpRequestException("private-network");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (cancel)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RunAsync(service, token: cancellation.Token));
        }
        else
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => RunAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Run_LateCancellationDoesNotEraseConfirmedAcceptanceOrReadTheBody()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new UnreadableHttpContent();
        using var response = Accepted(content: content);
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);

        var result = await RunAsync(new FabricCoreService(client, Credential()), token: cancellation.Token);

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal("Accepted", result.SubmissionStatus);
        Assert.Equal(Guid.Parse(JobInstanceId), result.JobInstanceId);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task Run_KeepsConcurrentCredentialsBodiesAndReceiptsIsolated()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            var current = request.Headers.Authorization!.Parameter!;
            observed.Add(current);
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(current, body.RootElement.GetProperty("executionData").GetProperty("id").GetString());
            return Accepted(Location.Replace(JobInstanceId, current, StringComparison.Ordinal));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            identity.Value = id.ToString();
            var receipt = await RunAsync(service, executionData: $$"""{"id":"{{id}}"}""");
            Assert.Equal(id, receipt.JobInstanceId);
        }));

        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, observed.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }
}
