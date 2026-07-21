// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.IoTHub;

public class IoTHubGetCommandTests : SubscriptionCommandUnitTestsBase<IoTHubGetCommand, IIoTHubService>
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
    [InlineData("--subscription sub123 --hub-name hub1", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --hub-name hub1", true)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Ensure environment variable fallback does not interfere with validation tests
        TestEnvironment.ClearAzureSubscriptionId();

        if (shouldSucceed)
        {
            Service.GetIoTHub(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new IoTHubDescription(
                    Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1",
                    Name: "hub1",
                    Location: "eastus",
                        ResourceGroup: "rg1",
                        SubscriptionId: "sub123",
                        Sku: "S1",
                        Capacity: 1,
                        State: "Active",
                        HostName: "hub1.azure-devices.net")
                );
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("hub-name-")]
    [InlineData("hub!name")]
    [InlineData("hub' OR 1=1")]
    public async Task ExecuteAsync_RejectsInvalidIoTHubName(string invalidName)
    {
        // Ensure environment variable fallback does not interfere with validation tests
        TestEnvironment.ClearAzureSubscriptionId();

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", invalidName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--hub-name must be 3-50 characters long", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsOversizedIoTHubName()
    {
        // Ensure environment variable fallback does not interfere with validation tests
        TestEnvironment.ClearAzureSubscriptionId();

        var invalidName = new string('a', 51);
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", invalidName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--hub-name must be 3-50 characters long", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetIoTHub(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubDescription(
                Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1",
                Name: "hub1",
                Location: "eastus",
                ResourceGroup: "rg1",
                SubscriptionId: "sub123",
                Sku: "S1",
                Capacity: 1,
                State: "Active",
                HostName: "hub1.azure-devices.net")
            );

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        var result = ValidateAndDeserializeResponse(response, IoTHubJsonContext.Default.IoTHubGetCommandResult);
        Assert.NotNull(result.IoTHub);
        Assert.False(result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetIoTHub(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetIoTHub(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--hub-name", "hub1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }
}
