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

public sealed class DrillCreateCommandTests : CommandUnitTestsBase<DrillCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--service-group sg1 --drill drill1 --subscription sub --region westus2 --resource-group rg --drill-type Zonal --rbac-setup-mode AutomatedBuiltinRoles";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--drill drill1 --subscription sub --region westus2 --drill-type Zonal --rbac-setup-mode Manual", false)]
    [InlineData("--service-group sg1 --subscription sub --region westus2 --drill-type Zonal --rbac-setup-mode Manual", false)]
    [InlineData("--service-group sg1 --drill drill1 --region westus2 --drill-type Zonal --rbac-setup-mode Manual", false)]
    [InlineData("--service-group sg1 --drill drill1 --subscription sub --drill-type Zonal --rbac-setup-mode Manual", false)]
    [InlineData("")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed = false)
    {
        if (shouldSucceed)
        {
            ConfigureCreatedDrill();
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDrillNameContainingPathSeparator()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--drill", "../drill",
            "--subscription", "sub",
            "--region", "westus2",
            "--drill-type", "Zonal",
            "--rbac-setup-mode", "Manual");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single path segment", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedDrill()
    {
        ConfigureCreatedDrill();

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillCreateCommandResult);
        Assert.Equal("drill1", result.Drill.Name);
        await Service.Received(1).CreateDrillAsync(
            "sg1",
            "drill1",
            "sub",
            "westus2",
            "rg",
            DrillKind.Zonal,
            DrillRbacSetupMode.AutomatedBuiltinRoles,
            null,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "conflicts with the current resource state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        Service.CreateDrillAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DrillKind>(),
            Arg.Any<DrillRbacSetupMode>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, "Sensitive provider details"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sensitive provider details", response.Message);
    }

    private void ConfigureCreatedDrill()
    {
        Service.CreateDrillAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<DrillKind>(),
            Arg.Any<DrillRbacSetupMode>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DrillInfo("id1", "drill1"));
    }
}