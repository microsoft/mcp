// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans;

public sealed class UsagePlanCreateCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--resource-group rg --usage-plan up1 --plan-type Basic --subscription sub";

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
    [InlineData("--usage-plan up1 --plan-type Basic --subscription sub", false)] // Missing resource group
    [InlineData("--resource-group rg --plan-type Basic --subscription sub", false)] // Missing usage plan
    [InlineData("--resource-group rg --usage-plan up1 --plan-type Basic", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.CreateUsagePlanAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<UsagePlanKind>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new UsagePlanInfo("id1", "up1", "Microsoft.ResilienceManagement/usagePlans", "eastus"));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Theory]
    [InlineData("bad_name")]
    [InlineData("1234567890123456789012345")]
    [InlineData("../plan")]
    public async Task ExecuteAsync_RejectsInvalidUsagePlanName(string usagePlan)
    {
        var response = await ExecuteCommandAsync(
            "--resource-group", "rg",
            "--usage-plan", usagePlan,
            "--plan-type", "Basic",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("3 to 24 characters", response.Message);
        await Service.DidNotReceive().CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedUsagePlan()
    {
        // Arrange
        Service.CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new UsagePlanInfo("id1", "up1", "Microsoft.ResilienceManagement/usagePlans", "eastus"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanCreateCommandResult);
        Assert.NotNull(result.UsagePlan);
        Assert.Equal("up1", result.UsagePlan.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWithoutAssumingNameCollision()
    {
        // Arrange
        Service.CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Provider-specific conflict details"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflicts with the current resource state", response.Message);
        Assert.DoesNotContain("Provider-specific conflict details", response.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "Resource group not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
