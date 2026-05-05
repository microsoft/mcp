// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Workbooks.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class UpdateWorkbooksCommandTests : CommandUnitTestsBase<UpdateWorkbooksCommand, IWorkbooksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Contains("Updates properties", CommandDefinition.Description);
        Assert.Contains("workbook", CommandDefinition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update Workbook", Command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = Command.Description;
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

        Service.UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("Updated Test Workbook"),
            Arg.Is("{\"version\":\"Notebook/1.0\",\"updated\":true}"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        var response = await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Updated Test Workbook",
            "--serialized-content", "{\"version\":\"Notebook/1.0\",\"updated\":true}");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);

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

        Service.UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("New Display Name Only"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        var response = await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "New Display Name Only");

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);

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

        Service.UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is((string?)null),
            Arg.Is(newSerializedContent),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        var response = await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--serialized-content", newSerializedContent);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);

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

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", displayName,
            "--serialized-content", serializedContent);

        // Assert
        await Service.Received(1).UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is(displayName),
            Arg.Is(serializedContent),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceReturnsNull()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<WorkbookInfo?>(null));

        // Act
        var response = await ExecuteCommandAsync("--workbook-id", workbookId, "--display-name", "Test Name");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Failed to update workbook", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--workbook-id", workbookId, "--display-name", "Test Name");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
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
        string[] args = invalidWorkbookId == null
            ? ["--display-name", "Test Name"]
            : ["--workbook-id", invalidWorkbookId, "--display-name", "Test Name"];

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        Service.UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("Updated Complex Workbook"),
            Arg.Is(complexSerializedData),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        var response = await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Updated Complex Workbook",
            "--serialized-content", complexSerializedData);

        // Assert
        var result = ValidateAndDeserializeResponse(response, WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);

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

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--tenant", tenantId);

        // Assert
        await Service.Received(1).UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is(tenantId),
            Arg.Any<CancellationToken>());
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

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--auth-method", "1");

        // Assert
        await Service.Received(1).UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
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

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedWorkbook);

        // Act
        await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Test Workbook",
            "--retry-max-retries", "5",
            "--retry-delay", "2.5");

        // Assert
        await Service.Received(1).UpdateWorkbookAsync(
            Arg.Is(workbookId),
            Arg.Is("Test Workbook"),
            Arg.Is((string?)null),
            Arg.Is<RetryPolicyOptions?>(x => x != null && x.MaxRetries == 5 && System.Math.Abs(x.DelaySeconds - 2.5) < 1e-6),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionCorrectly_WhenExceptionOccurs()
    {
        // Arrange
        var workbookId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1";
        var exception = new Exception("Test exception");

        Service.UpdateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var response = await ExecuteCommandAsync(
            "--workbook-id", workbookId,
            "--display-name", "Test Name");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test exception", response.Message);
    }
}
