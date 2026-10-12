// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Http;
using Microsoft.Security.AntiSSRF;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

/// <summary>
/// Verifies <see cref="SsrfProtectionPolicy.AllNamespaces"/> overrides without sharing configuration with other providers.
/// </summary>
public sealed class AllNamespaceHttpBypassTests
{
    [Fact]
    public async Task All_RequiresResolvedContext()
    {
        await using LoopbackHttpServer server = LoopbackHttpServer.Start();
        using ServiceProvider provider = AntiSsrfHttpClientTests.CreateProvider(ssrfProtectionPolicy: new SsrfProtectionPolicy(["all"]));
        ICommandContextAccessor accessor = provider.GetRequiredService<ICommandContextAccessor>();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
        using HttpClient client = factory.CreateClient();
        using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
        {
            using HttpResponseMessage response = await client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken);
            Assert.True(response.IsSuccessStatusCode);
        }

        foreach (string? namespaceName in new string?[] { null, "", " " })
        {
            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName }))
            {
                await Assert.ThrowsAsync<AntiSSRFException>(() =>
                    client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken));
            }
        }

        await Assert.ThrowsAsync<AntiSSRFException>(() =>
            client.GetAsync(server.Endpoint, TestContext.Current.CancellationToken));
        Assert.Single(server.Requests);
    }
}
