// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetProvisioningRecommendationCommand.
/// Tests command for retrieving provisioning recommendations based on workload.
/// </summary>
public class FileShareGetProvisioningRecommendationCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSharesService _fileSharesService;
    private readonly ILogger<FileShareGetProvisioningRecommendationCommand> _logger;
    private readonly FileShareGetProvisioningRecommendationCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FileShareGetProvisioningRecommendationCommandTests()
    {
        _fileSharesService = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetProvisioningRecommendationCommand>>();

        var collection = new ServiceCollection().AddSingleton(_fileSharesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    /// <summary>
    /// Tests that the command initializes correctly with expected properties.
    /// </summary>
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();

        Assert.Equal("getprovisioningrecommendation", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("recommendation", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests that the command has correct metadata properties (read-only).
    /// </summary>
    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        var metadata = _command.Metadata;

        Assert.True(metadata.ReadOnly, "GetProvisioningRecommendation should be read-only");
        Assert.False(metadata.Destructive, "GetProvisioningRecommendation should not be destructive");
        Assert.True(metadata.Idempotent, "GetProvisioningRecommendation should be idempotent");
    }

    /// <summary>
    /// Tests retrieving provisioning recommendations for general workload.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithGeneralWorkload_ReturnsRecommendation()
    {
        // Arrange
        var subscription = "sub123";
        var workloadType = "general";
        var recommendation = new FileShareProvisioningRecommendationSchema
        {
            RecommendedTier = "Standard",
            RecommendedQuota = 102400,
            Rationale = "Suitable for general-purpose file sharing"
        };

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Is(subscription),
            Arg.Is(workloadType),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(recommendation));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--workload", workloadType]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    /// <summary>
    /// Tests retrieving recommendations for database workload.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithDatabaseWorkload_ReturnsRecommendation()
    {
        // Arrange
        var subscription = "sub123";
        var workloadType = "database";
        var recommendation = new FileShareProvisioningRecommendationSchema
        {
            RecommendedTier = "Premium",
            RecommendedQuota = 1048576,
            Rationale = "Database workloads require Premium tier for performance"
        };

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Is(subscription),
            Arg.Is(workloadType),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(recommendation));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--workload", workloadType]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    /// <summary>
    /// Tests recommendation retrieval with tenant ID.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithTenantId_SucceedsCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var workloadType = "analytics";
        var tenantId = "tenant123";
        var recommendation = new FileShareProvisioningRecommendationSchema
        {
            RecommendedTier = "Standard",
            RecommendedQuota = 512000
        };

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Is(subscription),
            Arg.Is(workloadType),
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(recommendation));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--workload", workloadType, "--tenant", tenantId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    /// <summary>
    /// Tests that service exceptions are properly handled.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var subscription = "sub123";
        var workloadType = "invalid";
        var expectedError = "Invalid workload type";

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Is(subscription),
            Arg.Is(workloadType),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--workload", workloadType]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Status == HttpStatusCode.BadRequest || response.Status == HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Tests command option validation for required parameters.
    /// </summary>
    [Theory]
    [InlineData("--subscription sub123 --workload general", true)]
    [InlineData("--subscription sub123 --workload database --tenant tenant123", true)]
    [InlineData("--subscription sub123", false)] // Missing workload
    [InlineData("--workload general", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var recommendation = new FileShareProvisioningRecommendationSchema { RecommendedTier = "Standard" };

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(recommendation));

        // Act & Assert
        if (shouldSucceed)
        {
            var parsedArgs = _commandDefinition.Parse(args);
            var response = await _command.ExecuteAsync(_context, parsedArgs, TestContext.Current.CancellationToken);
            Assert.NotNull(response);
        }
        else
        {
            var exception = Assert.Throws<Exception>(() => _commandDefinition.Parse(args));
            Assert.NotNull(exception);
        }
    }

    /// <summary>
    /// Tests multiple workload profiles return recommendations.
    /// </summary>
    [Theory]
    [InlineData("general")]
    [InlineData("database")]
    [InlineData("analytics")]
    [InlineData("backup")]
    public async Task ExecuteAsync_WithVariousWorkloads_ReturnsRecommendations(string workload)
    {
        // Arrange
        var subscription = "sub123";
        var recommendation = new FileShareProvisioningRecommendationSchema { RecommendedTier = "Standard" };

        _fileSharesService.GetProvisioningRecommendationAsync(
            Arg.Is(subscription),
            Arg.Is(workload),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(recommendation));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--workload", workload]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
        Assert.True(command.ToolMetadata.Idempotent);
    }
}
