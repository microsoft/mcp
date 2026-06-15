// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.Tests.FileSystem.ExpansionJob;

public class ExpansionJobDeleteCommandTests : CommandUnitTestsBase<ExpansionJobDeleteCommand, IManagedLustreService>
{
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";
    private readonly string _jobName = "expansion-001";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--expansion-job-name", _jobName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).DeleteExpansionJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(_jobName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub123 --filesystem-name fs1 --expansion-job-name j1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1 --expansion-job-name j1", false)] // missing filesystem-name
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name fs1", false)] // missing expansion-job-name
    public async Task ExecuteAsync_ValidationErrors_Return400(string argLine, bool shouldSucceed)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        var expectedStatus = shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        Assert.Equal(expectedStatus, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_RequestFailed_UsesStatusCode()
    {
        // Arrange
        Service.DeleteExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--expansion-job-name", _jobName);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        Service.DeleteExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--expansion-job-name", _jobName);

        // Assert
        Assert.True((int)response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--expansion-job-name", _jobName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteExpansionJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(_jobName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
