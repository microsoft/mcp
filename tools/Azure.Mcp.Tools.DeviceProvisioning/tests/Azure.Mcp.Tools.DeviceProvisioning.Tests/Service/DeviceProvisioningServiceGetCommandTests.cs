// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.DeviceProvisioning.Commands;
using Azure.Mcp.Tools.DeviceProvisioning.Commands.Service;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Service;

public class DeviceProvisioningServiceGetCommandTests
    : SubscriptionCommandUnitTestsBase<DeviceProvisioningServiceGetCommand, IDeviceProvisioningService>
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
    [InlineData("--subscription sub123 --service dps1", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --service dps1", true)]
    [InlineData("--subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(
        string args,
        bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetService(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>())
                .Returns(CreateDescription());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(
            shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
            response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLowerInvariant());
        }
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("-dps-name")]
    [InlineData("dps-name-")]
    [InlineData("dps!name")]
    [InlineData("dps' OR 1=1")]
    public async Task ExecuteAsync_RejectsInvalidServiceName(string invalidName)
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            invalidName);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(
            "--service must be 3-64 characters long",
            response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsOversizedServiceName()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            new string('a', 65));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(
            "--service must be 3-64 characters long",
            response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetService(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateDescription());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1");

        var result = ValidateAndDeserializeResponse(
            response,
            DeviceProvisioningJsonContext.Default.DeviceProvisioningServiceGetCommandResult);
        Assert.Equal("dps1", result.DeviceProvisioningService.Name);
        Assert.Single(result.DeviceProvisioningService.IotHubs);
        Assert.False(result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetService(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        Service.GetService(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.NotFound,
                "Resource not found"));

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    private static DeviceProvisioningServiceDescription CreateDescription()
    {
        return new DeviceProvisioningServiceDescription(
            Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/dps1",
            Name: "dps1",
            Location: "eastus",
            ResourceGroup: "rg1",
            SubscriptionId: "sub123",
            Sku: "S1",
            Capacity: 1,
            State: "Active",
            ProvisioningState: "Succeeded",
            ServiceOperationsHostName: "dps1.azure-devices-provisioning.net",
            DeviceProvisioningHostName: "global.azure-devices-provisioning.net",
            IdScope: "0ne00000001",
            AllocationPolicy: "Hashed",
            PublicNetworkAccess: "Enabled",
            EnableDataResidency: false,
            IotHubs:
            [
                new LinkedIoTHubDescription(
                    "hub1.azure-devices.net",
                    "eastus",
                    true,
                    1)
            ]);
    }
}
