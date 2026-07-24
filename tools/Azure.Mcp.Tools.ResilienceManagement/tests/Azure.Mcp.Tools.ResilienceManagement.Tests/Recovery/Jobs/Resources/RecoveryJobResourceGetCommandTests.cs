// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Jobs.Resources;

public class RecoveryJobResourceGetCommandTests : CommandUnitTestsBase<RecoveryJobResourceGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string RecoveryPlan = "plan1";
    private const string RecoveryJob = "job1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public async Task ExecuteAsync_ListsRecoveryJobResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "target1"), new("id2", "target2") };
        Service.ListRecoveryJobResourcesAsync(ServiceGroup, RecoveryPlan, RecoveryJob, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan, "--recovery-job", RecoveryJob);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryJobResourceGetCommandResult);
        Assert.NotNull(result.RecoveryJobResources);
        Assert.Equal(2, result.RecoveryJobResources!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsRecoveryJobResource_WhenNameProvided()
    {
        Service.GetRecoveryJobResourceAsync(ServiceGroup, RecoveryPlan, RecoveryJob, "target1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("target1"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan, "--recovery-job", RecoveryJob, "--name", "target1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryJobResourceGetCommandResult);
        Assert.Null(result.RecoveryJobResources);
        Assert.Equal("target1", result.RecoveryJobResource.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListRecoveryJobResourcesAsync(ServiceGroup, RecoveryPlan, RecoveryJob, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan, "--recovery-job", RecoveryJob);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
