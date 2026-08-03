// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public class RecoveryPlanGetCommandTests : CommandUnitTestsBase<RecoveryPlanGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public async Task ExecuteAsync_ListsRecoveryPlans_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "plan1"), new("id2", "plan2") };
        Service.ListRecoveryPlansAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanGetCommandResult);
        Assert.NotNull(result.RecoveryPlans);
        Assert.Equal(2, result.RecoveryPlans!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsRecoveryPlan_WhenNameProvided()
    {
        Service.GetRecoveryPlanAsync(ServiceGroup, "plan1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("plan1"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--name", "plan1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanGetCommandResult);
        Assert.Null(result.RecoveryPlans);
        Assert.Equal("plan1", result.RecoveryPlan.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListRecoveryPlansAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
