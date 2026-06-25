// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Resources;

public class DrillResourceGetCommandTests : SubscriptionCommandUnitTestsBase<DrillResourceGetCommand, IResilienceManagementService>
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public async Task ExecuteAsync_ListsDrillResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "target1"), new("id2", "target2") };
        Service.ListDrillResourcesAsync(ServiceGroup, Drill, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup, "--drill", Drill);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillResourceGetCommandResult);
        Assert.NotNull(result.DrillResources);
        Assert.Equal(2, result.DrillResources!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsDrillResource_WhenNameProvided()
    {
        Service.GetDrillResourceAsync(ServiceGroup, Drill, "target1", SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("target1"));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup, "--drill", Drill, "--name", "target1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillResourceGetCommandResult);
        Assert.Null(result.DrillResources);
        Assert.Equal("target1", result.DrillResource.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListDrillResourcesAsync(ServiceGroup, Drill, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup, "--drill", Drill);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
