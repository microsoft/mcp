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

public sealed class DrillEndCommandTests : CommandUnitTestsBase<DrillEndCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--service-group sg1 --drill drill1 --attestation Success --attestation-notes completed";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("end", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--service-group sg1 --drill drill1 --attestation Success", false)]
    [InlineData("--service-group sg1 --drill drill1 --attestation-notes completed", false)]
    [InlineData("--service-group sg1 --attestation Success --attestation-notes completed", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.EndDrillAsync(
                Arg.Any<string>(),
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

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidAttestation()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--drill", "drill1",
            "--attestation", "Unknown",
            "--attestation-notes", "completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("drill attestation", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IResilienceManagementService.EndDrillAsync));
    }

    [Fact]
    public async Task ExecuteAsync_EndsDrillAndReturnsOperation()
    {
        Service.EndDrillAsync("sg1", "drill1", "Failed", "test notes", null, Arg.Any<CancellationToken>())
            .Returns("operation-id");

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--drill", "drill1",
            "--attestation", "failed",
            "--attestation-notes", "test notes");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillEndCommandResult);
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
        Service.EndDrillAsync(
            Arg.Any<string>(),
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
