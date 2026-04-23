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

public class ImportJobDeleteCommandTests : CommandUnitTestsBase<ImportJobDeleteCommand, IManagedLustreService>
{
    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string JobName = "import-job-01";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01 --job-name import-job-01", true)]
    [InlineData("--resource-group rg1 --filesystem-name amlfs-01 --job-name import-job-01", false)] // Missing subscription
    [InlineData("--subscription sub123 --filesystem-name amlfs-01 --job-name import-job-01", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1 --job-name import-job-01", false)] // Missing filesystem
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01", false)] // Missing job-name
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteImportJobAsync(
                Arg.Is(Sub), Arg.Is(Rg), Arg.Is(Name), Arg.Is(JobName), Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
        }

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobDeleteResult);
            Assert.Equal(JobName, result.JobName);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        Service.DeleteImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {JobName}";

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.DeleteImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {JobName}";

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
        Service.DeleteImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", Sub,
            "--resource-group", Rg,
            "--filesystem-name", Name,
            "--job-name", JobName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ManagedLustreJsonContext.Default.ImportJobDeleteResult);
        Assert.Equal(JobName, result.JobName);
    }
}
