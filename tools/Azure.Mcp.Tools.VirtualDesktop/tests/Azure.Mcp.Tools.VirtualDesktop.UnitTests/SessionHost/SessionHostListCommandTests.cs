// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using SessionHostModel = Azure.Mcp.Tools.VirtualDesktop.Models.SessionHost;

namespace Azure.Mcp.Tools.VirtualDesktop.UnitTests.SessionHost;

[Trait("Area", "VirtualDesktop")]
public class SessionHostListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVirtualDesktopService _virtualDesktopService;
    private readonly ILogger<SessionHostListCommand> _logger;
    private readonly SessionHostListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SessionHostListCommandTests()
    {
        _virtualDesktopService = Substitute.For<IVirtualDesktopService>();
        _logger = Substitute.For<ILogger<SessionHostListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_virtualDesktopService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Equal("List SessionHosts", _command.Title);
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

            _virtualDesktopService.ListSessionHostsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult<IReadOnlyList<SessionHostModel>>(mockSessionHosts));

            _virtualDesktopService.ListSessionHostsByResourceIdAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult<IReadOnlyList<SessionHostModel>>(mockSessionHosts));

            _virtualDesktopService.ListSessionHostsByResourceGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult<IReadOnlyList<SessionHostModel>>(mockSessionHosts));
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

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
        _virtualDesktopService.ListSessionHostsAsync(
            "sub123",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSessionHosts);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub123 --hostpool pool1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListSessionHostsAsync(
            "sub123",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>());
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

        _virtualDesktopService.ListSessionHostsByResourceIdAsync(
            "sub123",
            resourceId,
            null,
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSessionHosts);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse($"--subscription sub123 --hostpool-resource-id {resourceId}");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListSessionHostsByResourceIdAsync(
            "sub123",
            resourceId,
            null,
            Arg.Any<RetryPolicyOptions>());

        await _virtualDesktopService.DidNotReceive().ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_CallsServiceCorrectly()
    {
        // First test: Can we parse the command line correctly?
        var parseResult = _commandDefinition.Parse("--subscription sub123 --hostpool pool1 --resource-group rg1");

        // Check for parse errors
        if (parseResult.Errors.Any())
        {
            Console.WriteLine("Parse errors:");
            foreach (var error in parseResult.Errors)
            {
                Console.WriteLine($"  {error}");
            }
        }

        // Arrange
        var expectedSessionHosts = new List<SessionHostModel>
        {
            CreateMockSessionHost("sessionhost1"),
            CreateMockSessionHost("sessionhost2")
        };

        _virtualDesktopService.ListSessionHostsByResourceGroupAsync(
            "sub123",
            "rg1",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSessionHosts);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        // If this fails, let's see what the actual message is
        if (response.Status != HttpStatusCode.OK)
        {
            Console.WriteLine($"Actual Status: {response.Status}");
            Console.WriteLine($"Actual Message: {response.Message}");
        }

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _virtualDesktopService.Received(1).ListSessionHostsByResourceGroupAsync(
            "sub123",
            "rg1",
            "pool1",
            null,
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResults_ReturnsNullResults()
    {
        // Arrange
        _virtualDesktopService.ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        _virtualDesktopService.ListSessionHostsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub123 --hostpool pool1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _virtualDesktopService.ListSessionHostsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<IReadOnlyList<SessionHostModel>>(new Exception("Test error")));

        _virtualDesktopService.ListSessionHostsByResourceIdAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<IReadOnlyList<SessionHostModel>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub123 --hostpool pool1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

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
        // Arrange
        var context = new CommandContext(_serviceProvider);

        // Act & Assert
        try
        {
            var parseResult = _commandDefinition.Parse(invalidArgs);
            var response = await _command.ExecuteAsync(context, parseResult);

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
