// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceRegistry.Commands;
using Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DeviceRegistry.UnitTests.Namespace;

public class NamespaceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDeviceRegistryService _deviceRegistryService;
    private readonly ILogger<NamespaceListCommand> _logger;
    private readonly NamespaceListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public NamespaceListCommandTests()
    {
        _deviceRegistryService = Substitute.For<IDeviceRegistryService>();
        _logger = Substitute.For<ILogger<NamespaceListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_deviceRegistryService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedNamespaces));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.NotNull(result);
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

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Is(resourceGroup),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedNamespaces));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Namespaces);
        Assert.Equal("adr-ns-01", result.Namespaces[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoNamespacesExist()
    {
        var subscription = "sub123";

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<DeviceRegistryNamespaceInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DeviceRegistryJsonContext.Default.NamespaceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Namespaces);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var subscription = "sub123";

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
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

            _deviceRegistryService.ListNamespacesAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedNamespaces));
        }

        var parseResult = _commandDefinition.Parse(args);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        var subscription = "sub123";

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        var subscription = "sub123";

        _deviceRegistryService.ListNamespacesAsync(
            Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
