// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tests.Helpers;

public static class TestHttpClientFactoryProvider
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddConfiguredHttpClient();
        return services.BuildServiceProvider();
    }
}
