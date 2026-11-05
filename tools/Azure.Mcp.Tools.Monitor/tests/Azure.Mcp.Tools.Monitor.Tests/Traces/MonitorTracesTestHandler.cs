// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

internal sealed class MonitorTracesTestHandler(
    int workspaceCount,
    bool areResultsTruncated,
    string endpoint = "https://amw.data.monitor.azure.com",
    string cancelBody = """{"status":"Cancelled"}""",
    IEnumerable<HttpResponseMessage>? resultPages = null) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _resultPages = new(resultPages ?? []);
    internal const string Subscription = "00000000-0000-0000-0000-000000000001";
    internal const string AccountPrefix = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Monitor/accounts";

    public int DataPlaneCalls { get; private set; }
    public int SubmitCalls { get; private set; }
    public int PollCalls { get; private set; }
    public string? LastSubmitBody { get; private set; }
    public string? LastGraphQuery { get; private set; }
    public Uri? LastDataPlaneUri { get; private set; }
    public Uri? LastWorkspaceUri { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.Host != "management.azure.com")
        {
            DataPlaneCalls++;
            LastDataPlaneUri = request.RequestUri;
            if (request.RequestUri.AbsolutePath.EndsWith("/traces:query", StringComparison.Ordinal))
            {
                SubmitCalls++;
                LastSubmitBody = await request.Content!.ReadAsStringAsync(cancellationToken);
                return new(HttpStatusCode.Accepted) { Content = new StringContent("""{"operationId":"op-1234"}""") };
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/results", StringComparison.Ordinal))
            {
                PollCalls++;
                if (_resultPages.TryDequeue(out var page))
                {
                    return page;
                }
            }

            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(request.RequestUri.AbsolutePath.EndsWith(":cancel", StringComparison.Ordinal)
                    ? cancelBody
                    : """{"value":[],"traceFields":[],"spanFields":[]}""")
            };
        }

        if (request.Method == HttpMethod.Post)
        {
            Assert.Equal("/providers/Microsoft.ResourceGraph/resources", request.RequestUri!.AbsolutePath);
            using JsonDocument graphRequest = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            LastGraphQuery = graphRequest.RootElement.GetProperty("query").GetString();
            string rows = string.Join(",", Enumerable.Range(0, workspaceCount)
                .Select(index => $$"""{"id":"{{AccountPrefix}}/amw{{index}}","location":"eastus"}"""));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($$"""{"data":[{{rows}}],"resultTruncated":{{(areResultsTruncated ? "true" : "false")}}}""")
            };
        }

        Assert.Equal(HttpMethod.Get, request.Method);
        string path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/providers/Microsoft.Monitor/accounts/amw0", StringComparison.Ordinal))
        {
            LastWorkspaceUri = request.RequestUri;
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent($$$$"""{"id":"{{{{path}}}}","name":"amw0","type":"Microsoft.Monitor/accounts","properties":{"endpoints":{"query":"{{{{endpoint}}}}"}}}""")
            };
        }
        Assert.StartsWith(AccountPrefix + "/amw", path, StringComparison.Ordinal);
        Assert.EndsWith("/traceContainers/default", path, StringComparison.Ordinal);
        string indexText = path[(AccountPrefix.Length + "/amw".Length)..^"/traceContainers/default".Length];
        bool enabled = int.Parse(indexText, System.Globalization.CultureInfo.InvariantCulture) % 2 == 0;
        return new HttpResponseMessage(enabled ? HttpStatusCode.OK : HttpStatusCode.NotFound)
        {
            Content = new StringContent(enabled
                ? $$$"""{"id":"{{{path}}}","name":"default","type":"Microsoft.Monitor/accounts/traceContainers","properties":{"traceDurationWindowInSeconds":3600}}"""
                : """{"error":{"code":"ResourceNotFound","message":"Not trace-enabled"}}""")
        };
    }
}
