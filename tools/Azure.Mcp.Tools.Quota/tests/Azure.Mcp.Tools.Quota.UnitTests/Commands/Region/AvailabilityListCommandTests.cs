// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Quota.Commands.Region;
using Azure.Mcp.Tools.Quota.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Quota.UnitTests.Commands.Region;

[Trait("Area", "Quota")]
public sealed class AvailabilityListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuotaService _quotaService;
    private readonly ILogger<AvailabilityListCommand> _logger;
    private readonly AvailabilityListCommand _command;
    private readonly Command _commandDefinition;

    public AvailabilityListCommandTests()
    {
        _quotaService = Substitute.For<IQuotaService>();
        _logger = Substitute.For<ILogger<AvailabilityListCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_quotaService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new AvailabilityListCommand(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_check_azure_regions_success()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "Microsoft.Web/sites, Microsoft.Storage/storageAccounts";

        var expectedRegions = new List<string>
        {
            "eastus",
            "westus",
            "eastus2",
            "westus2",
            "centralus"
        };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Is<string[]>(array =>
                    array.Length == 2 &&
                    array.Contains("Microsoft.Web/sites") &&
                    array.Contains("Microsoft.Storage/storageAccounts")),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct parameters
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array =>
                array.Length == 2 &&
                array.Contains("Microsoft.Web/sites") &&
                array.Contains("Microsoft.Storage/storageAccounts")),
            subscriptionId,
            null,
            null,
            null);

        // Verify the response structure
        var json = JsonSerializer.Serialize(result.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var response = JsonSerializer.Deserialize<AvailabilityListCommand.RegionCheckCommandResult>(json, options);
        Assert.NotNull(response);
        Assert.NotNull(response.AvailableRegions);
        Assert.Equal(5, response.AvailableRegions.Count);

        // Verify the expected regions are returned
        Assert.Contains("eastus", response.AvailableRegions);
        Assert.Contains("westus", response.AvailableRegions);
        Assert.Contains("eastus2", response.AvailableRegions);
        Assert.Contains("westus2", response.AvailableRegions);
        Assert.Contains("centralus", response.AvailableRegions);
    }

    [Fact]
    public async Task Should_check_regions_with_cognitive_services_success()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "Microsoft.CognitiveServices/accounts";
        var cognitiveServiceModelName = "gpt-4o";
        var cognitiveServiceDeploymentSkuName = "Standard";

        var expectedRegions = new List<string>
        {
            "eastus",
            "westus2",
            "northcentralus"
        };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Is<string[]>(array =>
                    array.Length == 1 &&
                    array.Contains("Microsoft.CognitiveServices/accounts")),
                subscriptionId,
                cognitiveServiceModelName,
                Arg.Any<string?>(),
                cognitiveServiceDeploymentSkuName)
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes,
            "--cognitive-service-model-name", cognitiveServiceModelName,
            "--cognitive-service-deployment-sku-name", cognitiveServiceDeploymentSkuName
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with the correct parameters
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array =>
                array.Length == 1 &&
                array.Contains("Microsoft.CognitiveServices/accounts")),
            subscriptionId,
            cognitiveServiceModelName,
            null,
            cognitiveServiceDeploymentSkuName);

        // Verify the response structure
        var json = JsonSerializer.Serialize(result.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var response = JsonSerializer.Deserialize<AvailabilityListCommand.RegionCheckCommandResult>(json, options);
        Assert.NotNull(response);
        Assert.NotNull(response.AvailableRegions);
        Assert.Equal(3, response.AvailableRegions.Count);

        // Verify the expected regions are returned
        Assert.Contains("eastus", response.AvailableRegions);
        Assert.Contains("westus2", response.AvailableRegions);
        Assert.Contains("northcentralus", response.AvailableRegions);
    }

    [Fact]
    public async Task Should_ReturnError_empty_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "";

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.Status);
        Assert.Contains("Missing Required options: --resource-types", result.Message);

        // Verify the service was not called
        await _quotaService.DidNotReceive().GetAvailableRegionsForResourceTypesAsync(
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Should_handle_service_exception()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "Microsoft.Web/sites";
        var expectedException = new Exception("Service error occurred");

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Any<string[]>(),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .ThrowsAsync(expectedException);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.Status);
        Assert.Contains("Service error occurred", result.Message);
    }

    [Fact]
    public async Task Should_parse_multiple_resource_types_with_spaces()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = " Microsoft.Web/sites , Microsoft.Storage/storageAccounts , Microsoft.Compute/virtualMachines ";

        var expectedRegions = new List<string> { "eastus", "westus2" };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Is<string[]>(array =>
                    array.Length == 3 &&
                    array.Contains("Microsoft.Web/sites") &&
                    array.Contains("Microsoft.Storage/storageAccounts") &&
                    array.Contains("Microsoft.Compute/virtualMachines")),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);

        // Verify the service was called with correctly parsed resource types
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array =>
                array.Length == 3 &&
                array.Contains("Microsoft.Web/sites") &&
                array.Contains("Microsoft.Storage/storageAccounts") &&
                array.Contains("Microsoft.Compute/virtualMachines")),
            subscriptionId,
            null,
            null,
            null);
    }

    [Fact]
    public async Task Should_return_null_results_when_no_regions_found()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "Microsoft.Web/sites";

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Any<string[]>(),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(new List<string>());

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Results); // Should be null when no regions are found
    }

    [Fact]
    public async Task Should_include_all_cognitive_service_parameters()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "Microsoft.CognitiveServices/accounts";
        var cognitiveServiceModelName = "gpt-4o";
        var cognitiveServiceModelVersion = "2024-05-13";
        var cognitiveServiceDeploymentSkuName = "Standard";

        var expectedRegions = new List<string> { "eastus" };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Any<string[]>(),
                subscriptionId,
                cognitiveServiceModelName,
                cognitiveServiceModelVersion,
                cognitiveServiceDeploymentSkuName)
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes,
            "--cognitive-service-model-name", cognitiveServiceModelName,
            "--cognitive-service-model-version", cognitiveServiceModelVersion,
            "--cognitive-service-deployment-sku-name", cognitiveServiceDeploymentSkuName
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);

        // Verify the service was called with all cognitive service parameters
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array =>
                array.Length == 1 &&
                array.Contains("Microsoft.CognitiveServices/accounts")),
            subscriptionId,
            cognitiveServiceModelName,
            cognitiveServiceModelVersion,
            cognitiveServiceDeploymentSkuName);
    }

    [Fact]
    public async Task Should_handle_whitespace_only_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "   ";

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.Status);
        Assert.Contains("Missing Required options: --resource-types", result.Message);

        // Verify the service was not called
        await _quotaService.DidNotReceive().GetAvailableRegionsForResourceTypesAsync(
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task Should_handle_mixed_casing_in_resource_types()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";
        var resourceTypes = "microsoft.web/SITES, MICROSOFT.Storage/storageaccounts, Microsoft.COMPUTE/VirtualMachines";

        var expectedRegions = new List<string> { "eastus", "westus2" };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Is<string[]>(array =>
                    array.Length == 3 &&
                    array.Contains("microsoft.web/SITES") &&
                    array.Contains("MICROSOFT.Storage/storageaccounts") &&
                    array.Contains("Microsoft.COMPUTE/VirtualMachines")),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);

        // Verify the service was called with resource types preserving original casing
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array =>
                array.Length == 3 &&
                array.Contains("microsoft.web/SITES") &&
                array.Contains("MICROSOFT.Storage/storageaccounts") &&
                array.Contains("Microsoft.COMPUTE/VirtualMachines")),
            subscriptionId,
            null,
            null,
            null);
    }

    [Fact]
    public async Task Should_handle_very_long_resource_types_list()
    {
        // Arrange
        var subscriptionId = "test-subscription-id";

        // Create a very long list of resource types
        var resourceTypesList = new List<string>();
        for (int i = 1; i <= 50; i++)
        {
            resourceTypesList.Add($"Microsoft.TestProvider{i}/resourceType{i}");
        }
        var resourceTypes = string.Join(", ", resourceTypesList);

        var expectedRegions = new List<string>
        {
            "eastus",
            "westus2",
            "centralus",
            "northeurope",
            "southeastasia"
        };

        _quotaService.GetAvailableRegionsForResourceTypesAsync(
                Arg.Is<string[]>(array => array.Length == 50),
                subscriptionId,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(expectedRegions);

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-types", resourceTypes
        ]);

        var context = new CommandContext(_serviceProvider);

        // Act
        var result = await _command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        // Verify the service was called with all 50 resource types
        await _quotaService.Received(1).GetAvailableRegionsForResourceTypesAsync(
            Arg.Is<string[]>(array => array.Length == 50),
            subscriptionId,
            null,
            null,
            null);

        // Verify the response structure
        var json = JsonSerializer.Serialize(result.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var response = JsonSerializer.Deserialize<AvailabilityListCommand.RegionCheckCommandResult>(json, options);
        Assert.NotNull(response);
        Assert.NotNull(response.AvailableRegions);
        Assert.Equal(5, response.AvailableRegions.Count);

        // Verify the expected regions are returned
        Assert.Contains("eastus", response.AvailableRegions);
        Assert.Contains("westus2", response.AvailableRegions);
        Assert.Contains("centralus", response.AvailableRegions);
        Assert.Contains("northeurope", response.AvailableRegions);
        Assert.Contains("southeastasia", response.AvailableRegions);
    }

}
