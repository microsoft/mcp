// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tests.Client.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tests.Helpers;

/// <summary>
/// Provides a test helper for creating a <see cref="ServiceProvider"/> pre-configured with HTTP client services.
/// This is intended for use in unit and integration tests that require HTTP client dependencies.
/// </summary>
public static class TestHttpClientFactoryProvider
{
    /// <summary>
    /// Creates a new <see cref="ServiceProvider"/> instance with HTTP client services configured for testing.
    /// </summary>
    /// <returns>
    /// A <see cref="ServiceProvider"/> containing the configured HTTP client services for use in tests.
    /// </returns>
    public static ServiceProvider Create(TestProxyFixture? fixture = null)
    {
        Func<Uri?>? recordingProxyResolver = fixture == null ? null : fixture.GetProxyUri;

        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<HttpClientOptions>(_ => { });
        services.Configure<ServiceStartOptions>(_ => { });
        services.AddHttpClient();

        services.ConfigureDefaultHttpClient(recordingProxyResolver);
        return services.BuildServiceProvider();
    }
}
