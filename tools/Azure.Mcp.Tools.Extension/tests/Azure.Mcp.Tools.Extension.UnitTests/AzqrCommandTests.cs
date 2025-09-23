// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Core.Services.Time;
using Azure.Mcp.Tools.Extension.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

public sealed class AzqrCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IExternalProcessService _processService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AzqrCommand> _logger;

    public AzqrCommandTests()
    {
        _processService = Substitute.For<IExternalProcessService>();
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _logger = Substitute.For<ILogger<AzqrCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_processService);
        collection.AddSingleton(_subscriptionService);
        collection.AddSingleton(_dateTimeProvider);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessResult_WhenScanSucceeds()
    {
        // Arrange
        var fixedDateTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(fixedDateTime);

        var command = new AzqrCommand(_logger);

        var mockSubscriptionId = "12345678-1234-1234-1234-123456789012";
        var args = command.GetCommand().Parse($"--subscription {mockSubscriptionId}");
        var context = new CommandContext(_serviceProvider);

        var expectedOutput = "Scan completed successfully";
        var reportFilePath = Path.Combine(Path.GetTempPath(), $"azqr-report-{mockSubscriptionId}-{fixedDateTime:yyyyMMdd-HHmmss}");
        var xlsxReportFilePath = $"{reportFilePath}.xlsx";
        var jsonReportFilePath = $"{reportFilePath}.json";
        // Create empty files to simulate the report generation
        File.WriteAllText(xlsxReportFilePath, "");
        File.WriteAllText(jsonReportFilePath, "");

        // Create a temporary fake azqr executable
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var tempAzqrName = isWindows ? "azqr.exe" : "azqr";
        var tempAzqrPath = Path.Combine(Path.GetTempPath(), tempAzqrName);
        File.WriteAllText(tempAzqrPath, string.Empty); // Empty file is enough for path check

        // Set the private static _cachedAzqrPath field via reflection
        var field = typeof(AzqrCommand).GetField("_cachedAzqrPath", BindingFlags.Static | BindingFlags.NonPublic);
        var originalAzqrPath = field?.GetValue(null);
        field?.SetValue(null, tempAzqrPath);

        _processService.ExecuteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<IEnumerable<string>>())
            .Returns(new ProcessResult(0, expectedOutput, string.Empty, $"scan --subscription-id {mockSubscriptionId}"));

        try
        {
            // Act
            var response = await command.ExecuteAsync(context, args);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal("azqr report generated successfully.", response.Message);
            await _processService.Received().ExecuteAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<IEnumerable<string>>());
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
            if (File.Exists(tempAzqrPath))
            {
                File.Delete(tempAzqrPath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingSubscriptionArgument()
    {
        // Arrange
        var command = new AzqrCommand(_logger);

        var args = command.GetCommand().Parse(""); // No subscription specified
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
