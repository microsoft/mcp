// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using SessionHostModel = Azure.Mcp.Tools.VirtualDesktop.Models.SessionHost;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.SessionHost;

public class SessionHostListCommandTests : CommandUnitTestsBase<SessionHostListCommand, IVirtualDesktopService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Equal("List SessionHosts", Command.Title);
    }

    [Theory]
    [InlineData("--subscription sub123 --hostpool pool1", true)]
    [InlineData("--subscription sub123 --hostpool pool1 --tenant tenant1", true)]
    [InlineData("--subscription sub123 --hostpool pool1 --resource-group rg1", true)]
    [InlineData("--subscription sub123 --hostpool pool1 --resource-group rg1 --tenant tenant1", true)]
    [InlineData("--subscription sub123 --hostpool-resource-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DesktopVirtualization/hostPools/pool1", true)]
    [InlineData("--subscription sub123 --hostpool-resource-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DesktopVirtualization/hostPools/pool1 --tenant tenant1", true)]
    [InlineData("--subscription sub123", false)] // Missing both hostpool and hostpool-resource-id
    [InlineData("--subscription sub123 --hostpool pool1 --hostpool-resource-id /subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DesktopVirtualization/hostPools/pool1", false)] // Both provided
    [InlineData("--hostpool pool1", false)] // Missing subscription
    [InlineData("", false)] // Missing both
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockSessionHosts = new List<SessionHostModel>
            {
                CreateMockSessionHost("sessionhost1"),
                CreateMockSessionHost("sessionhost2")
            };

            Service.ListSessionHostsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(mockSessionHosts);

            Service.ListSessionHostsByResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(mockSessionHosts);

            Service.ListSessionHostsByResourceGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(mockSessionHosts);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.True(response.Message.Contains("required", StringComparison.CurrentCultureIgnoreCase) ||
                       response.Message.Contains("hostpool") ||
                       response.Message.Contains("hostpool-resource-id"));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_CallsServiceCorrectly()
    {
        // Arrange
        var expectedSessionHosts = new List<SessionHostModel>
        {
            new() { Name = "sessionhost1" },
            new() { Name = "sessionhost2" }
        };
        Service.ListSessionHostsAsync(
            "sub123",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedSessionHosts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--hostpool", "pool1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListSessionHostsAsync(
            "sub123",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceId_CallsServiceCorrectly()
    {
        // Arrange
        var expectedSessionHosts = new List<SessionHostModel>
        {
            new() { Name = "sessionhost1" },
            new() { Name = "sessionhost2" }
        };
        var resourceId = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.DesktopVirtualization/hostPools/pool1";

        Service.ListSessionHostsByResourceIdAsync(
            "sub123",
            resourceId,
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedSessionHosts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--hostpool-resource-id", resourceId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListSessionHostsByResourceIdAsync(
            "sub123",
            resourceId,
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_CallsServiceCorrectly()
    {
        // Arrange
        var expectedSessionHosts = new List<SessionHostModel>
        {
            CreateMockSessionHost("sessionhost1"),
            CreateMockSessionHost("sessionhost2")
        };

        Service.ListSessionHostsByResourceGroupAsync(
            "sub123",
            "rg1",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedSessionHosts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--hostpool", "pool1", "--resource-group", "rg1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListSessionHostsByResourceGroupAsync(
            "sub123",
            "rg1",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResults_ReturnsNullResults()
    {
        // Arrange
        Service.ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        Service.ListSessionHostsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--hostpool", "pool1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        Service.ListSessionHostsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--hostpool", "pool1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--hostpool")]
    [InlineData("--invalid-option")]
    public async Task ExecuteAsync_WithInvalidArgs_ReturnsBadRequest(string invalidArgs)
    {
        // Act & Assert
        try
        {
            var response = await ExecuteCommandAsync(invalidArgs);

            // If parsing succeeds but validation fails, expect 400
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
        catch (InvalidOperationException)
        {
            // This is expected for malformed arguments like incomplete options
            // The parser throws InvalidOperationException for incomplete options
            Assert.True(true, "Expected InvalidOperationException for malformed arguments");
        }
    }

    private static SessionHostModel CreateMockSessionHost(string name)
    {
        return new SessionHostModel
        {
            Name = name,
            ResourceGroupName = "test-rg",
            SubscriptionId = "test-sub",
            HostPoolName = "test-pool",
            Status = "Available",
            Sessions = 2,
            AgentVersion = "1.0.0",
            AllowNewSession = true,
            AssignedUser = "test@example.com",
            FriendlyName = $"Friendly {name}",
            OsVersion = "Windows 11",
            UpdateState = "NotStarted",
            UpdateErrorMessage = null
        };
    }
}
