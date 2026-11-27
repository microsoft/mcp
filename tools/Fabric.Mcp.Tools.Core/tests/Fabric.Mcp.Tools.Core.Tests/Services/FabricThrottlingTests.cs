// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricThrottlingTests()
{
    public static TheoryData<string, string?, string?> RetryAfterCases
    {
        get
        {
            var cases = new TheoryData<string, string?, string?>();
            foreach (var operation in new[] { "list-capacities", "get-capacity", "get-workspace", "list-workspaces", "delete-workspace" })
            {
                cases.Add(operation, "0", "Wait at least 0 seconds before retrying");
                cases.Add(operation, "120", "Wait at least 120 seconds before retrying");
                cases.Add(operation, "2147483647", "Wait at least 2147483647 seconds before retrying");
                cases.Add(operation, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC");
                cases.Add(operation, null, null);
                cases.Add(operation, "private-header-detail", null);
                cases.Add(operation, "-1", null);
                cases.Add(operation, "1.5", null);
                cases.Add(operation, "2147483648", null);
                cases.Add(operation, "999999999999999999999999999999", null);
                cases.Add(operation, "120, 240", null);
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(RetryAfterCases))]
    public async Task CommonOperations_PreserveSharedThrottlingAndInvalidHeaderFallback(
        string operation, string? header, string? guidance)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("private-backend-detail")
        };
        if (header is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", header));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            InvokeAsync(service, operation, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        if (guidance is not null)
        {
            Assert.IsType<FabricThrottledException>(exception);
            Assert.Equal($"Fabric throttled the request. {guidance}", exception.Message);
        }
        else
        {
            Assert.IsType<HttpRequestException>(exception);
            Assert.DoesNotContain("Retry", exception.Message);
        }
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    private static Task InvokeAsync(FabricCoreService service, string operation, CancellationToken cancellationToken) => operation switch
    {
        "list-capacities" => service.ListCapacitiesAsync(cancellationToken: cancellationToken),
        "get-capacity" => service.GetCapacityAsync(WorkspaceTestData.WorkspaceId, cancellationToken),
        "get-workspace" => service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: cancellationToken),
        "list-workspaces" => service.ListWorkspacesAsync(cancellationToken: cancellationToken),
        "delete-workspace" => service.DeleteWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
}
