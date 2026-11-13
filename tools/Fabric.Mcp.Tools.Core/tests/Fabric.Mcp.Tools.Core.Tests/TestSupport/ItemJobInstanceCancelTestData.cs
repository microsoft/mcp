// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemJobInstanceCancelTestData
{
    internal const string ToolName = "jobs_cancel-item-job-instance";
    internal const string Location = $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{JobInstanceId}";
    internal const string AlreadyCompleted = """{"errorCode":"JobAlreadyCompleted","message":"private-backend","moreDetails":[{"message":"private-detail"}],"isRetriable":true}""";

    internal static HttpResponseMessage Accepted(string? retryAfter = "60", HttpContent? content = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = content };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }

    internal static ItemJobInstanceCancelCommandResult Receipt() =>
        new(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), Guid.Parse(JobInstanceId), 60, null, null);

    internal static Task<ItemJobInstanceCancelCommandResult> CancelAsync(FabricCoreService service, CancellationToken? token = null) =>
        service.CancelItemJobInstanceAsync(WorkspaceId, ItemId, JobInstanceId, token ?? TestContext.Current.CancellationToken);
}
