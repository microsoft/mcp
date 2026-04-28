// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.ImportJob;

public class ImportJobGetCommandTests : CommandUnitTestsBase<ImportJobGetCommand, IManagedLustreService>
{
    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string JobName = "import-job-01";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01 --job-name import-job-01", true)]
    [InlineData("--resource-group rg1 --filesystem-name amlfs-01", false)] // Missing subscription
    [InlineData("--subscription sub123 --filesystem-name amlfs-01", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // Missing filesystem
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var importJob = new Models.ImportJob { Name = JobName, Id = "id1", Properties = new() { ProvisioningState = "Succeeded" } };

            Service.GetImportJobAsync(
                Arg.Is(Sub), Arg.Is(Rg), Arg.Is(Name), Arg.Is(JobName),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(importJob);
        }

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobGetResult);
            Assert.Equal(JobName, result.Job.Name);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithJobName_CallsGetSingleJob()
    {
        // Arrange
        var importJob = new Models.ImportJob
        {
            Name = JobName,
            Id = "id1",
            Properties = new Models.ImportJobProperties
            {
                ProvisioningState = "Succeeded",
                ConflictResolutionMode = "Fail",
                MaximumErrors = 10
            }
        };

        Service.GetImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJob);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {JobName}";

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.DidNotReceive().ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutJobName_CallsListJobs()
    {
        // Arrange
        var importJobs = new List<Models.ImportJob>
        {
            new() { Name = "job1", Id = "id1", Properties = new Models.ImportJobProperties { ProvisioningState = "Running" } },
            new() { Name = "job2", Id = "id2", Properties = new Models.ImportJobProperties { ProvisioningState = "Succeeded" } }
        };

        Service.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJobs);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name}";

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name}";

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var importJobs = new List<Models.ImportJob>
        {
            new() { Name = JobName, Id = "id1", Properties = new() { ProvisioningState = "Succeeded" } }
        };

        Service.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJobs);

        // Act
        var response = await ExecuteCommandAsync("--subscription", Sub, "--resource-group", Rg, "--filesystem-name", Name);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobListResult);
        Assert.Single(result.Jobs);
        Assert.Equal(JobName, result.Jobs[0].Name);
    }
}
