// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.SessionHost;

public class SessionHostUserSessionListCommandTests : CommandUnitTestsBase<SessionHostUserSessionListCommand, IVirtualDesktopService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("user-list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Theory]
    [InlineData("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost", true)]
    [InlineData("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost --resource-group test-rg", true)]
    [InlineData("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost --resource-group test-rg --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --hostpool-resource-id /subscriptions/test-sub/resourceGroups/rg/providers/Microsoft.DesktopVirtualization/hostPools/test-hostpool --sessionhost test-sessionhost", true)]
    [InlineData("--subscription test-sub --hostpool-resource-id /subscriptions/test-sub/resourceGroups/rg/providers/Microsoft.DesktopVirtualization/hostPools/test-hostpool --sessionhost test-sessionhost --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --hostpool test-hostpool", false)] // Missing sessionhost
    [InlineData("--subscription test-sub --sessionhost test-sessionhost", false)] // Missing both hostpool parameters
    [InlineData("--subscription test-sub --hostpool test-hostpool --hostpool-resource-id /subscriptions/test-sub/resourceGroups/rg/providers/Microsoft.DesktopVirtualization/hostPools/test-hostpool --sessionhost test-sessionhost", false)] // Both hostpool parameters
    [InlineData("--hostpool test-hostpool --sessionhost test-sessionhost", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var userSessions = new List<UserSession>
            {
                new() {
                    Name = "session1",
                    UserPrincipalName = "user1@contoso.com",
                    HostPoolName = "test-hostpool",
                    SessionHostName = "test-sessionhost",
                    SessionState = "Active",
                    ApplicationType = "RemoteApp",
                    CreateTime = DateTime.UtcNow
                }
            };
            Service.ListUserSessionsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(userSessions.AsReadOnly());

            Service.ListUserSessionsByResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(userSessions.AsReadOnly());

            Service.ListUserSessionsByResourceGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(userSessions.AsReadOnly());
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
            Assert.True(response.Message?.Contains("required", StringComparison.CurrentCultureIgnoreCase) == true ||
                       response.Message?.Contains("hostpool") == true ||
                       response.Message?.Contains("hostpool-resource-id") == true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUserSessionsSuccessfully()
    {
        // Arrange
        var userSessions = new List<UserSession>
        {
            new() {
                Name = "session1",
                UserPrincipalName = "user1@contoso.com",
                HostPoolName = "test-hostpool",
                SessionHostName = "test-sessionhost",
                SessionState = "Active",
                ApplicationType = "RemoteApp",
                CreateTime = DateTime.UtcNow
            },
            new() {
                Name = "session2",
                UserPrincipalName = "user2@contoso.com",
                HostPoolName = "test-hostpool",
                SessionHostName = "test-sessionhost",
                SessionState = "Disconnected",
                ApplicationType = "Published",
                CreateTime = DateTime.UtcNow.AddMinutes(-30)
            }
        };

        Service.ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceId_CallsServiceCorrectly()
    {
        // Arrange
        var userSessions = new List<UserSession>
        {
            new() {
                Name = "session1",
                UserPrincipalName = "user1@contoso.com",
                HostPoolName = "test-hostpool",
                SessionHostName = "test-sessionhost",
                SessionState = "Active",
                ApplicationType = "RemoteApp",
                CreateTime = DateTime.UtcNow
            }
        };
        var resourceId = "/subscriptions/test-sub/resourceGroups/rg/providers/Microsoft.DesktopVirtualization/hostPools/test-hostpool";

        Service.ListUserSessionsByResourceIdAsync(
            "test-sub",
            resourceId,
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool-resource-id", resourceId,
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListUserSessionsByResourceIdAsync(
            "test-sub",
            resourceId,
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_CallsServiceCorrectly()
    {
        // Arrange
        var userSessions = new List<UserSession>
        {
            new() {
                Name = "session1",
                UserPrincipalName = "user1@contoso.com",
                HostPoolName = "test-hostpool",
                SessionHostName = "test-sessionhost",
                SessionState = "Active",
                ApplicationType = "RemoteApp",
                CreateTime = DateTime.UtcNow
            }
        };

        Service.ListUserSessionsByResourceGroupAsync(
            "test-sub",
            "test-rg",
            "test-hostpool",
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost",
            "--resource-group", "test-rg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListUserSessionsByResourceGroupAsync(
            "test-sub",
            "test-rg",
            "test-hostpool",
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResultsWhenNoUserSessions()
    {
        // Arrange
        var userSessions = new List<UserSession>();

        Service.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        Service.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        Service.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange
        var exception = new RequestFailedException((int)HttpStatusCode.NotFound, "Session host not found");
        Service.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        Service.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Session host or hostpool not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange
        var exception = new RequestFailedException((int)HttpStatusCode.Forbidden, "Access denied");
        Service.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        Service.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Access denied", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenantParameter()
    {
        // Arrange
        var userSessions = new List<UserSession>
        {
            new() {
                Name = "session1",
                UserPrincipalName = "user1@contoso.com",
                HostPoolName = "test-hostpool",
                SessionHostName = "test-sessionhost",
                SessionState = "Active",
                ApplicationType = "RemoteApp",
                CreateTime = DateTime.UtcNow
            }
        };

        Service.ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        Service.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(userSessions.AsReadOnly());

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--hostpool", "test-hostpool",
            "--sessionhost", "test-sessionhost",
            "--tenant", "test-tenant");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await Service.Received(1).ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
