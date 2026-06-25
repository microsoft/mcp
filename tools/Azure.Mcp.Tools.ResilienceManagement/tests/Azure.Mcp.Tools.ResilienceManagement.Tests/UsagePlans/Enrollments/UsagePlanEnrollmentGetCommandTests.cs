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

public sealed class UsagePlanEnrollmentGetCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanEnrollmentGetCommand, IResilienceManagementService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--resource-group rg --usage-plan up1 --enrollment e1 --subscription sub", true)]
    [InlineData("--resource-group rg --usage-plan up1 --subscription sub", false)] // Missing enrollment
    [InlineData("--resource-group rg --enrollment e1 --subscription sub", false)] // Missing usage plan
    [InlineData("--usage-plan up1 --enrollment e1 --subscription sub", false)] // Missing resource group
    [InlineData("--resource-group rg --usage-plan up1 --enrollment e1", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GetUsagePlanEnrollmentAsync(
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
    public async Task ExecuteAsync_ReturnsEnrollment()
    {
        // Arrange
        Service.GetUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new UsagePlanEnrollmentInfo("id1", "e1"));

        // Act
        var response = await ExecuteCommandAsync("--resource-group", "rg", "--usage-plan", "up1", "--enrollment", "e1", "--subscription", "sub");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanEnrollmentGetCommandResult);
        Assert.NotNull(result.Enrollment);
        Assert.Equal("e1", result.Enrollment.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--resource-group", "rg", "--usage-plan", "up1", "--enrollment", "e1", "--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
