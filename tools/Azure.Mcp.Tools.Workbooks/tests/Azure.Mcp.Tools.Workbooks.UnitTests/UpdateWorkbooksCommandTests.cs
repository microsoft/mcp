// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class UpdateWorkbooksCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkbooksService _service;
    private readonly ILogger<UpdateWorkbooksCommand> _logger;
    private readonly UpdateWorkbooksCommand _command;

    public UpdateWorkbooksCommandTests()
    {
        _service = Substitute.For<IWorkbooksService>();
        _logger = Substitute.For<ILogger<UpdateWorkbooksCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Updates properties", command.Description);
        Assert.Contains("workbook", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update Workbook", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.NotNull(description);
        Assert.Contains("Updates properties", description);
        Assert.Contains("workbook", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("display name", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("serialized content", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesWorkbook_WhenValidParametersProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Updated Test Workbook",
            Description: "Updated Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\",\"updated\":true}",
            Version: "2.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("Updated Test Workbook"),
            Arg.Is("{\"version\":\"Notebook/1.0\",\"updated\":true}"),
            Arg.Any<RetryPolicyOptions>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Updated Test Workbook",
            "--serialized-content", "{\"version\":\"Notebook/1.0\",\"updated\":true}"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<UpdateWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal("Updated Test Workbook", result.Workbook.DisplayName);
        Assert.Equal("2.0", result.Workbook.Version);
        Assert.Contains("updated", result.Workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesOnlyDisplayName_WhenOnlyDisplayNameProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "New Display Name Only",
            Description: "Original Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\"}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("New Display Name Only"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "New Display Name Only"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<UpdateWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal("New Display Name Only", result.Workbook.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesOnlySerializedContent_WhenOnlySerializedContentProvided()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var newSerializedContent = "{\"version\":\"Notebook/2.0\",\"content\":\"new\"}";
        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Original Display Name",
            Description: "Original Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: newSerializedContent,
            Version: "2.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is((string?)null),
            Arg.Is(newSerializedContent),
            Arg.Any<RetryPolicyOptions>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--serialized-content", newSerializedContent
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<UpdateWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Contains("Notebook/2.0", result.Workbook.SerializedData);
        Assert.Contains("new", result.Workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var workbookId = "/subscriptions/test-sub/resourceGroups/test-rg/providers/microsoft.insights/workbooks/test-workbook";
        var displayName = "Test Display Name";
        var serializedContent = "{\"test\":\"content\"}";

        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: displayName,
            Description: "Test Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: serializedContent,
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", displayName,
            "--serialized-content", serializedContent
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is(displayName),
            Arg.Is(serializedContent),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is((string?)null));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceReturnsNull()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromResult<WorkbookInfo?>(null));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Name"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Failed to update workbook", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromException<WorkbookInfo?>(new Exception("Service error")));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Name"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidWorkbookId_ReturnsValidationError(string? invalidWorkbookId)
    {
        // Arrange
        var args = invalidWorkbookId == null
            ? _command.GetCommand().Parse(["--display-name", "Test Name"])
            : _command.GetCommand().Parse([
                "--workbook-id", invalidWorkbookId,
                "--display-name", "Test Name"
            ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("workbook", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexSerializedContent_HandlesCorrectly()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/complex";
        var complexSerializedData = @"{
            ""version"": ""Notebook/1.0"",
            ""items"": [
                {
                    ""type"": 1,
                    ""content"": {
                        ""json"": ""# Updated Complex Workbook\n\nThis workbook has been updated with complex data.""
                    }
                },
                {
                    ""type"": 3,
                    ""content"": {
                        ""version"": ""KqlItem/1.0"",
                        ""query"": ""requests | take 10"",
                        ""size"": 1
                    }
                }
            ],
            ""styleSettings"": {
                ""paddingStyle"": ""narrow""
            }
        }";

        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Updated Complex Workbook",
            Description: "A workbook with updated complex data",
            Category: "workbook",
            Location: "westus2",
            Kind: "shared",
            Tags: @"{""environment"": ""test"", ""updated"": ""true""}",
            SerializedData: complexSerializedData,
            Version: "3.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "complex-user-id",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("Updated Complex Workbook"),
            Arg.Is(complexSerializedData),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Updated Complex Workbook",
            "--serialized-content", complexSerializedData
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<UpdateWorkbooksCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal("Updated Complex Workbook", result.Workbook.DisplayName);
        Assert.Equal("3.0", result.Workbook.Version);
        Assert.Contains("Updated Complex Workbook", result.Workbook.SerializedData);
        Assert.Contains("KqlItem/1.0", result.Workbook.SerializedData);
        Assert.Contains("requests | take 10", result.Workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var tenantId = "test-tenant-123";

        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test Workbook",
            Description: "Test Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\"}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--tenant", tenantId
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Is(tenantId));
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test Workbook",
            Description: "Test Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\"}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--auth-method", "1"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryOptions_PassesCorrectParameters()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        var updatedWorkbook = new WorkbookInfo(
            WorkbookId: workbookId,
            DisplayName: "Test Workbook",
            Description: "Test Description",
            Category: "workbook",
            Location: "eastus",
            Kind: "shared",
            Tags: "{}",
            SerializedData: "{\"version\":\"Notebook/1.0\"}",
            Version: "1.0",
            TimeModified: DateTimeOffset.UtcNow,
            UserId: "user1",
            SourceId: "azure monitor"
        );

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(updatedWorkbook);

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--retry-max-retries", "5",
            "--retry-delay", "2.5"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).UpdateWorkbook(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Is<RetryPolicyOptions>(x => x.MaxRetries == 5 && x.DelaySeconds == 2.5),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionCorrectly_WhenExceptionOccurs()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var exception = new Exception("Test exception");

        _service.UpdateWorkbook(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<string?>())
            .Returns(Task.FromException<WorkbookInfo?>(exception));

        var args = _command.GetCommand().Parse([
            "--workbook-id", workbookId,
            "--display-name", "Test Name"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test exception", response.Message);
    }

    private class UpdateWorkbooksCommandResult
    {
        public WorkbookInfo Workbook { get; set; } = null!;
    }
}
