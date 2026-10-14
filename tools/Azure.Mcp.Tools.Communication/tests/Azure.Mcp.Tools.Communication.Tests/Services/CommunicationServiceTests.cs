// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Communication.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.Tests.Services;

public class CommunicationServiceTests
{
    private readonly IAzureService _mockAzureService;
    private readonly ILogger<CommunicationService> _mockLogger;
    private readonly CommunicationService _service;

    public CommunicationServiceTests()
    {
        _mockAzureService = AzureServiceTestHelpers.CreateAzureService();
        _mockLogger = Substitute.For<ILogger<CommunicationService>>();
        _service = new CommunicationService(_mockAzureService, _mockLogger);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyEndpoint_ThrowsValidationException()
    {
        // Arrange
        string endpoint = string.Empty;
        string sender = "sender@example.com";
        string[] to = ["recipient@example.com"];
        string subject = "Test Subject";
        string message = "Test Message";

        // Act & Assert
        // Updated to use Assert.ThrowsAsync for async methods
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SendEmailAsync(endpoint, sender, to, subject, message, false, null, null, null, null, TestContext.Current.CancellationToken));

        Assert.Contains("endpoint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendEmailAsync_InUnsupportedCloud_ThrowsNotSupportedException()
    {
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzureChina);
        azureService.CloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzureChinaCloud);
        CommunicationService service = new(azureService, _mockLogger);

        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => service.SendEmailAsync(
                "https://example.communication.azure.com",
                "sender@example.com",
                ["recipient@example.com"],
                "Test Subject",
                "Test Message",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AzureChinaCloud", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendSmsAsync_InUnsupportedCloud_ThrowsNotSupportedException()
    {
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzureChina);
        azureService.CloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzureChinaCloud);
        CommunicationService service = new(azureService, _mockLogger);

        NotSupportedException exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => service.SendSmsAsync(
                "https://example.communication.azure.com",
                "+15555550100",
                ["+15555550101"],
                "Test Message",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AzureChinaCloud", exception.Message, StringComparison.Ordinal);
    }

    // Additional tests would require refactoring the service to allow for mocking
    // of EmailClient or creating a wrapper interface to make it testable
}
