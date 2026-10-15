// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class WorkspaceCapacityAssignmentServiceTests
{
    private static readonly Guid s_workspaceId = Guid.Parse("cfafbeb1-8037-4d0c-896e-a46fb27ff512");
    private static readonly Guid s_capacityId = Guid.Parse("0f084df7-c13d-451b-af5f-ed0c466403b2");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AssignWorkspaceToCapacityAsync_SendsOnePostAndDisposesMessages(bool includeOperationHeaders)
    {
        var credential = CreateCredential();
        using var body = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = body };
        if (includeOperationHeaders)
        {
            response.Headers.Location = new Uri("https://example.com/must-not-be-followed");
            response.Headers.Add("x-ms-operation-id", "must-not-be-used");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
        }
        HttpRequestMessage? lastRequest = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            lastRequest = request;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{s_workspaceId:D}/assignToCapacity", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("fake-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.NotNull(request.Content);
            using var payload = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.Single(payload.RootElement.EnumerateObject());
            Assert.Equal(s_capacityId, payload.RootElement.GetProperty("capacityId").GetGuid());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await service.AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.True(body.IsDisposed);
        Assert.NotNull(lastRequest?.Content);
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            lastRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssignWorkspaceToCapacityAsync_AcceptsEmpty202WithoutBody()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
        using var client = new HttpClient(handler);

        await new FabricCoreService(client, CreateCredential())
            .AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AssignWorkspaceToCapacityAsync_RejectsEmptyGuidBeforeAuthentication(bool invalidWorkspace)
    {
        var credential = CreateCredential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AssignWorkspaceToCapacityAsync(
            invalidWorkspace ? Guid.Empty : s_workspaceId,
            invalidWorkspace ? s_capacityId : Guid.Empty,
            TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TemporaryRedirect, HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.OK, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    public async Task AssignWorkspaceToCapacityAsync_RejectsNon202WithoutReadingBodyOrRetrying(
        HttpStatusCode status, HttpStatusCode expectedStatus)
    {
        using var body = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = body, ReasonPhrase = "sensitive-backend-details" };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, TestContext.Current.CancellationToken));

        Assert.Equal(expectedStatus, exception.StatusCode);
        Assert.DoesNotContain("sensitive", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(body.IsDisposed);
    }

    [Theory]
    [InlineData("17", true)]
    [InlineData("0", true)]
    [InlineData("-1", false)]
    [InlineData("sensitive-header", false)]
    [InlineData("9999999999999999999999999", false)]
    [InlineData("2147483647", true)]
    [InlineData("2147483648", false)]
    [InlineData("9223372036854775807", false)]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", false)]
    public async Task ExecuteAsync_ReportsOnlyValidRetryAfterWithoutRetrying(string retryAfter, bool valid)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());
        var command = new WorkspaceAssignToCapacityCommand(NullLogger<WorkspaceAssignToCapacityCommand>.Instance, service);
        var context = new CommandContext();

        var result = await ((IBaseCommand)command).ExecuteAsync(
            context, command.GetCommand().Parse(["--workspace-id", s_workspaceId.ToString(), "--capacity-id", s_capacityId.ToString()]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, result.Status);
        Assert.Equal(valid, result.Message.Contains("wait at least", StringComparison.Ordinal));
        if (valid)
        {
            Assert.Contains($"wait at least {retryAfter} seconds", result.Message);
        }
        Assert.DoesNotContain("sensitive", result.Message);
        Assert.DoesNotContain("sensitive", JsonSerializer.Serialize(result, ModelsJsonContext.Default.CommandResponse));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(45L)]
    [InlineData(2147483647L)]
    public async Task ExecuteAsync_PreservesLongSecondsFromTypedRetryAfter(long seconds)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var command = new WorkspaceAssignToCapacityCommand(
            NullLogger<WorkspaceAssignToCapacityCommand>.Instance, new FabricCoreService(client, CreateCredential()));

        var result = await ((IBaseCommand)command).ExecuteAsync(
            new CommandContext(), command.GetCommand().Parse(["--workspace-id", s_workspaceId.ToString(), "--capacity-id", s_capacityId.ToString()]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, result.Status);
        Assert.Contains($"wait at least {seconds} seconds", result.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("17", "18", 17L)]
    [InlineData("17", "sensitive-header", 17L)]
    [InlineData("sensitive-header", "17", null)]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "17", null)]
    public async Task ExecuteAsync_PreservesOriginalMultipleHeaderBehavior(string first, string second, long? expectedSeconds)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("Retry-After", [first, second]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var command = new WorkspaceAssignToCapacityCommand(
            NullLogger<WorkspaceAssignToCapacityCommand>.Instance, new FabricCoreService(client, CreateCredential()));

        var result = await ((IBaseCommand)command).ExecuteAsync(
            new CommandContext(), command.GetCommand().Parse(["--workspace-id", s_workspaceId.ToString(), "--capacity-id", s_capacityId.ToString()]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, result.Status);
        Assert.Equal(expectedSeconds.HasValue, result.Message.Contains("wait at least", StringComparison.Ordinal));
        if (expectedSeconds.HasValue)
        {
            Assert.Contains($"wait at least {expectedSeconds} seconds", result.Message);
        }
        Assert.DoesNotContain("sensitive", result.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("before-auth")]
    [InlineData("during-auth")]
    [InlineData("after-auth")]
    [InlineData("during-http")]
    public async Task AssignWorkspaceToCapacityAsync_PropagatesCancellation(string stage)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = CreateCredential();
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        if (stage == "before-auth")
        {
            await cancellation.CancelAsync();
        }
        else if (stage == "during-auth")
        {
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token).Returns(_ =>
            {
                cancellation.Cancel();
                return new ValueTask<AccessToken>(Task.FromCanceled<AccessToken>(cancellation.Token));
            });
        }
        else if (stage == "after-auth")
        {
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token).Returns(_ =>
            {
                cancellation.Cancel();
                return new ValueTask<AccessToken>(new AccessToken("fake-token", DateTimeOffset.MaxValue));
            });
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, cancellation.Token));

        Assert.Equal(stage == "during-http" ? 1 : 0, handler.CallCount);
        if (stage == "before-auth")
        {
            Assert.Empty(credential.ReceivedCalls());
        }
    }

    [Fact]
    public async Task AssignWorkspaceToCapacityAsync_DoesNotCacheBearerTokensAcrossInvocations()
    {
        var credential = CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(
            new ValueTask<AccessToken>(new AccessToken("first-token", DateTimeOffset.MaxValue)),
            new ValueTask<AccessToken>(new AccessToken("second-token", DateTimeOffset.MaxValue)));
        var tokens = new List<string?>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            tokens.Add(request.Headers.Authorization?.Parameter);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await service.AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, TestContext.Current.CancellationToken);
        await service.AssignWorkspaceToCapacityAsync(s_workspaceId, s_capacityId, TestContext.Current.CancellationToken);

        Assert.Equal(["first-token", "second-token"], tokens);
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("fake-token", DateTimeOffset.MaxValue)));
        return credential;
    }
}
