// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.Hostpool;

public class HostpoolListCommandTests : CommandUnitTestsBase<HostpoolListCommand, IVirtualDesktopService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Equal("List hostpools", Command.Title);
    }

    [Theory]
    [InlineData("--subscription test-sub", true)]
    [InlineData("--subscription test-sub --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --resource-group test-rg", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --tenant test-tenant", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var hostpools = new List<HostPool>
            {
                new() { Name = "hostpool1" },
                new() { Name = "hostpool2" }
            }.AsReadOnly();
            Service.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(hostpools);
            Service.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(hostpools);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message?.ToLower() ?? "");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResult_WhenNoHostpools()
    {
        // Arrange
        Service.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);
        Service.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHostpools_WhenSuccessful()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        Service.ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedHostpools);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsResourceGroupService_WhenResourceGroupProvided()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        Service.ListHostpoolsByResourceGroupAsync("test-sub", "test-rg", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedHostpools);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub", "--resource-group", "test-rg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListHostpoolsByResourceGroupAsync("test-sub", "test-rg", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().ListHostpoolsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsSubscriptionService_WhenNoResourceGroup()
    {
        // Arrange
        var expectedHostpools = new List<HostPool>
        {
            new() { Name = "hostpool1" },
            new() { Name = "hostpool2" }
        }.AsReadOnly();
        Service.ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedHostpools);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListHostpoolsAsync("test-sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResult_WhenNoHostpoolsInResourceGroup()
    {
        // Arrange
        Service.ListHostpoolsByResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub", "--resource-group", "test-rg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
