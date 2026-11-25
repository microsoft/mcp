// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.Tests;

public sealed class AvmDocsServiceAuthenticationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task GitHubRequests_OnlyReadHostTokenWhenAllowed(bool local, bool documentation)
    {
        var environment = Substitute.For<IEnvironmentVariables>();
        environment.IsAccessAllowed.Returns(local);
        environment.GetEnvironmentVariable("GITHUB_TOKEN").Returns("test-token");
        var requests = 0;
        using var handler = new AvmTestHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith(".csv", StringComparison.Ordinal))
            {
                return ModuleIndexResponse();
            }

            requests++;
            Assert.Equal(local ? "Bearer test-token" : null, request.Headers.Authorization?.ToString());
            return new(HttpStatusCode.OK) { Content = new StringContent(documentation ? "# Module documentation" : "[]") };
        });
        var service = CreateService(handler, environment);

        await FetchAsync(service, documentation);

        Assert.Equal(1, requests);
        environment.Received(local ? 1 : 0).GetEnvironmentVariable("GITHUB_TOKEN");
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Unauthorized)]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.TooManyRequests)]
    [InlineData(true, HttpStatusCode.TooManyRequests)]
    public async Task RemoteGitHubRequests_SurfaceAuthenticationAndRateLimitFailures(bool documentation, HttpStatusCode status)
    {
        var environment = Substitute.For<IEnvironmentVariables>();
        var requests = 0;
        using var handler = new AvmTestHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith(".csv", StringComparison.Ordinal))
            {
                return ModuleIndexResponse();
            }

            requests++;
            Assert.Null(request.Headers.Authorization);
            var response = new HttpResponseMessage(status);
            if (status != HttpStatusCode.Unauthorized)
            {
                response.Headers.Add("X-RateLimit-Remaining", "0");
            }
            return response;
        });
        var service = CreateService(handler, environment);

        if (status == HttpStatusCode.Unauthorized)
        {
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => FetchAsync(service, documentation));
            Assert.Equal(status, exception.StatusCode);
        }
        else
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => FetchAsync(service, documentation));
            Assert.Contains("rate limit exceeded", exception.Message);
            Assert.DoesNotContain("Set a GITHUB_TOKEN", exception.Message);
        }

        Assert.Equal(1, requests);
        environment.DidNotReceive().GetEnvironmentVariable(Arg.Any<string>());
    }

    private static AvmDocsService CreateService(HttpMessageHandler handler, IEnvironmentVariables environment)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        return new(factory, environment);
    }

    private static async Task FetchAsync(AvmDocsService service, bool documentation)
    {
        if (documentation)
        {
            await service.GetDocumentationAsync("avm-res-storage-storageaccount", "1.0", TestContext.Current.CancellationToken);
        }
        else
        {
            await service.GetVersionsAsync("avm-res-storage-storageaccount", TestContext.Current.CancellationToken);
        }
    }

    private static HttpResponseMessage ModuleIndexResponse() => new(HttpStatusCode.OK)
    {
        Content = new StringContent("""
            ModuleName,Description,ModuleStatus,RepoURL
            avm-res-storage-storageaccount,Storage Account,Available,https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount
            """)
    };
}
