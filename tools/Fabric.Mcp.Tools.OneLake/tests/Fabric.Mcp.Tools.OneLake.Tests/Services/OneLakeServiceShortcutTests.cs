// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable xUnit1051

using System.Net;
using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Fabric.Mcp.Tools.OneLake.Tests.TestSupport;

namespace Fabric.Mcp.Tools.OneLake.Tests.Services;

public class OneLakeServiceShortcutTests
{
    private const string WorkspaceId = "ws-shortcut-test";
    private const string ItemId = "item-shortcut-test";

    [Fact]
    public async Task CreateShortcutAsync_WithConflictPolicy_SendsExpectedRequestAndReturnsResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        var expectedShortcut = new OneLakeShortcut
        {
            Path = "Files/landing",
            Name = "shortcut1",
            Target = new ShortcutTarget
            {
                Type = "OneLake",
                OneLake = new OneLakeShortcutTarget
                {
                    WorkspaceId = "target-ws",
                    ItemId = "target-item",
                    Path = "Files/data"
                }
            }
        };

        var handler = new CapturingHttpMessageHandler(request =>
        {
            capturedRequest = request;
            capturedBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            var responseJson = JsonSerializer.Serialize(expectedShortcut, OneLakeJsonContext.Default.OneLakeShortcut);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        });

        using var httpClient = new HttpClient(handler);
        var service = new OneLakeService(httpClient, new FakeTokenCredential());

        var result = await service.CreateShortcutAsync(WorkspaceId, ItemId, expectedShortcut, "CreateOrOverwrite");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal(
            $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}/shortcuts?shortcutConflictPolicy=CreateOrOverwrite",
            capturedRequest.RequestUri!.ToString());

        Assert.NotNull(capturedBody);
        var sentShortcut = JsonSerializer.Deserialize(capturedBody, OneLakeJsonContext.Default.OneLakeShortcut);
        Assert.NotNull(sentShortcut);
        Assert.Equal(expectedShortcut.Name, sentShortcut!.Name);
        Assert.Equal(expectedShortcut.Target!.Type, sentShortcut.Target!.Type);
        Assert.Equal(expectedShortcut.Target.OneLake!.WorkspaceId, sentShortcut.Target.OneLake!.WorkspaceId);

        Assert.Equal(expectedShortcut.Name, result.Name);
        Assert.Equal(expectedShortcut.Target.OneLake.ItemId, result.Target!.OneLake!.ItemId);
    }

    [Fact]
    public async Task CreateShortcutAsync_WithEmptyResponse_ReturnsOriginalShortcut()
    {
        var shortcut = new OneLakeShortcut
        {
            Path = "Files/landing",
            Name = "shortcut2",
            Target = new ShortcutTarget
            {
                Type = "ExternalDataShare",
                ExternalDataShare = new ExternalDataShareShortcutTarget
                {
                    ConnectionId = "connection-1"
                }
            }
        };

        var handler = new CapturingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        });

        using var httpClient = new HttpClient(handler);
        var service = new OneLakeService(httpClient, new FakeTokenCredential());

        var result = await service.CreateShortcutAsync(WorkspaceId, ItemId, shortcut);

        Assert.Same(shortcut, result);
    }
}
