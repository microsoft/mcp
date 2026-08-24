// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills;

public class DrillDeleteCommandTests : CommandUnitTestsBase<DrillDeleteCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--service-group sg1 --drill drill1", true)]
    [InlineData("--service-group sg1", false)]
    [InlineData("--drill drill1", false)]
    [InlineData("--service-group sg/1 --drill drill1", false)]
    [InlineData("--service-group sg1 --drill drill/1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesDrill()
    {
        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillDeleteCommandResult);
        Assert.True(result.Success);
        Assert.Contains(Drill, result.Message);
        await Service.Received(1).DeleteDrillAsync(
            ServiceGroup,
            Drill,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenException()
    {
        Service.DeleteDrillAsync(ServiceGroup, Drill, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Sensitive backend details"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.DoesNotContain("Sensitive backend details", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundException()
    {
        Service.DeleteDrillAsync(ServiceGroup, Drill, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Drill not found"));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--drill", Drill);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.StartsWith("Drill not found.", response.Message);
    }
}
