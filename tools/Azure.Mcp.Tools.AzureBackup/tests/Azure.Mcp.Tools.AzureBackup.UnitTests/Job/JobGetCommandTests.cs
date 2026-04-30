// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Job;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Job;

public class JobGetCommandTests : CommandUnitTestsBase<JobGetCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsJobs_WhenNoJobSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var expectedJobs = new List<BackupJobInfo>
        {
            new("id1", "job1", "rsv", "Backup", "Completed", DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), "AzureIaasVM", "vm1"),
            new("id2", "job2", "rsv", "Restore", "InProgress", DateTimeOffset.UtcNow.AddMinutes(-30), null, "SQLDataBase", "sql1")
        };

        Service.ListJobsAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.JobGetCommandResult);

        Assert.Equal(2, result.Jobs.Count);
        Assert.Equal("job1", result.Jobs[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleJob_WhenJobSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vault = "myVault";
        var resourceGroup = "myRg";
        var jobId = "job1";
        var expectedJob = new BackupJobInfo("id1", jobId, "rsv", "Backup", "Completed", DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), "AzureIaasVM", "vm1");

        Service.GetJobAsync(
            Arg.Is(vault),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Is(jobId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--vault", vault,
            "--resource-group", resourceGroup,
            "--job", jobId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.JobGetCommandResult);

        Assert.Single(result.Jobs);
        Assert.Equal(jobId, result.Jobs[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoJobsExist()
    {
        // Arrange
        Service.ListJobsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.JobGetCommandResult);

        Assert.Empty(result.Jobs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ListJobsAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.GetJobAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("nonexistent"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Job not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--job", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub --vault v --resource-group rg --job j1", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListJobsAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns([]);

            Service.GetJobAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("j1"), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new BackupJobInfo("id", "j1", "rsv", "Backup", "Completed", null, null, "VM", "vm1"));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = CommandDefinition.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--job");
    }
}
