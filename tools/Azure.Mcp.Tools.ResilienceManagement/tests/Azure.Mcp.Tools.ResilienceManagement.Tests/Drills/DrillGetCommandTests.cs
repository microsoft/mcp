// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills;

public class DrillGetCommandTests : CommandUnitTestsBase<DrillGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";

    private static DrillInfo Drill(string name)
        => new("id1", name);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--service-group sg1", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListDrillsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDrills_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "drill1"), new("id2", "drill2") };
        Service.ListDrillsAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillGetCommandResult);
        Assert.NotNull(result.Drills);
        Assert.Equal(2, result.Drills!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_GetsDrill_WhenNameProvided()
    {
        Service.GetDrillAsync(ServiceGroup, "drill1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Drill("drill1"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--name", "drill1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillGetCommandResult);
        Assert.Null(result.Drills);
        Assert.NotNull(result.Drill);
        Assert.Equal("drill1", result.Drill!.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListDrillsAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
