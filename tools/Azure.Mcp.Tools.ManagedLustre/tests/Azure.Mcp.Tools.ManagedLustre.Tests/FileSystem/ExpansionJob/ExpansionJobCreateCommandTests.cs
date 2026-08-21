// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.Tests.FileSystem.ExpansionJob;

public class ExpansionJobCreateCommandTests : SubscriptionCommandUnitTestsBase<ExpansionJobCreateCommand, IManagedLustreService>
{
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(CommandDefinition.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange
        Service.CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<float>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("expansion-20250107120000");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", "64");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).CreateExpansionJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(64f),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub123 --filesystem-name fs1 --new-size 64", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1 --new-size 64", false)] // missing filesystem-name
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name fs1", false)] // missing new-size
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

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("NaN")]
    public async Task ExecuteAsync_InvalidNewSize_Returns400(string newSize)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", newSize);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("finite value greater than zero", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().CreateExpansionJobAsync(
            default!,
            default!,
            default!,
            default,
            default,
            default,
            default,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ValidateOptions_InfiniteNewSize_AddsValidationError()
    {
        foreach (var newSize in new[] { float.PositiveInfinity, float.NegativeInfinity })
        {
            var options = new ExpansionJobCreateOptions
            {
                Subscription = _subscription,
                ResourceGroup = _resourceGroup,
                FilesystemName = _fileSystemName,
                NewSize = newSize
            };
            var validationResult = new ValidationResult();

            Command.ValidateOptions(options, validationResult);

            Assert.Contains(validationResult.Errors, error =>
                error.Contains("finite value greater than zero", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_RequestFailed_UsesStatusCode()
    {
        // Arrange
        Service.CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<float>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", "64");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        Service.CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<float>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", "64");

        // Assert
        Assert.True((int)response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        Service.CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<float>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("expansion-20250107120000");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", "128");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateExpansionJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(128f),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithJobName()
    {
        // Arrange
        var jobName = "my-expansion-job";
        Service.CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<float>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(jobName);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--new-size", "64",
            "--expansion-job-name", jobName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateExpansionJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is(64f),
            Arg.Is(jobName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
