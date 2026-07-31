// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationApplyCommandTests : CommandUnitTestsBase<RecommendationApplyCommand, object>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("apply", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void Command_HasCorrectProperties()
    {
        Assert.Equal("apply", Command.Name);
        Assert.Equal("Apply Advisor Recommendations", Command.Title);
        Assert.Equal("174fd0df-a11a-4139-b987-efd57611f62f", Command.Id);
        Assert.Contains("IaaC", Command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        var metadata = Command.Metadata;
        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("compute_virtualmachines")]
    [InlineData("storage_storageaccounts")]
    [InlineData("keyvault_vaults")]
    [InlineData("containerservice_managedclusters")]
    public async Task ExecuteAsync_ValidResource_ReturnsOk(string resource)
    {
        var response = await ExecuteCommandAsync("--resource", resource);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData("compute_virtualmachines")]
    [InlineData("sql_managedinstances")]
    public async Task ExecuteAsync_DeserializationValidation(string resource)
    {
        var response = await ExecuteCommandAsync("--resource", resource);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.ListString);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("rules", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResource_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidResource_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--resource", "invalid_resource_type");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid resource", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidResource_ListsAvailableResources()
    {
        var response = await ExecuteCommandAsync("--resource", "nonexistent");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Available resources", response.Message);
        Assert.Contains("compute_virtualmachines", response.Message);
    }

    [Theory]
    [InlineData("--resource compute_virtualmachines", true)]
    [InlineData("--resource storage_storageaccounts", true)]
    [InlineData("--resource invalid_type", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);

        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var command = Command.GetCommand();
        var parseResult = command.Parse("--resource compute_virtualmachines");

        Assert.Empty(parseResult.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCachedResultOnSubsequentCalls()
    {
        var response1 = await ExecuteCommandAsync("--resource", "keyvault_vaults");
        var response2 = await ExecuteCommandAsync("--resource", "keyvault_vaults");

        var result1 = ValidateAndDeserializeResponse(response1, AdvisorJsonContext.Default.ListString);
        var result2 = ValidateAndDeserializeResponse(response2, AdvisorJsonContext.Default.ListString);

        Assert.Single(result1);
        Assert.Single(result2);
        Assert.Equal(result1[0], result2[0]);
    }

    [Theory]
    [InlineData("aad_domainservices")]
    [InlineData("apimanagement_service")]
    [InlineData("cognitiveservices_accounts")]
    [InlineData("compute_virtualmachinescalesets")]
    [InlineData("containerregistry_registries")]
    [InlineData("dbforpostgresql_flexibleservers")]
    [InlineData("documentdb_databaseaccounts")]
    [InlineData("kubernetes_connectedclusters")]
    [InlineData("kubernetesconfiguration_extensions")]
    [InlineData("netapp_volumes")]
    [InlineData("network_applicationgatewaywebapplicationfirewallpolicies")]
    [InlineData("network_expressrouteports")]
    [InlineData("network_frontdoorwebapplicationfirewallpolicies")]
    [InlineData("web_serverfarms")]
    [InlineData("web_staticsites")]
    public async Task ExecuteAsync_AllResources_ReturnValidRules(string resource)
    {
        var response = await ExecuteCommandAsync("--resource", resource);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.ListString);
        Assert.NotEmpty(result);
        Assert.Contains("rules", result[0]);
    }
}
