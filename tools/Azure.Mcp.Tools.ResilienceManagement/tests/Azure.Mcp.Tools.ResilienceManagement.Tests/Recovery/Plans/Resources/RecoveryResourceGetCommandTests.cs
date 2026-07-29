// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans.Resources;

public class RecoveryResourceGetCommandTests : CommandUnitTestsBase<RecoveryResourceGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string RecoveryPlan = "plan1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public async Task ExecuteAsync_ListsRecoveryResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "member1"), new("id2", "member2") };
        Service.ListRecoveryResourcesAsync(ServiceGroup, RecoveryPlan, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryResourceGetCommandResult);
        Assert.NotNull(result.RecoveryResources);
        Assert.Equal(2, result.RecoveryResources!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsRecoveryResource_WhenNameProvided()
    {
        Service.GetRecoveryResourceAsync(ServiceGroup, RecoveryPlan, "member1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("member1"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan, "--name", "member1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryResourceGetCommandResult);
        Assert.Null(result.RecoveryResources);
        Assert.Equal("member1", result.RecoveryResource.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListRecoveryResourcesAsync(ServiceGroup, RecoveryPlan, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--recovery-plan", RecoveryPlan);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
