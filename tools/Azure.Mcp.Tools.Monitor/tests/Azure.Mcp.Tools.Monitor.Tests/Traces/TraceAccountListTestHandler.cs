// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

internal sealed class TraceAccountListTestHandler(int workspaceCount, bool areResultsTruncated) : HttpMessageHandler
{
    internal const string Subscription = "00000000-0000-0000-0000-000000000001";
    private const string AccountPrefix = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Monitor/accounts";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Post)
        {
            Assert.Equal("/providers/Microsoft.ResourceGraph/resources", request.RequestUri!.AbsolutePath);
            var rows = string.Join(",", Enumerable.Range(0, workspaceCount)
                .Select(index => $$"""{"id":"{{AccountPrefix}}/amw{{index}}","location":"eastus"}"""));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($$"""{"data":[{{rows}}],"resultTruncated":{{(areResultsTruncated ? "true" : "false")}}}""")
            });
        }

        Assert.Equal(HttpMethod.Get, request.Method);
        var path = request.RequestUri!.AbsolutePath;
        Assert.StartsWith(AccountPrefix + "/amw", path, StringComparison.Ordinal);
        Assert.EndsWith("/traceContainers/default", path, StringComparison.Ordinal);
        var indexText = path[(AccountPrefix.Length + "/amw".Length)..^"/traceContainers/default".Length];
        var enabled = int.Parse(indexText, System.Globalization.CultureInfo.InvariantCulture) % 2 == 0;
        return Task.FromResult(new HttpResponseMessage(enabled ? HttpStatusCode.OK : HttpStatusCode.NotFound)
        {
            Content = new StringContent(enabled
                ? $$$"""{"id":"{{{path}}}","name":"default","type":"Microsoft.Monitor/accounts/traceContainers","properties":{"traceDurationWindowInSeconds":3600}}"""
                : """{"error":{"code":"ResourceNotFound","message":"Not trace-enabled"}}""")
        });
    }
}
