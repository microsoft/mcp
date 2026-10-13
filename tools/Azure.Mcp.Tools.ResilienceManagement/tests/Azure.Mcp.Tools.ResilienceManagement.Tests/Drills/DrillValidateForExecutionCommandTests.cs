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

public class DrillValidateForExecutionCommandTests
    : CommandUnitTestsBase<DrillValidateForExecutionCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("validate-for-execution", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--service-group sg1 --drill drill1 --source-locations eastus-az1", true)]
    [InlineData("--drill drill1 --source-locations eastus-az1", false)]
    [InlineData("--service-group sg1 --source-locations eastus-az1", false)]
    [InlineData("--service-group sg1 --drill drill1", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ValidateDrillForExecutionAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new DrillValidateForExecutionResult("operation1", false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_StartsValidationAndReturnsOperation()
    {
        Service.ValidateDrillForExecutionAsync(
            ServiceGroup,
            Drill,
            Arg.Is<IEnumerable<string>>(locations => locations.SequenceEqual(new[] { "eastus-az1", "westus-az2" })),
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(new DrillValidateForExecutionResult("operation1", false));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--source-locations", "eastus-az1",
            "--source-locations", "westus-az2",
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillValidateForExecutionCommandResult);
        Assert.Equal("operation1", result.Validation.OperationId);
        Assert.False(result.Validation.HasCompleted);
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
            "--drill", drill,
            "--source-locations", "eastus-az1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        Service.ValidateDrillForExecutionAsync(
            ServiceGroup,
            Drill,
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--source-locations", "eastus-az1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundException()
    {
        Service.ValidateDrillForExecutionAsync(
            ServiceGroup,
            Drill,
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Not found"));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--source-locations", "eastus-az1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.StartsWith("Drill not found", response.Message);
    }
}
