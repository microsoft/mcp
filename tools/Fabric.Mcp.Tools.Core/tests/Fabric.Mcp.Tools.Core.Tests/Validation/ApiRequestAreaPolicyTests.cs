// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Areas.Server;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class ApiRequestAreaPolicyTests
{
    [Theory]
    [InlineData("/v1/workspaces/w/items/i/jobs/instances?jobType=DefaultJob")]
    [InlineData("/v1/workspaces/w/items/i/jobs/DefaultJob/schedules")]
    [InlineData("/v1/workspaces/w/sparkJobDefinitions/i/JOBS/instances")]
    [InlineData("/v1/workspaces/w/items/i/j%6Fbs/instances")]
    [InlineData("/v1/workspaces/w/items/i/jobs;x=1/instances")]
    public void IsJobsPath_DetectsJobsSegments(string path) => Assert.True(ApiRequestAreaPolicy.IsJobsPath(path));

    [Theory]
    [InlineData("/v1/workspaces/w/copyJobs/i")]
    [InlineData("/v1/workspaces/w/items?type=jobs")]
    [InlineData("/v1/connections")]
    [InlineData(null)]
    public void IsJobsPath_AllowsOtherEndpoints(string? path) => Assert.False(ApiRequestAreaPolicy.IsJobsPath(path));

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(new[] { "core", "jobs" }, null, true)]
    [InlineData(new[] { "core", "JOBS" }, null, true)]
    [InlineData(new[] { "core" }, null, false)]
    [InlineData(null, new[] { "core_get-api-resource" }, false)]
    [InlineData(null, new[] { "core_get-api-resource", "jobs_get-item-schedule" }, true)]
    public void GetDisabledAreaError_RejectsJobsPathsOnlyWhenJobsAreaIsHidden(string[]? namespaces, string[]? tools, bool allowed)
    {
        var configuration = new ServerRuntimeConfiguration { Namespace = namespaces, Tool = tools };

        var error = ApiRequestAreaPolicy.GetDisabledAreaError("/v1/workspaces/w/items/i/jobs/instances", configuration);

        Assert.Equal(allowed, error is null);
        Assert.Null(ApiRequestAreaPolicy.GetDisabledAreaError("/v1/connections", configuration));
    }
}
