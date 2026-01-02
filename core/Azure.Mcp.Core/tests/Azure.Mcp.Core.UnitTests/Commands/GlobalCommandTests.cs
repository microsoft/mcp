// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Commands;

/// <summary>
/// Tests for GlobalCommand option binding, ensuring global options like authorityHost, tenant, and retry policies
/// are correctly bound from command-line arguments.
/// </summary>
public class GlobalCommandTests
{
    [Fact]
    public void BindOptions_WithAuthorityHost_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var authorityHostUri = new Uri("https://login.microsoftonline.us");
        var parseResult = command.Parse($"--authority-host {authorityHostUri}");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.AuthorityHost);
        Assert.Equal(authorityHostUri, options.AuthorityHost);
    }

    [Fact]
    public void BindOptions_WithoutAuthorityHost_ReturnsNull()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.AuthorityHost);
    }

    [Fact]
    public void BindOptions_WithTenant_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var tenantId = "12345678-1234-1234-1234-123456789012";
        var parseResult = command.Parse($"--tenant {tenantId}");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(tenantId, options.Tenant);
    }

    [Fact]
    public void BindOptions_WithAuthorityHostAndTenant_BindsBothCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var authorityHostUri = new Uri("https://login.microsoftonline.us");
        var tenantId = "12345678-1234-1234-1234-123456789012";
        var parseResult = command.Parse($"--authority-host {authorityHostUri} --tenant {tenantId}");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(authorityHostUri, options.AuthorityHost);
        Assert.Equal(tenantId, options.Tenant);
    }

    [Fact]
    public void BindOptions_WithAuthMethod_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--auth-method Credential");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(AuthMethod.Credential, options.AuthMethod);
    }

    [Theory]
    [InlineData("--max-retries 5", 5)]
    [InlineData("--max-retries 10", 10)]
    [InlineData("--max-retries 0", 0)]
    public void BindOptions_WithRetryPolicyMaxRetries_BindsCorrectly(string args, int expectedMaxRetries)
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse(args);

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasMaxRetries);
        Assert.Equal(expectedMaxRetries, options.RetryPolicy.MaxRetries);
    }

    [Fact]
    public void BindOptions_WithRetryPolicyDelay_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--delay 2.5");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasDelaySeconds);
        Assert.Equal(2.5, options.RetryPolicy.DelaySeconds);
    }

    [Fact]
    public void BindOptions_WithRetryPolicyMaxDelay_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--max-delay 30");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasMaxDelaySeconds);
        Assert.Equal(30, options.RetryPolicy.MaxDelaySeconds);
    }

    [Fact]
    public void BindOptions_WithRetryPolicyMode_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--mode Exponential");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasMode);
        Assert.Equal(Azure.Core.RetryMode.Exponential, options.RetryPolicy.Mode);
    }

    [Fact]
    public void BindOptions_WithRetryPolicyNetworkTimeout_BindsCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--network-timeout 60");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasNetworkTimeoutSeconds);
        Assert.Equal(60, options.RetryPolicy.NetworkTimeoutSeconds);
    }

    [Fact]
    public void BindOptions_WithMultipleRetryPolicyOptions_BindsAllCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("--max-retries 3 --delay 1.5 --max-delay 20 --mode Fixed --network-timeout 45");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.RetryPolicy);
        Assert.True(options.RetryPolicy.HasMaxRetries);
        Assert.Equal(3, options.RetryPolicy.MaxRetries);
        Assert.True(options.RetryPolicy.HasDelaySeconds);
        Assert.Equal(1.5, options.RetryPolicy.DelaySeconds);
        Assert.True(options.RetryPolicy.HasMaxDelaySeconds);
        Assert.Equal(20, options.RetryPolicy.MaxDelaySeconds);
        Assert.True(options.RetryPolicy.HasMode);
        Assert.Equal(Azure.Core.RetryMode.Fixed, options.RetryPolicy.Mode);
        Assert.True(options.RetryPolicy.HasNetworkTimeoutSeconds);
        Assert.Equal(45, options.RetryPolicy.NetworkTimeoutSeconds);
    }

    [Fact]
    public void BindOptions_WithNoRetryPolicyOptions_ReturnsNullRetryPolicy()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var parseResult = command.Parse("");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.RetryPolicy);
    }

    [Fact]
    public void BindOptions_WithAllGlobalOptions_BindsAllCorrectly()
    {
        // Arrange
        var testCommand = new TestGlobalCommand();
        var command = testCommand.GetCommand();
        var authorityHostUri = new Uri("https://login.microsoftonline.us");
        var tenantId = "12345678-1234-1234-1234-123456789012";
        var parseResult = command.Parse(
            $"--authority-host {authorityHostUri} " +
            $"--tenant {tenantId} " +
            $"--auth-method Credential " +
            $"--max-retries 5 " +
            $"--delay 2.0 " +
            $"--max-delay 25 " +
            $"--mode Exponential " +
            $"--network-timeout 90");

        // Act
        var options = testCommand.BindOptionsPublic(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(authorityHostUri, options.AuthorityHost);
        Assert.Equal(tenantId, options.Tenant);
        Assert.Equal(AuthMethod.Credential, options.AuthMethod);
        Assert.NotNull(options.RetryPolicy);
        Assert.Equal(5, options.RetryPolicy.MaxRetries);
        Assert.Equal(2.0, options.RetryPolicy.DelaySeconds);
        Assert.Equal(25, options.RetryPolicy.MaxDelaySeconds);
        Assert.Equal(Azure.Core.RetryMode.Exponential, options.RetryPolicy.Mode);
        Assert.Equal(90, options.RetryPolicy.NetworkTimeoutSeconds);
    }

    /// <summary>
    /// Test implementation of GlobalCommand that exposes BindOptions for testing
    /// </summary>
    private sealed class TestGlobalCommand : GlobalCommand<GlobalOptions>
    {
        public override string Id => "test-global-command-id";
        public override string Name => "test";
        public override string Description => "Test global command for option binding tests";
        public override string Title => "Test Global Command";
        public override ToolMetadata Metadata => new()
        {
            OpenWorld = false,
            Destructive = false,
            Idempotent = true,
            ReadOnly = true,
            Secret = false,
            LocalRequired = false
        };

        public GlobalOptions BindOptionsPublic(ParseResult parseResult)
        {
            return BindOptions(parseResult);
        }

        public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
        {
            // Not needed for option binding tests
            throw new NotImplementedException();
        }
    }
}
