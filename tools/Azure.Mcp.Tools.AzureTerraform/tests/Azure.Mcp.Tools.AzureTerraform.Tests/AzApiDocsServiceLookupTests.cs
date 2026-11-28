// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.Tests;

public class AzApiDocsServiceLookupTests
{
    private readonly AzApiDocsService _service = new();

    [Fact]
    public void GetDocumentation_UnknownApiVersion_FallsBackToLatestStableWithNote()
    {
        var result = _service.GetDocumentation("Microsoft.Storage/storageAccounts", "2099-01-01");

        Assert.NotEqual("2099-01-01", result.ApiVersion);
        Assert.NotNull(result.Note);
        Assert.Contains("2099-01-01", result.Note);
        Assert.Contains(result.ApiVersion, result.Note);
    }

    [Fact]
    public void GetDocumentation_KnownApiVersion_HasNoNote()
    {
        var result = _service.GetDocumentation("Microsoft.Storage/storageAccounts", "2023-05-01");

        Assert.Equal("2023-05-01", result.ApiVersion);
        Assert.Null(result.Note);
    }

    [Fact]
    public void GetDocumentation_DifferentCasing_ReturnsCanonicalType()
    {
        var result = _service.GetDocumentation("microsoft.storage/STORAGEACCOUNTS");

        Assert.Equal("Microsoft.Storage/storageAccounts", result.ResourceType);
    }

    [Fact]
    public void GetDocumentation_VersionEmbeddedInType_IsUsed()
    {
        var result = _service.GetDocumentation("Microsoft.Storage/storageAccounts@2023-05-01");

        Assert.Equal("Microsoft.Storage/storageAccounts", result.ResourceType);
        Assert.Equal("2023-05-01", result.ApiVersion);
    }

    [Fact]
    public void GetDocumentation_Typo_SuggestsCloseMatch()
    {
        var ex = Assert.Throws<InvalidDataException>(() => _service.GetDocumentation("Microsoft.Storage/storageAccount"));

        Assert.Contains("Did you mean", ex.Message);
        Assert.Contains("Microsoft.Storage/storageAccounts", ex.Message);
    }

    [Fact]
    public void GetDocumentation_OversizedResourceType_ThrowsInvalidData()
    {
        string oversized = "Microsoft.Storage/" + new string('a', 100_000);

        var ex = Assert.Throws<InvalidDataException>(() => _service.GetDocumentation(oversized));

        Assert.Contains("too long", ex.Message);
    }

    [Fact]
    public void GetDocumentation_NoProviderSlash_ThrowsInvalidData()
    {
        Assert.Throws<InvalidDataException>(() => _service.GetDocumentation("storageAccounts"));
    }

    [Theory]
    [InlineData(" Microsoft.Web/sites/ ", null, "Microsoft.Web/sites", null)]
    [InlineData("Microsoft.Web/sites@2023-01-01", null, "Microsoft.Web/sites", "2023-01-01")]
    [InlineData("Microsoft.Web/sites@2023-01-01", "2022-03-01", "Microsoft.Web/sites", "2022-03-01")]
    [InlineData("Microsoft.Web/sites", " ", "Microsoft.Web/sites", null)]
    public void NormalizeInput_HandlesCommonShapes(string type, string? version, string expectedType, string? expectedVersion)
    {
        var (actualType, actualVersion) = AzApiDocsService.NormalizeInput(type, version);

        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedVersion, actualVersion);
    }
}
