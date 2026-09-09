// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans.Enrollments;

public sealed class UsagePlanEnrollmentCreateCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanEnrollmentCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs =
        "--resource-group rg --usage-plan up1 --enrollment en1 --service-group sg --subscription sub";

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
    [InlineData("--usage-plan up1 --enrollment en1 --service-group sg --subscription sub", false)] // Missing resource group
    [InlineData("--resource-group rg --enrollment en1 --service-group sg --subscription sub", false)] // Missing usage plan
    [InlineData("--resource-group rg --usage-plan up1 --service-group sg --subscription sub", false)] // Missing enrollment
    [InlineData("--resource-group rg --usage-plan up1 --enrollment en1 --subscription sub", false)] // Missing service group
    [InlineData("--resource-group rg --usage-plan up1 --enrollment en1 --service-group sg", false)] // Missing subscription
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

    [Theory]
    [InlineData("usage-plan", "bad_name", "usage plan")]
    [InlineData("usage-plan", "1234567890123456789012345", "usage plan")]
    [InlineData("usage-plan", "../plan", "usage plan")]
    [InlineData("enrollment", "bad_name", "enrollment")]
    [InlineData("enrollment", "1234567890123456789012345", "enrollment")]
    [InlineData("enrollment", "../enrollment", "enrollment")]
    [InlineData("service-group", "bad#name", "service group")]
    [InlineData("service-group", "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901", "service group")]
    [InlineData("service-group", "../group", "service group")]
    public async Task ExecuteAsync_RejectsInvalidResourceName(string option, string value, string resourceType)
    {
        string[] args = [
            "--resource-group", "rg",
            "--usage-plan", option == "usage-plan" ? value : "up1",
            "--enrollment", option == "enrollment" ? value : "en1",
            "--service-group", option == "service-group" ? value : "sg",
            "--subscription", "sub"
        ];

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(resourceType, response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CreateUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
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
            Arg.Any<CancellationToken>())
            .Returns(new UsagePlanEnrollmentInfo("id1", "en1"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanEnrollmentCreateCommandResult);
        Assert.NotNull(result.Enrollment);
        Assert.Equal("en1", result.Enrollment.Name);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.CreateUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
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
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
