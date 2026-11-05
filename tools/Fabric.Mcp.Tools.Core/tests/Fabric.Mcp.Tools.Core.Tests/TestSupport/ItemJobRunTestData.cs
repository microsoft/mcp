// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemJobRunTestData
{
    internal const string ToolName = "jobs_run-on-demand-item-job";
    internal const string JobType = "Execute";
    internal const string Location = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{JobInstanceId}";
    internal const string Parameters = """[{"name":"Threshold","type":"FutureType","value":{"nested":[null,true,1.25,"private-value"]},"extension":false}]""";

    internal static HttpResponseMessage Accepted(string? location = Location, string? retryAfter = "60", HttpContent? content = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = content };
        if (location is not null)
        {
            response.Headers.TryAddWithoutValidation("Location", location);
        }
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }

    internal static ItemJobRunCommandResult Receipt(Guid? jobId = null) =>
        new(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), JobType, jobId, 60, null,
            jobId is null ? "MissingLocation" : null, null);

    internal static Task<ItemJobRunCommandResult> RunAsync(
        FabricCoreService service, string? executionData = null, string? parameters = null, CancellationToken? token = null) =>
        service.RunOnDemandItemJobAsync(WorkspaceId, ItemId, JobType, executionData, parameters, token ?? TestContext.Current.CancellationToken);
}
