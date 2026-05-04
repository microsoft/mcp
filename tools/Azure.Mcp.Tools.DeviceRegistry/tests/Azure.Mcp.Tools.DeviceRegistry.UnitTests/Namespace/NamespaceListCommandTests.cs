// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceRegistry.Commands;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DeviceRegistry.UnitTests.Namespace;

public class NamespaceListCommandTests : CommandUnitTestsBase<NamespaceListCommand, IDeviceRegistryService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsNamespaces_WhenSubscriptionProvided()
    {
        var subscription = "sub123";
        var expectedNamespaces = new ResourceQueryResults<DeviceRegistryNamespaceInfo>(
        [
            new("adr-ns-01", "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DeviceRegistry/namespaces/adr-ns-01",
                "North Europe", "Succeeded", "cefe124a-6971-4c90-a7a9-99be82def1ab", "rg1", "Microsoft.DeviceRegistry/namespaces"),
            new("adr-ns-02", "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DeviceRegistry/namespaces/adr-ns-02",
                "West US", "Succeeded", "defe124a-6971-4c90-a7a9-99be82def2ab", "rg1", "Microsoft.DeviceRegistry/namespaces")
        ], false);

        Service.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedNamespaces);

        var response = await ExecuteCommandAsync("--subscription", subscription);

        var result = ValidateAndDeserializeResponse(response, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.NotNull(result.Namespaces);
        Assert.Equal(expectedNamespaces.Results.Count, result.Namespaces.Count);
        Assert.Equal(expectedNamespaces.Results.Select(n => n.Name), result.Namespaces.Select(n => n.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNamespaces_WhenResourceGroupProvided()
    {
        var subscription = "sub123";
        var resourceGroup = "myRG";
        var expectedNamespaces = new ResourceQueryResults<DeviceRegistryNamespaceInfo>(
        [
            new("adr-ns-01", "/subscriptions/sub123/resourceGroups/myRG/providers/Microsoft.DeviceRegistry/namespaces/adr-ns-01",
                "North Europe", "Succeeded", "cefe124a-6971-4c90-a7a9-99be82def1ab", "myRG", "Microsoft.DeviceRegistry/namespaces")
        ], false);

        Service.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Is(resourceGroup),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedNamespaces);

        var response = await ExecuteCommandAsync("--subscription", subscription, "--resource-group", resourceGroup);

        var result = ValidateAndDeserializeResponse(response, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.Single(result.Namespaces);
        Assert.Equal("adr-ns-01", result.Namespaces[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoNamespacesExist()
    {
        var subscription = "sub123";

        Service.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<DeviceRegistryNamespaceInfo>([], false));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        var result = ValidateAndDeserializeResponse(response, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.Empty(result.Namespaces);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var subscription = "sub123";

        Service.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --resource-group myRG", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedNamespaces = new ResourceQueryResults<DeviceRegistryNamespaceInfo>(
            [
                new("adr-ns-01", "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DeviceRegistry/namespaces/adr-ns-01",
                    "North Europe", "Succeeded", "cefe124a-6971-4c90-a7a9-99be82def1ab", "rg1", "Microsoft.DeviceRegistry/namespaces")
            ], false);

            Service.ListNamespacesAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedNamespaces);
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

        Service.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        var subscription = "sub123";

        Service.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        var subscription = "sub123";

        Service.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
