// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
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
                Arg.Any<RetryPolicyOptions?>(),
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
            Arg.Any<RetryPolicyOptions?>(),
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
    public async Task ExecuteAsync_HandlesUsagePlanAlreadyExists()
    {
        // Arrange
        Service.CreateUsagePlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UsagePlanKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Usage plan name already exists"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
