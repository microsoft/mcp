// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Commands;
using Azure.Mcp.Tools.Workbooks.Commands.Workbooks;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Workbooks.UnitTests;

public class ListWorkbooksCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkbooksService _service;
    private readonly ILogger<ListWorkbooksCommand> _logger;
    private readonly ListWorkbooksCommand _command;

    public ListWorkbooksCommandTests()
    {
        _service = Substitute.For<IWorkbooksService>();
        _logger = Substitute.For<ILogger<ListWorkbooksCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("workbooks", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("list", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("List Workbooks", _command.Title);
    }

    [Fact]
    public void Description_ContainsRequiredInformation()
    {
        var description = _command.Description;
        Assert.NotNull(description);
        Assert.Contains("workbooks", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resource group", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subscription", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWorkbooks_WhenWorkbooksExist()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Test Workbook 1",
                Description: "Test Description 1",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            ),
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook2",
                DisplayName: "Test Workbook 2",
                Description: "Test Description 2",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: "{}",
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user2",
                SourceId: "azure monitor"
            )
        };

        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedWorkbooks.Count, result.Workbooks.Count);
        Assert.Collection(result.Workbooks,
            workbook =>
            {
                Assert.Equal("Test Workbook 1", workbook.DisplayName);
                Assert.Contains("workbook1", workbook.WorkbookId);
            },
            workbook =>
            {
                Assert.Equal("Test Workbook 2", workbook.DisplayName);
                Assert.Contains("workbook2", workbook.WorkbookId);
            });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResults_WhenNoWorkbooksExist()
    {
        // Arrange
        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns([]);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Workbooks);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResults_WhenServiceReturnsNull()
    {
        // Arrange
        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(Task.FromResult<List<WorkbookInfo>>(null!));

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Workbooks);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(Task.FromException<List<WorkbookInfo>>(new Exception("Service error")));

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--tenant", "tenant123"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
        Assert.Contains("troubleshooting", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>();
        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--tenant", "test-tenant"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).ListWorkbooks(
            Arg.Is("test-subscription"),
            Arg.Is("test-resource-group"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullTenant_WhenTenantNotProvided()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>();
        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).ListWorkbooks(
            Arg.Is("test-subscription"),
            Arg.Is("test-resource-group"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesCorrectParameters()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>();
        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-resource-group",
            "--auth-method", "1"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        await _command.ExecuteAsync(context, args);

        // Assert
        await _service.Received(1).ListWorkbooks(
            Arg.Is("test-subscription"),
            Arg.Is("test-resource-group"),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithInvalidResourceGroup_ReturnsValidationError(string invalidResourceGroup)
    {
        // Arrange
        var args = _command.GetCommand().Parse([
            "--subscription", "valid-sub",
            "--resource-group", invalidResourceGroup
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("resource", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexWorkbookData_SerializesCorrectly()
    {
        // Arrange
        var complexSerializedData = @"{
            ""version"": ""Notebook/1.0"",
            ""items"": [
                {
                    ""type"": 1,
                    ""content"": {
                        ""json"": ""# Complex Workbook\n\nThis is a complex test workbook.""
                    }
                }
            ]
        }";

        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/complex",
                DisplayName: "Complex Test Workbook",
                Description: "A workbook with complex data",
                Category: "workbook",
                Location: "westus2",
                Kind: "shared",
                Tags: @"{""environment"": ""test"", ""team"": ""data""}",
                SerializedData: complexSerializedData,
                Version: "2.0",
                TimeModified: new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
                UserId: "complex-user-id",
                SourceId: "azure monitor"
            )
        };

        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<WorkbookFilters?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123"
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, WorkbooksJsonContext.Default.ListWorkbooksCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Workbooks);

        var workbook = result.Workbooks.First();
        Assert.Equal("Complex Test Workbook", workbook.DisplayName);
        Assert.Equal("2.0", workbook.Version);
        Assert.Contains("complex", workbook.WorkbookId);
        Assert.Contains("Notebook/1.0", workbook.SerializedData);
    }

    [Fact]
    public async Task ExecuteAsync_WithKindFilter_PassesCorrectFilter()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Shared Workbook",
                Description: "A shared workbook",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared"
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFilter_PassesCorrectFilter()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Sentinel Workbook",
                Description: "A sentinel workbook",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == "sentinel"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--category", "sentinel"
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == "sentinel"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithSourceIdFilter_PassesCorrectFilter()
    {
        // Arrange
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/components/myapp";
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "App Insights Workbook",
                Description: "A workbook linked to App Insights",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: sourceId
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--source-id", sourceId
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleFilters_PassesCorrectFilters()
    {
        // Arrange
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/components/myapp";
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Filtered Workbook",
                Description: "A workbook with multiple filters",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: sourceId
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared",
            "--category", "sentinel",
            "--source-id", sourceId
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAllFiltersApplied_PassesCorrectFilters()
    {
        // Arrange
        var sourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/components/myapp";
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Filtered Workbook",
                Description: "A workbook with multiple filters",
                Category: "sentinel",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: sourceId
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", "shared",
            "--category", "sentinel",
            "--source-id", sourceId
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == "shared" && f.Category == "sentinel" && f.SourceId == sourceId),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_PassesEmptyFilters()
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>
        {
            new(
                WorkbookId: "/subscriptions/sub1/resourceGroups/rg1/providers/microsoft.insights/workbooks/workbook1",
                DisplayName: "Unfiltered Workbook",
                Description: "A workbook without filters",
                Category: "workbook",
                Location: "eastus",
                Kind: "shared",
                Tags: null,
                SerializedData: "{\"version\":\"Notebook/1.0\"}",
                Version: "1.0",
                TimeModified: DateTimeOffset.UtcNow,
                UserId: "user1",
                SourceId: "azure monitor"
            )
        };

        _service.ListWorkbooks(
            Arg.Is("sub123"),
            Arg.Is("rg123"),
            Arg.Is<WorkbookFilters?>(f => f != null && !f.HasFilters),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123"
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && !f.HasFilters),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Theory]
    [InlineData("shared")]
    [InlineData("user")]
    public async Task ExecuteAsync_WithValidKind_AcceptsKindValue(string kind)
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>();
        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == kind),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--kind", kind
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Kind == kind),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }

    [Theory]
    [InlineData("workbook")]
    [InlineData("sentinel")]
    [InlineData("TSG")]
    [InlineData("application")]
    public async Task ExecuteAsync_WithValidCategory_AcceptsCategoryValue(string category)
    {
        // Arrange
        var expectedWorkbooks = new List<WorkbookInfo>();
        _service.ListWorkbooks(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == category),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>())
            .Returns(expectedWorkbooks);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg123",
            "--category", category
        ]);

        // Act
        var context = new CommandContext(_serviceProvider);
        var response = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);

        await _service.Received(1).ListWorkbooks(
            "sub123",
            "rg123",
            Arg.Is<WorkbookFilters?>(f => f != null && f.Category == category),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<string?>());
    }
}
