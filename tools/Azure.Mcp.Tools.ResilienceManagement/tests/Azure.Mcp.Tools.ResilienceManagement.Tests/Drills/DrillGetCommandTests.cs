// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills;

public class DrillGetCommandTests : SubscriptionCommandUnitTestsBase<DrillGetCommand, IResilienceManagementService>
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string ServiceGroup = "sg1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public async Task ExecuteAsync_ListsDrills_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "drill1"), new("id2", "drill2") };
        Service.ListDrillsAsync(ServiceGroup, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillGetCommandResult);
        Assert.NotNull(result.Drills);
        Assert.Equal(2, result.Drills!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsDrill_WhenNameProvided()
    {
        Service.GetDrillAsync(ServiceGroup, "drill1", SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("drill1"));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup, "--name", "drill1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillGetCommandResult);
        Assert.Null(result.Drills);
        Assert.Equal("drill1", result.Drill.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListDrillsAsync(ServiceGroup, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--service-group", ServiceGroup);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
