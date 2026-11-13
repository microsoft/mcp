// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

internal sealed class TestHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
{
    public string? LastClientName { get; private set; }

    public int CreateCount { get; private set; }

    public HttpClient CreateClient(string name)
    {
        LastClientName = name;
        CreateCount++;
        return new HttpClient(handler, disposeHandler: false);
    }
}
