// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

/// <summary>
/// Verifies fail-closed <see cref="SsrfProtectionPolicy.AllNamespaces"/> behavior
/// with an independently configured <see cref="EndpointValidator"/>.
/// </summary>
public sealed class ArmAllNamespaceBypassTests
{
    /// <summary>
    /// Verifies that <see cref="SsrfProtectionPolicy.AllNamespaces"/> permits a resolved namespace
    /// but cannot exempt requests without one.
    /// </summary>
    [Fact]
    public async Task All_StillValidatesAbsentAndUnresolvedContextsForSyncAndAsyncSends()
    {
        var accessor = new CommandContextAccessor();
        using var handler = new ArmTestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        AzureService service = ArmEndpointValidationTests.CreateService(
            accessor, handler, ArmEnvironment.AzurePublicCloud, new SsrfProtectionPolicy(["all"]));
        var options = new ArmClientOptions();
        service.ConfigureArmClientOptions(options);
        HttpPipeline pipeline = HttpPipelineBuilder.Build(options);

        foreach (bool synchronous in new[] { false, true })
        {
            using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "storage" }))
            {
                await SendAsync("https://evil.example", synchronous);
            }

            foreach (string? namespaceName in new string?[] { null, "", " " })
            {
                using (accessor.BeginScope(new CommandContext { ToolNamespaceName = namespaceName }))
                {
                    await Assert.ThrowsAsync<SecurityException>(() => SendAsync("https://evil.example", synchronous));
                }
            }

            Assert.Null(accessor.CurrentContext);
            await Assert.ThrowsAsync<SecurityException>(() => SendAsync("https://evil.example", synchronous));
            await SendAsync(ArmEnvironment.AzurePublicCloud.Endpoint.AbsoluteUri, synchronous);
        }

        Assert.Equal(4, handler.RequestUris.Count);
        Assert.Equal(2, handler.RequestUris.Count(uri => uri.Host == "evil.example"));

        async Task SendAsync(string endpoint, bool synchronous)
        {
            using Request request = pipeline.CreateRequest();
            request.Uri.Reset(new Uri(endpoint));
            using Response response = synchronous
                ? pipeline.SendRequest(request, TestContext.Current.CancellationToken)
                : await pipeline.SendRequestAsync(request, TestContext.Current.CancellationToken);
        }
    }
}
