// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Fabric.Mcp.Tools.OneLake.Tests.TestSupport;

namespace Fabric.Mcp.Tools.OneLake.Tests.Services;

/// <summary>
/// Tests that OneLakeService correctly handles Fabric long running operations (LRO)
/// that return 202 Accepted with a Location header.
/// </summary>
public class OneLakeServiceLroTests
{
    private const string WorkspaceId = "ws-lro-test";
    private const string ItemId = "item-lro-test";
    private const string OperationId = "op-lro-12345";
    private const string OperationUrl = $"https://dailyapi.fabric.microsoft.com/v1/operations/{OperationId}";
    private const string ResultUrl = $"https://dailyapi.fabric.microsoft.com/v1/operations/{OperationId}/result";
    private const string ShortcutsUrl = $"https://dailyapi.fabric.microsoft.com/v1/workspaces/{WorkspaceId}/items/{ItemId}/shortcuts/bulkCreate";

    private static OneLakeService CreateService(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var httpClient = new HttpClient(new CapturingHttpMessageHandler(handler));
        return new OneLakeService(httpClient, new FakeTokenCredential());
    }

    private static string RunningStateJson => JsonSerializer.Serialize(
        new { status = "Running", percentComplete = 50 });

    private static string SucceededStateJson => JsonSerializer.Serialize(
        new { status = "Succeeded", percentComplete = 100 });

    private static string FailedStateJson => JsonSerializer.Serialize(
        new { status = "Failed", percentComplete = 0, error = new { errorCode = "InternalError", message = "Something went wrong" } });

    private static string BulkCreateResultJson => JsonSerializer.Serialize(
        new
        {
            value = new[]
            {
                new { status = "Succeeded", result = new { path = "/Files", name = "lro-test-shortcut" } }
            }
        });

    /// <summary>
    /// Single-poll success: 202 → poll returns Succeeded → result fetched.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_202WithLocation_PollsAndReturnsResult()
    {
        var callCount = 0;
        var service = CreateService(request =>
        {
            callCount++;
            var url = request.RequestUri!.ToString();

            // First call: the bulk create → 202 with Location
            if (url.Contains("bulkCreate") && callCount == 1)
            {
                var accepted = new HttpResponseMessage(HttpStatusCode.Accepted);
                accepted.Headers.Location = new Uri(OperationUrl);
                return accepted;
            }

            // Second call: polling → Succeeded with Location pointing to result
            if (url == OperationUrl)
            {
                var poll = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SucceededStateJson, Encoding.UTF8, "application/json")
                };
                poll.Headers.Location = new Uri(ResultUrl);
                return poll;
            }

            // Third call: result fetch
            if (url == ResultUrl)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BulkCreateResultJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.CreateOrUpdateShortcutsAsync(
            WorkspaceId, ItemId,
            "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"lro-test-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Succeeded", result.Value[0].Status);
        Assert.Equal(3, callCount); // bulk create + poll + result
    }

    /// <summary>
    /// Multi-poll: 202 → Running → Succeeded → result fetched.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_202_MultiplePolls_EventuallySucceeds()
    {
        var pollCount = 0;
        var service = CreateService(request =>
        {
            var url = request.RequestUri!.ToString();

            if (url.Contains("bulkCreate"))
            {
                var accepted = new HttpResponseMessage(HttpStatusCode.Accepted);
                accepted.Headers.Location = new Uri(OperationUrl);
                return accepted;
            }

            if (url == OperationUrl)
            {
                pollCount++;
                if (pollCount < 3)
                {
                    // First two polls: still running
                    var running = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(RunningStateJson, Encoding.UTF8, "application/json")
                    };
                    running.Headers.Location = new Uri(OperationUrl);
                    return running;
                }
                // Third poll: succeeded
                var succeeded = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SucceededStateJson, Encoding.UTF8, "application/json")
                };
                succeeded.Headers.Location = new Uri(ResultUrl);
                return succeeded;
            }

            if (url == ResultUrl)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BulkCreateResultJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.CreateOrUpdateShortcutsAsync(
            WorkspaceId, ItemId,
            "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"lro-test-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.Equal(3, pollCount);
    }

    /// <summary>
    /// 202 → poll returns Succeeded but no Location header → returns empty response (no crash).
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_202_SucceededWithNoResultLocation_ReturnsEmpty()
    {
        var service = CreateService(request =>
        {
            var url = request.RequestUri!.ToString();

            if (url.Contains("bulkCreate"))
            {
                var accepted = new HttpResponseMessage(HttpStatusCode.Accepted);
                accepted.Headers.Location = new Uri(OperationUrl);
                return accepted;
            }

            if (url == OperationUrl)
            {
                // Succeeded but no Location header for result
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SucceededStateJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.CreateOrUpdateShortcutsAsync(
            WorkspaceId, ItemId,
            "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"lro-test-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Value); // empty response from Stream.Null
    }

    /// <summary>
    /// 202 → poll returns Failed → HttpRequestException thrown.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_202_OperationFails_ThrowsHttpRequestException()
    {
        var service = CreateService(request =>
        {
            var url = request.RequestUri!.ToString();

            if (url.Contains("bulkCreate"))
            {
                var accepted = new HttpResponseMessage(HttpStatusCode.Accepted);
                accepted.Headers.Location = new Uri(OperationUrl);
                return accepted;
            }

            if (url == OperationUrl)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(FailedStateJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CreateOrUpdateShortcutsAsync(
                WorkspaceId, ItemId,
                "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"lro-test-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
                cancellationToken: CancellationToken.None));

        Assert.Contains("InternalError", ex.Message);
        Assert.Contains("Something went wrong", ex.Message);
    }

    /// <summary>
    /// 202 with no Location header → returns empty response gracefully.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_202_NoLocationHeader_ReturnsEmpty()
    {
        var service = CreateService(request =>
        {
            if (request.RequestUri!.ToString().Contains("bulkCreate"))
            {
                // 202 but without Location header
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.CreateOrUpdateShortcutsAsync(
            WorkspaceId, ItemId,
            "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"lro-test-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Value);
    }

    /// <summary>
    /// Synchronous 200 response (non-LRO path) still works correctly.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateShortcutsAsync_200_ReturnsResultDirectly()
    {
        var resultJson = JsonSerializer.Serialize(new
        {
            value = new[]
            {
                new { status = "Succeeded", result = new { path = "/Files", name = "sync-shortcut" } }
            }
        });

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(resultJson, Encoding.UTF8, "application/json")
        });

        var result = await service.CreateOrUpdateShortcutsAsync(
            WorkspaceId, ItemId,
            "{\"createShortcutRequests\":[{\"path\":\"/Files\",\"name\":\"sync-shortcut\",\"target\":{\"oneLake\":{\"workspaceId\":\"ws2\",\"itemId\":\"item2\",\"path\":\"Files\"}}}]}",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Succeeded", result.Value[0].Status);
    }
}
