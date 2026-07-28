// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
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

public class InstanceListCommandTests : SubscriptionCommandUnitTestsBase<InstanceListCommand, IIoTOperationsService>
{
    private static IoTOperationsInstanceInfo CreateInstance(string name, string resourceGroup = "rg1") =>
        new(
            Name: name,
            Id: $"/subscriptions/sub123/resourceGroups/{resourceGroup}/providers/Microsoft.IoTOperations/instances/{name}",
            Location: "eastus2",
            ResourceGroup: resourceGroup,
            Type: "microsoft.iotoperations/instances",
            ProvisioningState: "Succeeded",
            Version: "1.4.62",
            Description: "An AIO instance.",
            SchemaRegistryResourceId: null);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInstances_WhenSubscriptionProvided()
    {
        var subscription = "sub123";
        var expected = new ResourceQueryResults<IoTOperationsInstanceInfo>(
            [CreateInstance("aio-01"), CreateInstance("aio-02")], false);

        Service.ListInstancesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", subscription);

        var result = ValidateAndDeserializeResponse(response, IoTOperationsJsonContext.Default.InstanceListCommandResult);

        Assert.NotNull(result.Instances);
        Assert.Equal(expected.Results.Count, result.Instances.Count);
        Assert.Equal(expected.Results.Select(i => i.Name), result.Instances.Select(i => i.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInstances_WhenResourceGroupProvided()
    {
        var subscription = "sub123";
        var resourceGroup = "myRG";
        var expected = new ResourceQueryResults<IoTOperationsInstanceInfo>(
            [CreateInstance("aio-01", resourceGroup)], false);

        Service.ListInstancesAsync(
            Arg.Is(subscription),
            Arg.Is(resourceGroup),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", subscription, "--resource-group", resourceGroup);

        var result = ValidateAndDeserializeResponse(response, IoTOperationsJsonContext.Default.InstanceListCommandResult);

        Assert.Single(result.Instances);
        Assert.Equal("aio-01", result.Instances[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoInstancesExist()
    {
        var subscription = "sub123";

        Service.ListInstancesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<IoTOperationsInstanceInfo>([], false));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        var result = ValidateAndDeserializeResponse(response, IoTOperationsJsonContext.Default.InstanceListCommandResult);

        Assert.Empty(result.Instances);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --resource-group myRG", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListInstancesAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<IoTOperationsInstanceInfo>([CreateInstance("aio-01")], false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var subscription = "sub123";

        Service.ListInstancesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        var subscription = "sub123";

        Service.ListInstancesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        var subscription = "sub123";

        Service.ListInstancesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
