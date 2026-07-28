// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.IoTOperations.Commands;
using Azure.Mcp.Tools.IoTOperations.Commands.Instance;
using Azure.Mcp.Tools.IoTOperations.Models;
using Azure.Mcp.Tools.IoTOperations.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.IoTOperations.Tests.Instance;

public class InstanceGetCommandTests : SubscriptionCommandUnitTestsBase<InstanceGetCommand, IIoTOperationsService>
{
    private const string Subscription = "sub123";
    private const string ResourceGroup = "rg1";
    private const string InstanceName = "aio-01";

    private static IoTOperationsInstanceInfo CreateInstance() =>
        new(
            Name: InstanceName,
            Id: $"/subscriptions/{Subscription}/resourceGroups/{ResourceGroup}/providers/Microsoft.IoTOperations/instances/{InstanceName}",
            Location: "eastus2",
            ResourceGroup: ResourceGroup,
            Type: "microsoft.iotoperations/instances",
            ProvisioningState: "Succeeded",
            Version: "1.4.62",
            Description: "An AIO instance.",
            SchemaRegistryResourceId: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DeviceRegistry/schemaRegistries/sr1");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInstance_WhenValidArgsProvided()
    {
        Service.GetInstanceAsync(
            Arg.Is(Subscription),
            Arg.Is(ResourceGroup),
            Arg.Is(InstanceName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(CreateInstance());

        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--instance", InstanceName);

        var result = ValidateAndDeserializeResponse(response, IoTOperationsJsonContext.Default.InstanceGetCommandResult);

        Assert.NotNull(result.Instance);
        Assert.Equal(InstanceName, result.Instance.Name);
        Assert.Equal("Succeeded", result.Instance.ProvisioningState);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --instance aio-01", true)]
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing instance
    [InlineData("--subscription sub123 --instance aio-01", false)] // missing resource group
    [InlineData("--subscription sub123 --resource-group rg1 --instance AB", false)] // invalid name (too short, uppercase)
    [InlineData("--subscription sub123 --resource-group rg1 --instance -bad-", false)] // invalid name (leading/trailing hyphen)
    [InlineData("", false)] // missing everything
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetInstanceAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(CreateInstance());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.NotEmpty(response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotFound_WhenInstanceMissing()
    {
        Service.GetInstanceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("IoT Operations instance 'aio-01' not found."));

        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--instance", InstanceName);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetInstanceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--instance", InstanceName);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        Service.GetInstanceAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var response = await ExecuteCommandAsync(
            "--subscription", Subscription,
            "--resource-group", ResourceGroup,
            "--instance", InstanceName);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
