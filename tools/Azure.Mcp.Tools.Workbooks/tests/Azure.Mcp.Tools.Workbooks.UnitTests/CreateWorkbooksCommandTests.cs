// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class CreateWorkbooksCommandTests : CommandUnitTestsBase<CreateWorkbooksCommand, IWorkbooksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Contains("workbook", CommandDefinition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", CommandDefinition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("create", Command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create Workbook", Command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = Command.Description;
        Assert.NotNull(description);
        Assert.Contains("workbook", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subscription", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("display name", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("serialized", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesWorkbook_WhenValidParametersProvided()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: "Test Workbook",
            Description: null,
            Category: "workbook",
            Location: "West US 2",
            Kind: "shared",
            Tags: null,
            SerializedData: """{"items":[{"type":"text","content":"Test content"}]}""",
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: "azure monitor"
        );

        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Assert
        Assert.NotNull(result.Results);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await Service.Received(1).CreateWorkbookAsync(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[{"type":"text","content":"Test content"}]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesProvidedSourceId_WhenSpecified()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: "Test Workbook",
            Description: null,
            Category: null,
            Location: "West US 2",
            Kind: null,
            Tags: null,
            SerializedData: """{"items":[{"type":"text","content":"Test content"}]}""",
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: null
        );

        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""",
            "--source-id", "custom-source");

        // Assert
        await Service.Received(1).CreateWorkbookAsync(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[{"type":"text","content":"Test content"}]}""",
            "custom-source",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceReturnsNull()
    {
        // Arrange
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((WorkbookInfo?)null);

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[{"type":"text","content":"Test content"}]}""");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/test-id",
            DisplayName: null,
            Description: null,
            Category: null,
            Location: null,
            Kind: null,
            Tags: null,
            SerializedData: null,
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: null
        );

        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--display-name", "My Test Workbook",
            "--serialized-content", """{"version": "Notebook/1.0","items": [{"type": "1","content": "Hello World"}]}""");

        // Assert
        await Service.Received(1).CreateWorkbookAsync(
            "test-subscription",
            "test-resource-group",
            "My Test Workbook",
            """{"version": "Notebook/1.0","items": [{"type": "1","content": "Hello World"}]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""");

        // Assert
        await Service.Received(1).CreateWorkbookAsync(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Is<string?>(t => t == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""",
            "--auth-method", "1",
            "--tenant", "test-tenant");

        // Assert
        await Service.Received(1).CreateWorkbookAsync(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Any<RetryPolicyOptions?>(),
            "test-tenant",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidDisplayName_ReturnsValidationError(string? invalidDisplayName)
    {
        // Arrange
        var args = new List<string> { "--subscription", "test-sub", "--resource-group", "test-rg", "--serialized-content", """{"items":[]}""" };

        if (invalidDisplayName != null)
        {
            args.AddRange(["--display-name", invalidDisplayName]);
        }

        // Act
        var result = await ExecuteCommandAsync([.. args]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidSerializedContent_ReturnsValidationError(string? invalidSerializedContent)
    {
        // Arrange
        var args = new List<string> { "--subscription", "test-sub", "--resource-group", "test-rg", "--display-name", "Test Workbook" };

        if (invalidSerializedContent != null)
        {
            args.AddRange(["--serialized-content", invalidSerializedContent]);
        }

        // Act
        var result = await ExecuteCommandAsync([.. args]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexSerializedContent_HandlesCorrectly()
    {
        // Arrange
        var complexSerializedData = """
        {
            "version": "Notebook/1.0",
            "items": [
                {
                    "type": 1,
                    "content": "# Azure Workbook Dashboard\n\nThis is a test workbook with complex content."
                },
                {
                    "type": 3,
                    "content": {
                        "version": "KqlItem/1.0",
                        "query": "AzureActivity | summarize count() by ActivityStatus",
                        "size": 1,
                        "queryType": 0,
                        "resourceType": "microsoft.operationalinsights/workspaces"
                    }
                }
            ]
        }
        """;

        var workbook = new WorkbookInfo(
            WorkbookId: "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.Insights/workbooks/complex-id",
            DisplayName: "Complex Test Workbook",
            Description: null,
            Category: "workbook",
            Location: "West US 2",
            Kind: "shared",
            Tags: """{"Environment": "Test", "Team": "DevOps"}""",
            SerializedData: complexSerializedData,
            Version: null,
            TimeModified: null,
            UserId: null,
            SourceId: "azure monitor"
        );

        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Complex Test Workbook",
            "--serialized-content", complexSerializedData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryOptions_PassesCorrectParameters()
    {
        // Arrange
        var workbook = new WorkbookInfo("test-id", null, null, null, null, null, null, null, null, null, null, null);
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(workbook);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""",
            "--retry-max-retries", "5",
            "--retry-delay", "2.5",
            "--retry-max-delay", "30",
            "--retry-mode", "1");

        // Assert
        await Service.Received(1).CreateWorkbookAsync(
            "test-sub",
            "test-rg",
            "Test Workbook",
            """{"items":[]}""",
            "azure monitor",
            Arg.Is<RetryPolicyOptions?>(opts =>
                opts != null &&
                opts.MaxRetries == 5 &&
                opts.DelaySeconds == 2.5 &&
                opts.MaxDelaySeconds == 30),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionCorrectly_WhenExceptionOccurs()
    {
        // Arrange
        Service.CreateWorkbookAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid workbook data"));

        // Act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--display-name", "Test Workbook",
            "--serialized-content", """{"items":[]}""");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }
}
