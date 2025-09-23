// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Models;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.SessionHost;

[Trait("Area", "VirtualDesktop")]
public class SessionHostUserSessionListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVirtualDesktopService _virtualDesktopService;
    private readonly ILogger<SessionHostUserSessionListCommand> _logger;
    private readonly SessionHostUserSessionListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SessionHostUserSessionListCommandTests()
    {
        _virtualDesktopService = Substitute.For<IVirtualDesktopService>();
        _logger = Substitute.For<ILogger<SessionHostUserSessionListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_virtualDesktopService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("usersession-list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("List all user sessions on a specific session host", command.Description);
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
            _virtualDesktopService.ListUserSessionsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(userSessions.AsReadOnly());

            _virtualDesktopService.ListUserSessionsByResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(userSessions.AsReadOnly());

            _virtualDesktopService.ListUserSessionsByResourceGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(userSessions.AsReadOnly());
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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

        _virtualDesktopService.ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>());
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

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            "test-sub",
            resourceId,
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        var parseResult = _commandDefinition.Parse($"--subscription test-sub --hostpool-resource-id {resourceId} --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListUserSessionsByResourceIdAsync(
            "test-sub",
            resourceId,
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>());

        await _virtualDesktopService.DidNotReceive().ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
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

        _virtualDesktopService.ListUserSessionsByResourceGroupAsync(
            "test-sub",
            "test-rg",
            "test-hostpool",
            "test-sessionhost",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost --resource-group test-rg");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListUserSessionsByResourceGroupAsync(
            "test-sub",
            "test-rg",
            "test-hostpool",
            "test-sessionhost",
            null,
            Arg.Any<RetryPolicyOptions?>());

        await _virtualDesktopService.DidNotReceive().ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());

        await _virtualDesktopService.DidNotReceive().ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResultsWhenNoUserSessions()
    {
        // Arrange
        var userSessions = new List<UserSession>();

        _virtualDesktopService.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _virtualDesktopService.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("Test error"));

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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
        _virtualDesktopService.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(exception);

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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
        _virtualDesktopService.ListUserSessionsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(exception);

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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

        _virtualDesktopService.ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        _virtualDesktopService.ListUserSessionsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(userSessions.AsReadOnly());

        var parseResult = _commandDefinition.Parse("--subscription test-sub --hostpool test-hostpool --sessionhost test-sessionhost --tenant test-tenant");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListUserSessionsAsync(
            "test-sub",
            "test-hostpool",
            "test-sessionhost",
            "test-tenant",
            Arg.Any<RetryPolicyOptions?>());
    }
}
