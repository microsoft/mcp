// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Http;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public sealed class KustoHttpClientRegistrationTests
{
    [Fact]
    public void ConfigureServices_DisablesRedirectsOnlyForKustoClient()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddHttpClient();
        services.Configure<ServerRuntimeConfiguration>(_ => { });
        services.ConfigureDefaultHttpClient();

        new KustoSetup().ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var handlerFactory = serviceProvider.GetRequiredService<IHttpMessageHandlerFactory>();

        Assert.False(GetPrimaryHandler(handlerFactory.CreateHandler(KustoClient.HttpClientName)).AllowAutoRedirect);
        Assert.True(GetPrimaryHandler(handlerFactory.CreateHandler(Microsoft.Extensions.Options.Options.DefaultName)).AllowAutoRedirect);
    }

    private static HttpClientHandler GetPrimaryHandler(HttpMessageHandler handler)
    {
        while (handler is DelegatingHandler delegatingHandler)
        {
            handler = delegatingHandler.InnerHandler
                ?? throw new InvalidOperationException("HTTP handler chain contains a delegating handler without an inner handler.");
        }

        return Assert.IsType<HttpClientHandler>(handler);
    }
}
