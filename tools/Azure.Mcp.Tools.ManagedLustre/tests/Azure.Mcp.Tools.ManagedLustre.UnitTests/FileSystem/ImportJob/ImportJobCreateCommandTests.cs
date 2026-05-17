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

public class ImportJobCreateCommandTests : CommandUnitTestsBase<ImportJobCreateCommand, IManagedLustreService>
{
    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string JobName = "import-job-01";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01", true)]
    [InlineData("--resource-group rg1 --filesystem-name amlfs-01", false)] // Missing subscription
    [InlineData("--subscription sub123 --filesystem-name amlfs-01", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // Missing filesystem
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.CreateImportJobAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string[]?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(JobName);
        }

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobCreateResult);
            Assert.Equal(JobName, result.JobName);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_CallsService()
    {
        // Arrange
        const string jobName = "custom-job";
        const string conflictMode = "Fail";
        const string prefixes = "folder1/,folder2/";
        const long maxErrors = 10;

        Service.CreateImportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(jobName);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {jobName} --conflict-resolution-mode {conflictMode} --import-prefixes {prefixes} --maximum-errors {maxErrors}";

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateImportJobAsync(
            Sub, Rg, Name, jobName, conflictMode,
            Arg.Any<string[]>(),
            Arg.Any<long?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.CreateImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<long?>(), Arg.Any<string?>(),
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
        Service.CreateImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<long?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(JobName);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--filesystem-name", Name);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobCreateResult);
        Assert.Equal(JobName, result.JobName);
    }
}
