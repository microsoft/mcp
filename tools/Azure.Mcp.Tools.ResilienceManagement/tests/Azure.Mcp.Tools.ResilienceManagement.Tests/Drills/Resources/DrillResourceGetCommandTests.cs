// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Resources;

public class DrillResourceGetCommandTests : CommandUnitTestsBase<DrillResourceGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";

    private static JsonElement Element(string name)
        => JsonDocument.Parse($"{{\"id\":\"id1\",\"name\":\"{name}\"}}").RootElement.Clone();

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--service-group sg1 --drill drill1", true)]
    [InlineData("--service-group sg1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListDrillResourcesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDrillResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "target1"), new("id2", "target2") };
        Service.ListDrillResourcesAsync(ServiceGroup, Drill, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillResourceGetCommandResult);
        Assert.NotNull(result.DrillResources);
        Assert.Equal(2, result.DrillResources!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsDrillResource_WhenNameProvided()
    {
        Service.GetDrillResourceAsync(ServiceGroup, Drill, "target1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Element("target1"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill, "--name", "target1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillResourceGetCommandResult);
        Assert.Null(result.DrillResources);
        Assert.Equal("target1", result.DrillResource.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListDrillResourcesAsync(ServiceGroup, Drill, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
