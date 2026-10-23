// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
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

public sealed class DrillCheckResyncReadinessCommandTests
    : CommandUnitTestsBase<DrillCheckResyncReadinessCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string ValidArgs = "--service-group sg1 --drill drill1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("check-resync-readiness", command.Name);
        Assert.Contains("resync", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--drill drill1", false)]
    [InlineData("--service-group sg1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CheckDrillResyncReadinessAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new DrillResyncReadinessResult("operation1", false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_StartsCheckAndReturnsOperation()
    {
        Service.CheckDrillResyncReadinessAsync(
            ServiceGroup,
            Drill,
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(new DrillResyncReadinessResult("operation1", false));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillCheckResyncReadinessCommandResult);
        Assert.Equal("operation1", result.Readiness.OperationId);
        Assert.False(result.Readiness.HasCompleted);
    }

    [Theory]
    [InlineData("--service-group", "sg/1")]
    [InlineData("--drill", "drill/1")]
    public async Task ExecuteAsync_RejectsInvalidPathSegments(string invalidOption, string invalidValue)
    {
        string serviceGroup = invalidOption == "--service-group" ? invalidValue : ServiceGroup;
        string drill = invalidOption == "--drill" ? invalidValue : Drill;

        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--drill", drill);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
        await Service.DidNotReceive().CheckDrillResyncReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.CheckDrillResyncReadinessAsync(
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
