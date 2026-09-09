// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans;

public sealed class UsagePlanDeleteCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanDeleteCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--resource-group rg --usage-plan up1 --subscription sub";

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
    [InlineData("--usage-plan up1 --subscription sub", false)]
    [InlineData("--resource-group rg --subscription sub", false)]
    [InlineData("--resource-group rg --usage-plan up1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.DeleteUsagePlanAsync(
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
    [InlineData("ab")]
    [InlineData("1234567890123456789012345")]
    [InlineData("bad_name")]
    [InlineData("../plan")]
    public async Task ExecuteAsync_RejectsInvalidUsagePlanName(string usagePlan)
    {
        var response = await ExecuteCommandAsync(
            "--resource-group", "rg",
            "--usage-plan", usagePlan,
            "--subscription", "sub");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("3 to 24 characters", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ASCII letters, numbers, or hyphens", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().DeleteUsagePlanAsync(
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
        Service.DeleteUsagePlanAsync(
            "rg",
            "up1",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(deleted);

        var response = await ExecuteCommandAsync($"{ValidArgs} --tenant tenant");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.UsagePlanDeleteCommandResult);
        Assert.Equal(deleted, result.Deleted);
        Assert.Equal("up1", result.UsagePlan);
        await Service.Received(1).DeleteUsagePlanAsync(
            "rg",
            "up1",
            "sub",
            "tenant",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "dependent enrollments")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.DeleteUsagePlanAsync(
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
        Service.DeleteUsagePlanAsync(
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
