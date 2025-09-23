// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Quota.Commands;
using Azure.Mcp.Tools.Quota.Commands.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Quota.UnitTests.Commands.Usage;

[Trait("Area", "Quota")]
public sealed class CheckCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuotaService _quotaService;
    private readonly ILogger<CheckCommand> _logger;
    private readonly CheckCommand _command;
    private readonly Command _commandDefinition;

    public CheckCommandTests()
    {
        _quotaService = Substitute.For<IQuotaService>();
        _logger = Substitute.For<ILogger<CheckCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_quotaService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new CheckCommand(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_check_azure_quota_success()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "Microsoft.App, Microsoft.Storage/storageAccounts";

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>
        {
            {
                "Microsoft.App",
                new List<UsageInfo>
                {
                    new("ContainerApps", 100, 5, "Count"),
                    new("ContainerAppsEnvironments", 10, 2, "Count")
                }
            },
            {
                "Microsoft.Storage/storageAccounts",
                new List<UsageInfo>
                {
                    new("StorageAccounts", 250, 15, "Count"),
                    new("TotalStorageSize", 500, 150, "TB")
                }
            }
        };

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list =>
                    list.Count == 2 &&
                    list.Contains("Microsoft.App") &&
                    list.Contains("Microsoft.Storage/storageAccounts")),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct parameters
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list =>
                list.Count == 2 &&
                list.Contains("Microsoft.App") &&
                list.Contains("Microsoft.Storage/storageAccounts")),
            subscriptionId,
            region);

        // Verify the response structure
        var json = JsonSerializer.Serialize(result.Results);
        var response = JsonSerializer.Deserialize(json, QuotaJsonContext.Default.UsageCheckCommandResult);

        Assert.NotNull(response);
        Assert.NotNull(response.UsageInfo);
        Assert.Equal(2, response.UsageInfo.Count);

        // Verify Microsoft.App quotas
        Assert.True(response.UsageInfo.ContainsKey("Microsoft.App"));
        var appQuotas = response.UsageInfo["Microsoft.App"];
        Assert.Equal(2, appQuotas.Count);
        Assert.Contains(appQuotas, q => q.Name == "ContainerApps" && q.Limit == 100 && q.Used == 5);
        Assert.Contains(appQuotas, q => q.Name == "ContainerAppsEnvironments" && q.Limit == 10 && q.Used == 2);

        // Verify Microsoft.Storage/storageAccounts quotas
        Assert.True(response.UsageInfo.ContainsKey("Microsoft.Storage/storageAccounts"));
        var storageQuotas = response.UsageInfo["Microsoft.Storage/storageAccounts"];
        Assert.Equal(2, storageQuotas.Count);
        Assert.Contains(storageQuotas, q => q.Name == "StorageAccounts" && q.Limit == 250 && q.Used == 15);
        Assert.Contains(storageQuotas, q => q.Name == "TotalStorageSize" && q.Limit == 500 && q.Used == 150);
    }

    [Fact]
    public async Task Should_ReturnError_empty_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "";

        _quotaService.GetAzureQuotaAsync(
                Arg.Any<List<string>>(),
                subscriptionId,
                region)
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task Should_handle_service_exception()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "Microsoft.App";
        var expectedException = new Exception("Service error occurred");

        _quotaService.GetAzureQuotaAsync(
                Arg.Any<List<string>>(),
                subscriptionId,
                region)
            .ThrowsAsync(expectedException);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains("Service error occurred", result.Message);
    }

    [Fact]
    public async Task Should_parse_resource_types_with_spaces()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "westus2";
        var resourceTypes = " Microsoft.Web/sites , Microsoft.Storage/storageAccounts , Microsoft.Compute/virtualMachines ";

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>
        {
            { "Microsoft.Web/sites", new List<UsageInfo> { new("WebApps", 10, 3, "Count") } },
            { "Microsoft.Storage/storageAccounts", new List<UsageInfo> { new("StorageAccounts", 250, 15, "Count") } },
            { "Microsoft.Compute/virtualMachines", new List<UsageInfo> { new("VMs", 50, 10, "Count") } }
        };

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list =>
                    list.Count == 3 &&
                    list.Contains("Microsoft.Web/sites") &&
                    list.Contains("Microsoft.Storage/storageAccounts") &&
                    list.Contains("Microsoft.Compute/virtualMachines")),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        // Verify the service was called with correctly parsed resource types
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list =>
                list.Count == 3 &&
                list.Contains("Microsoft.Web/sites") &&
                list.Contains("Microsoft.Storage/storageAccounts") &&
                list.Contains("Microsoft.Compute/virtualMachines")),
            subscriptionId,
            region);
    }

    [Fact]
    public async Task Should_return_empty_results_when_no_quotas_found()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "Microsoft.App";

        _quotaService.GetAzureQuotaAsync(
                Arg.Any<List<string>>(),
                subscriptionId,
                region)
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results); // Should be empty when no quotas are found

        var json = JsonSerializer.Serialize(result.Results);
        var response = JsonSerializer.Deserialize(json, QuotaJsonContext.Default.UsageCheckCommandResult);

        Assert.NotNull(response);
        Assert.Empty(response.UsageInfo);
    }

    [Fact]
    public async Task Should_handle_whitespace_only_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "     ";

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
    }

    [Fact]
    public async Task Should_handle_mixed_casing_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "MICROSOFT.APP, microsoft.storage/storageaccounts, Microsoft.Compute/VirtualMachines";

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>
        {
            { "MICROSOFT.APP", new List<UsageInfo> { new("ContainerApps", 100, 5, "Count") } },
            { "microsoft.storage/storageaccounts", new List<UsageInfo> { new("StorageAccounts", 250, 15, "Count") } },
            { "Microsoft.Compute/VirtualMachines", new List<UsageInfo> { new("VMs", 50, 10, "Count") } }
        };

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list =>
                    list.Count == 3 &&
                    list.Contains("MICROSOFT.APP") &&
                    list.Contains("microsoft.storage/storageaccounts") &&
                    list.Contains("Microsoft.Compute/VirtualMachines")),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct casing preserved
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list =>
                list.Count == 3 &&
                list.Contains("MICROSOFT.APP") &&
                list.Contains("microsoft.storage/storageaccounts") &&
                list.Contains("Microsoft.Compute/VirtualMachines")),
            subscriptionId,
            region);
    }

    [Fact]
    public async Task Should_handle_unsupported_provider_returns_no_limit()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "Microsoft.UnsupportedProvider/resourceType";

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>
        {
            {
                "Microsoft.UnsupportedProvider/resourceType",
                new List<UsageInfo>
                {
                    new("Microsoft.UnsupportedProvider/resourceType", 0, 0, null, "No Limit")
                }
            }
        };

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list =>
                    list.Count == 1 &&
                    list.Contains("Microsoft.UnsupportedProvider/resourceType")),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct parameters
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list =>
                list.Count == 1 &&
                list.Contains("Microsoft.UnsupportedProvider/resourceType")),
            subscriptionId,
            region);

        // Verify the response structure
        var json = JsonSerializer.Serialize(result.Results);
        var response = JsonSerializer.Deserialize(json, QuotaJsonContext.Default.UsageCheckCommandResult);

        Assert.NotNull(response);
        Assert.NotNull(response.UsageInfo);
        Assert.True(response.UsageInfo.ContainsKey("Microsoft.UnsupportedProvider/resourceType"));

        var usageInfo = response.UsageInfo["Microsoft.UnsupportedProvider/resourceType"];
        Assert.Single(usageInfo);
        Assert.Equal("No Limit", usageInfo[0].Description);
        Assert.Equal(0, usageInfo[0].Limit);
        Assert.Equal(0, usageInfo[0].Used);
    }

    [Fact]
    public async Task Should_handle_very_long_resource_types_list()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";

        // Create a very long list of resource types
        var resourceTypesList = new List<string>();
        for (int i = 1; i <= 50; i++)
        {
            resourceTypesList.Add($"Microsoft.TestProvider{i}/resourceType{i}");
        }
        var resourceTypes = string.Join(", ", resourceTypesList);

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>();
        foreach (var resourceType in resourceTypesList)
        {
            expectedQuotaInfo.Add(resourceType,
            [
                new($"Resource{resourceType.Split('/')[1]}", 100, 10, "Count")
            ]);
        }

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list => list.Count == 50),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with all 50 resource types
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list => list.Count == 50),
            subscriptionId,
            region);

        // Verify the response contains all expected resource types
        var json = JsonSerializer.Serialize(result.Results);
        var response = JsonSerializer.Deserialize(json, QuotaJsonContext.Default.UsageCheckCommandResult);

        Assert.NotNull(response);
        Assert.NotNull(response.UsageInfo);
        Assert.Equal(50, response.UsageInfo.Count);
    }

    [Fact]
    public async Task Should_handle_network_failure_returns_descriptive_usage_info()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var region = "eastus";
        var resourceTypes = "Microsoft.Storage/storageAccounts";

        var expectedQuotaInfo = new Dictionary<string, List<UsageInfo>>
        {
            {
                "Microsoft.Storage/storageAccounts",
                new List<UsageInfo>
                {
                    new("Microsoft.Storage/storageAccounts", 0, 0, null, "Network failure occurred while retrieving quota information")
                }
            }
        };

        _quotaService.GetAzureQuotaAsync(
                Arg.Is<List<string>>(list =>
                    list.Count == 1 &&
                    list.Contains("Microsoft.Storage/storageAccounts")),
                subscriptionId,
                region)
            .Returns(expectedQuotaInfo);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--region", region,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct parameters
        await _quotaService.Received(1).GetAzureQuotaAsync(
            Arg.Is<List<string>>(list =>
                list.Count == 1 &&
                list.Contains("Microsoft.Storage/storageAccounts")),
            subscriptionId,
            region);

        // Verify the response structure contains descriptive error in Description
        var json = JsonSerializer.Serialize(result.Results);
        var response = JsonSerializer.Deserialize(json, QuotaJsonContext.Default.UsageCheckCommandResult);

        Assert.NotNull(response);
        Assert.NotNull(response.UsageInfo);
        Assert.True(response.UsageInfo.ContainsKey("Microsoft.Storage/storageAccounts"));

        var usageInfo = response.UsageInfo["Microsoft.Storage/storageAccounts"];
        Assert.Single(usageInfo);
        Assert.Equal("Network failure occurred while retrieving quota information", usageInfo[0].Description);
        Assert.Equal(0, usageInfo[0].Limit);
        Assert.Equal(0, usageInfo[0].Used);
    }
}
