// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AvmModuleListCommandTests : CommandUnitTestsBase<AvmModuleListCommand, IAvmDocsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.NotEmpty(Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsModuleList()
    {
        var expectedModules = new List<AvmModule>
        {
            new()
            {
                ModuleName = "avm-res-storage-storageaccount",
                Description = "Azure Storage Account module",
                Source = "Azure/avm-res-storage-storageaccount/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount"
            },
            new()
            {
                ModuleName = "avm-res-compute-virtualmachine",
                Description = "Azure Virtual Machine module",
                Source = "Azure/avm-res-compute-virtualmachine/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-compute-virtualmachine"
            }
        };

        Service.ListModulesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedModules);

        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.ListModulesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var response = await ExecuteCommandAsync([]);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListModulesAsync(Arg.Any<CancellationToken>())
                .Returns(new List<AvmModule>());
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var expectedModules = new List<AvmModule>
        {
            new()
            {
                ModuleName = "avm-res-storage-storageaccount",
                Description = "Azure Storage Account module",
                Source = "Azure/avm-res-storage-storageaccount/azurerm",
                RepoUrl = "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount"
            }
        };

        Service.ListModulesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedModules);

        var response = await ExecuteCommandAsync([]);

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AvmModuleListResult);

        Assert.NotNull(result.Modules);
        Assert.Single(result.Modules);
        Assert.Equal("avm-res-storage-storageaccount", result.Modules[0].ModuleName);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);
    }
}
