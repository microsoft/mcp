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

public class AvmVersionListCommandTests : CommandUnitTestsBase<AvmVersionListCommand, IAvmDocsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("versions", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.NotEmpty(Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ValidModuleName_ReturnsVersions()
    {
        var expectedVersions = new List<AvmVersion>
        {
            new() { TagName = "0.4.0", CreatedAt = "2024-12-01T00:00:00Z", TarballUrl = "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/tarball/v0.4.0" },
            new() { TagName = "0.3.0", CreatedAt = "2024-10-01T00:00:00Z", TarballUrl = "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/tarball/v0.3.0" }
        };

        Service.GetVersionsAsync("avm-res-storage-storageaccount", Arg.Any<CancellationToken>())
            .Returns(expectedVersions);

        var response = await ExecuteCommandAsync("--module-name", "avm-res-storage-storageaccount");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingModuleName_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.GetVersionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Module not found"));

        var response = await ExecuteCommandAsync("--module-name", "nonexistent-module");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--module-name avm-res-storage-storageaccount", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetVersionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns([]);
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
        var expectedVersions = new List<AvmVersion>
        {
            new() { TagName = "0.4.0", CreatedAt = "2024-12-01T00:00:00Z", TarballUrl = "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/tarball/v0.4.0" }
        };

        Service.GetVersionsAsync("avm-res-storage-storageaccount", Arg.Any<CancellationToken>())
            .Returns(expectedVersions);

        var response = await ExecuteCommandAsync("--module-name", "avm-res-storage-storageaccount");

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.AvmVersionListResult);

        Assert.Equal("avm-res-storage-storageaccount", result.ModuleName);
        Assert.NotNull(result.Versions);
        Assert.Single(result.Versions);
        Assert.Equal("0.4.0", result.Versions[0].TagName);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse(["--module-name", "avm-res-storage-storageaccount"]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--module-name");
    }
}
