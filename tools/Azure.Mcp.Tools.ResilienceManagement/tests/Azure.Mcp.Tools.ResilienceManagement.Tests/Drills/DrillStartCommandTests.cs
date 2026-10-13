// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore testfailover

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

public sealed class DrillStartCommandTests : CommandUnitTestsBase<DrillStartCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--service-group sg1 --drill drill1 --mode Failover";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("start", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--service-group sg1 --drill drill1", false)]
    [InlineData("--service-group sg1 --mode Failover", false)]
    [InlineData("--drill drill1 --mode Failover", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.StartDrillAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns("operation-id");
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("Unsupported", "drill mode")]
    [InlineData("fail/over", "drill mode")]
    public async Task ExecuteAsync_RejectsInvalidMode(string mode, string expectedMessage)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--drill", "drill1", "--mode", mode);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IResilienceManagementService.StartDrillAsync));
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidResourceNames()
    {
        var response = await ExecuteCommandAsync("--service-group", "../sg1", "--drill", "drill/1", "--mode", "Failover");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("service group name", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("drill name", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_StartsDrillAndReturnsOperation()
    {
        Service.StartDrillAsync("sg1", "drill1", "TestFailover", null, Arg.Any<CancellationToken>())
            .Returns("operation-id");

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--drill", "drill1", "--mode", "testfailover");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillStartCommandResult);
        Assert.Equal("operation-id", result.OperationId);
        Assert.Equal("drill1", result.Drill);
        Assert.Equal("Accepted", result.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123";
        Service.StartDrillAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
