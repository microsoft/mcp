// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans.Enrollments;

public sealed class UsagePlanEnrollmentCreateCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanEnrollmentCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs =
        "--resource-group rg --usage-plan up1 --enrollment e1 --service-group sg --subscription sub";

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
    [InlineData("--usage-plan up1 --enrollment e1 --service-group sg --subscription sub", false)] // Missing resource group
    [InlineData("--resource-group rg --enrollment e1 --service-group sg --subscription sub", false)] // Missing usage plan
    [InlineData("--resource-group rg --usage-plan up1 --service-group sg --subscription sub", false)] // Missing enrollment
    [InlineData("--resource-group rg --usage-plan up1 --enrollment e1 --subscription sub", false)] // Missing service group
    [InlineData("--resource-group rg --usage-plan up1 --enrollment e1 --service-group sg", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.CreateUsagePlanEnrollmentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new UsagePlanEnrollmentInfo("id1", "e1"));
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
    public async Task ExecuteAsync_ReturnsCreatedEnrollment()
    {
        // Arrange
        Service.CreateUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new UsagePlanEnrollmentInfo("id1", "e1"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanEnrollmentCreateCommandResult);
        Assert.NotNull(result.Enrollment);
        Assert.Equal("e1", result.Enrollment.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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
