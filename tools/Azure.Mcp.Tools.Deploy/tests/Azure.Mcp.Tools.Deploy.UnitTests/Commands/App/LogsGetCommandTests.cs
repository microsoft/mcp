// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Commands.App;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.App;

public class LogsGetCommandTests : CommandUnitTestsBase<LogsGetCommand, IDeployService>
{
    [Fact]
    public async Task Should_get_azd_app_logs()
    {
        // arrange
        var expectedLogs = "App logs retrieved:\n[2024-01-01 10:00:00] Application started\n[2024-01-01 10:01:00] Processing request";
        Service.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedLogs);

        // act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/Users/",
            "--azd-env-name", "dotnet-demo",
            "--limit", "10");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Equal(expectedLogs, result.Message);
    }

    [Fact]
    public async Task Should_get_azd_app_logs_with_default_limit()
    {
        // arrange
        var expectedLogs = "App logs retrieved:\nSample log entry";
        Service.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedLogs);

        // act
        // No limit specified - should use default
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/project",
            "--azd-env-name", "my-env");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Equal(expectedLogs, result.Message);
    }

    [Fact]
    public async Task Should_handle_no_logs_found()
    {
        // arrange
        Service.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>())
            .Returns("No logs found.");

        // act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/empty-project",
            "--azd-env-name", "empty-env",
            "--limit", "50");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.Equal("No logs found.", result.Message);
    }

    [Fact]
    public async Task Should_handle_error_during_log_retrieval()
    {
        // arrange
        var errorMessage = "Error during retrieval of app logs of azd project:\nNo resource group with tag {\"azd-env-name\": test-env} found.";
        Service.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>())
            .Returns(errorMessage);

        // act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/invalid-project",
            "--azd-env-name", "test-env");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Equal(errorMessage, result.Message);
    }

    [Fact]
    public async Task Should_handle_service_exception()
    {
        // arrange
        Service.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Failed to connect to Azure"));

        // act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/project",
            "--azd-env-name", "test-env");

        // assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status); // Should be an error status
        Assert.NotNull(result.Message);
        Assert.Contains("Failed to connect to Azure", result.Message);
    }

    [Fact]
    public async Task Should_validate_required_parameters()
    {
        // arrange & act - missing required workspace-folder parameter
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--azd-env-name", "test-env");

        // assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status); // Should fail validation
    }


}
