// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.AutoimportJob;

public class AutoimportJobCreateCommandTests : CommandUnitTestsBase<AutoimportJobCreateCommand, IManagedLustreService>
{
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(Command.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-20250107120000");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).CreateAutoimportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Fail")]
    [InlineData("Skip")]
    [InlineData("OverwriteIfDirty")]
    [InlineData("OverwriteAlways")]
    public async Task ExecuteAsync_Succeeds_WithDifferentConflictResolutionModes(string conflictMode)
    {
        // Arrange
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Is(conflictMode),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("blob_autoimport");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--conflict-resolution-mode", conflictMode);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            conflictMode,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Enable")]
    [InlineData("Disable")]
    public async Task ExecuteAsync_Succeeds_WithDifferentAdminStatus(string adminStatus)
    {
        // Arrange
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Is(adminStatus),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("blob_autoimport");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--admin-status", adminStatus);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            adminStatus,
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-group rg1 --filesystem-name fs1", false)] // missing subscription
    [InlineData("--subscription sub123 --filesystem-name fs1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing filesystem-name
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
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName);

        // Assert
        Assert.True((int)response.Status >= 500);
        Assert.Contains("boom", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        Service.CreateAutoimportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns("autoimport-20250107120000");

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName);

        // Assert - verify command executed successfully with expected parameters
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAutoimportJobAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}

