// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans.Enrollments;

public sealed class UsagePlanEnrollmentDeleteCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanEnrollmentDeleteCommand, IResilienceManagementService>
{
    private const string ValidArgs =
        "--resource-group rg --usage-plan up1 --enrollment en1 --subscription sub";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--usage-plan up1 --enrollment en1 --subscription sub", false)]
    [InlineData("--resource-group rg --enrollment en1 --subscription sub", false)]
    [InlineData("--resource-group rg --usage-plan up1 --subscription sub", false)]
    [InlineData("--resource-group rg --usage-plan up1 --enrollment en1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.DeleteUsagePlanEnrollmentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("usage-plan", "ab", "usage plan")]
    [InlineData("usage-plan", "1234567890123456789012345", "usage plan")]
    [InlineData("usage-plan", "bad_name", "usage plan")]
    [InlineData("usage-plan", "../plan", "usage plan")]
    [InlineData("enrollment", "ab", "enrollment")]
    [InlineData("enrollment", "1234567890123456789012345", "enrollment")]
    [InlineData("enrollment", "bad_name", "enrollment")]
    [InlineData("enrollment", "../enrollment", "enrollment")]
    public async Task ExecuteAsync_RejectsInvalidResourceName(string option, string value, string resourceType)
    {
        var response = await ExecuteCommandAsync(
            "--resource-group", "rg",
            "--usage-plan", option == "usage-plan" ? value : "up1",
            "--enrollment", option == "enrollment" ? value : "en1",
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(resourceType, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3 to 24 characters", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().DeleteUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ReturnsDeleteResult(bool deleted)
    {
        Service.DeleteUsagePlanEnrollmentAsync(
            "rg",
            "up1",
            "en1",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(deleted);

        var response = await ExecuteCommandAsync($"{ValidArgs} --tenant tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.UsagePlanEnrollmentDeleteCommandResult);
        Assert.Equal(deleted, result.Deleted);
        Assert.Equal("up1", result.UsagePlan);
        Assert.Equal("en1", result.Enrollment);
        await Service.Received(1).DeleteUsagePlanEnrollmentAsync(
            "rg",
            "up1",
            "en1",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.DeleteUsagePlanEnrollmentAsync(
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
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        Service.DeleteUsagePlanEnrollmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Internal timeout details"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Internal timeout details", response.Message);
    }
}
