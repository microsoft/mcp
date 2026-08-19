// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs.Resources;

public class DrillRunResourceGetCommandTests : CommandUnitTestsBase<DrillRunResourceGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Theory]
    [InlineData("")]
    [InlineData("--service-group sg1")]
    [InlineData("--drill drill1")]
    [InlineData("--service-group sg1 --drill drill1")]
    [InlineData("--service-group sg1 --drill-run run1")]
    [InlineData("--drill drill1 --drill-run run1")]
    public async Task ExecuteAsync_RejectsMissingRequiredOptions(string args)
    {
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDrillRunResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "target1"), new("id2", "target2") };
        Service.ListDrillRunResourcesAsync(ServiceGroup, Drill, DrillRun, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunResourceGetCommandResult);
        Assert.NotNull(result.DrillRunResources);
        Assert.Equal(2, result.DrillRunResources!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsDrillRunResource_WhenNameProvided()
    {
        Service.GetDrillRunResourceAsync(ServiceGroup, Drill, DrillRun, "target1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("target1"));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--name", "target1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunResourceGetCommandResult);
        Assert.Null(result.DrillRunResources);
        Assert.Equal("target1", result.DrillRunResource.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        const string expectedError = "Test error";
        Service.ListDrillRunResourcesAsync(ServiceGroup, Drill, DrillRun, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}