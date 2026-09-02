// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.ProcessExecution;
using Microsoft.Mcp.Core.Services.Time;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Extension.Tests;

public sealed class AzqrCommandTests : SubscriptionCommandUnitTestsBase<AzqrCommand, IExternalProcessService>
{
    private readonly IAzureService _azureService;
    private readonly IAzqrCliService _azqrCliService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AzqrCommandTests()
    {
        _azureService = Substitute.For<IAzureService>();
        _azqrCliService = Substitute.For<IAzqrCliService>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _azqrCliService.GetSupportedExecutablePath().Returns("azqr");

        Services.AddSingleton(_azureService);
        Services.AddSingleton(_azqrCliService);
        Services.AddSingleton(_dateTimeProvider);
    }

    [Fact]
    public void Metadata_RequiresLocalExecution()
    {
        Assert.True(Command.Metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessResult_WhenScanSucceeds()
    {
        // Arrange
        var fixedDateTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(fixedDateTime);

        var mockSubscriptionId = "12345678-1234-1234-1234-123456789012";

        var expectedOutput = "Scan completed successfully";
        var reportFilePath = Path.Combine(Path.GetTempPath(), $"azqr-report-{mockSubscriptionId}-{fixedDateTime:yyyyMMdd-HHmmss}");
        var xlsxReportFilePath = $"{reportFilePath}.xlsx";
        var jsonReportFilePath = $"{reportFilePath}.json";
        // Create empty files to simulate the report generation
        File.WriteAllText(xlsxReportFilePath, "");
        File.WriteAllText(jsonReportFilePath, "");

        Service.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, string>?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, expectedOutput, string.Empty, $"scan --subscription-id {mockSubscriptionId}"));

        try
        {
            // Act
            var response = await ExecuteCommandAsync("--subscription", mockSubscriptionId);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal("azqr report generated successfully.", response.Message);
            await Service.Received().ExecuteAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IDictionary<string, string>?>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
        }
        finally
        {
            // Cleanup
            if (File.Exists(xlsxReportFilePath))
            {
                File.Delete(xlsxReportFilePath);
            }
            if (File.Exists(jsonReportFilePath))
            {
                File.Delete(jsonReportFilePath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingSubscriptionArgument()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
