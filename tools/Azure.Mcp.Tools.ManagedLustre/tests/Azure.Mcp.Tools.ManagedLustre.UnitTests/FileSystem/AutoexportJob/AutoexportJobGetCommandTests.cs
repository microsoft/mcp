// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Text.Json;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.AutoexportJob;

public class AutoexportJobGetCommandTests
{
    private readonly IManagedLustreService _service;
    private readonly AutoexportJobGetCommand _command;
    private readonly CommandContext _context;

    public AutoexportJobGetCommandTests()
    {
        _service = Substitute.For<IManagedLustreService>();
        _command = new AutoexportJobGetCommand(_service);
        _context = CommandContextHelper.CreateTestContext();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = new AutoexportJobGetCommand(_service);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsJobDetails()
    {
        // Arrange
        var options = new AutoexportJobGetOptions
        {
            Subscription = "test-subscription",
            ResourceGroup = "test-rg",
            FileSystemName = "test-fs",
            JobName = "test-job"
        };

        var expectedJob = new Models.AutoexportJob
        {
            Name = "test-job",
            Id = "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.StorageCache/amlFilesystems/test-fs/autoExportJobs/test-job",
            ProvisioningState = "Succeeded"
        };

        _service.GetAutoexportJobAsync(
            options.Subscription!,
            options.ResourceGroup!,
            options.FileSystemName!,
            options.JobName!,
            options.Tenant,
            options.RetryPolicy,
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        // Act
        await _command.ExecuteAsync(options, _context);

        // Assert
        Assert.True(_context.Response.Results.IsSuccess);
        var result = JsonSerializer.Deserialize<AutoexportJobGetCommand.AutoexportJobGetResult>(
            _context.Response.Results.Data!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(result);
        Assert.Equal("test-job", result.Job.Name);
        Assert.Equal("Succeeded", result.Job.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_JobNotFound_ReturnsError()
    {
        // Arrange
        var options = new AutoexportJobGetOptions
        {
            Subscription = "test-subscription",
            ResourceGroup = "test-rg",
            FileSystemName = "test-fs",
            JobName = "nonexistent-job"
        };

        var exception = new Exception("Autoexport job 'nonexistent-job' not found for filesystem 'test-fs' in resource group 'test-rg'.");
        _service.GetAutoexportJobAsync(
            options.Subscription!,
            options.ResourceGroup!,
            options.FileSystemName!,
            options.JobName!,
            options.Tenant,
            options.RetryPolicy,
            Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        await _command.ExecuteAsync(options, _context);

        // Assert
        Assert.False(_context.Response.Results.IsSuccess);
        Assert.Contains("not found", _context.Response.Results.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_HandlesGracefully()
    {
        // Arrange
        var options = new AutoexportJobGetOptions
        {
            Subscription = "test-subscription",
            ResourceGroup = "test-rg",
            FileSystemName = "test-fs",
            JobName = "test-job"
        };

        _service.GetAutoexportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Throws(new Exception("Service error"));

        // Act
        await _command.ExecuteAsync(options, _context);

        // Assert
        Assert.False(_context.Response.Results.IsSuccess);
        Assert.NotNull(_context.Response.Results.Error);
    }

    [Theory]
    [InlineData(null, "test-rg", "test-fs", "test-job")]
    [InlineData("test-sub", null, "test-fs", "test-job")]
    [InlineData("test-sub", "test-rg", null, "test-job")]
    [InlineData("test-sub", "test-rg", "test-fs", null)]
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsError(
        string? subscription,
        string? resourceGroup,
        string? fileSystemName,
        string? jobName)
    {
        // Arrange
        var options = new AutoexportJobGetOptions
        {
            Subscription = subscription,
            ResourceGroup = resourceGroup,
            FileSystemName = fileSystemName,
            JobName = jobName
        };

        // Act
        await _command.ExecuteAsync(options, _context);

        // Assert
        Assert.False(_context.Response.Results.IsSuccess);
    }
}
