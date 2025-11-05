// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Communication.UnitTests.Services;

public class CommunicationServiceTests
{
    private readonly ITenantService _mockTenantService;
    private readonly ILogger<CommunicationService> _mockLogger;
    private readonly CommunicationService _service;

    public CommunicationServiceTests()
    {
        _mockTenantService = Substitute.For<ITenantService>();
        _mockLogger = Substitute.For<ILogger<CommunicationService>>();
        _service = new CommunicationService(_mockTenantService, _mockLogger);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyEndpoint_ThrowsValidationException()
    {
        // Arrange
        string endpoint = string.Empty;
        string sender = "sender@example.com";
        string? senderName = string.Empty;
        string[] to = new[] { "recipient@example.com" };
        string subject = "Test Subject";
        string message = "Test Message";

        // Act & Assert
        // Updated to use Assert.ThrowsAsync for async methods
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SendEmailAsync(endpoint, sender, senderName, to, subject, message, false, null, null, null, null, null));

        Assert.Contains("endpoint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Additional tests would require refactoring the service to allow for mocking
    // of EmailClient or creating a wrapper interface to make it testable
}
