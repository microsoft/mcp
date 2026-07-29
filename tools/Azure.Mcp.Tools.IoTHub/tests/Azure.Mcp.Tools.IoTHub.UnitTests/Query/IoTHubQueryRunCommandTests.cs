// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.Query;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Query;

public class IoTHubQueryRunCommandTests : SubscriptionCommandUnitTestsBase<IoTHubQueryRunCommand, IIoTHubDeviceService>
{
    private void SetupRunQuery(IoTHubQueryPage page) => Service.RunQuery(
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<int?>(),
        Arg.Any<string?>(),
        Arg.Any<string?>(),
        Arg.Any<RetryPolicyOptions?>(),
        Arg.Any<CancellationToken>())
        .Returns(page);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("run", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1 --query devices", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", false)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            SetupRunQuery(new IoTHubQueryPage([], null));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsQueryPage()
    {
        SetupRunQuery(new IoTHubQueryPage([], null));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        Assert.Equal(0, result.Count);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidMaxCount()
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices", "--max-count", "0");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.RunQuery(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1", "--query", "SELECT * FROM devices");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
