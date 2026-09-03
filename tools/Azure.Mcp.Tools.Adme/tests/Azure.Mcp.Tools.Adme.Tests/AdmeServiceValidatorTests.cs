// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests;

public sealed class AdmeServiceValidatorTests
{
    [Theory]
    [InlineData("opendes:master-data--Well:W-99")]
    [InlineData("opendes:work-product-component--SeismicBinGrid:grid-1")]
    [InlineData("opendes:master-data--Well:")]
    [InlineData("opendes:master-data--Well:record:123:")]
    public void ValidateRecordId_WithValidId_DoesNotAddError(string id)
    {
        var validationResult = new ValidationResult();

        AdmeServiceValidator.ValidateRecordId(id, "--id", validationResult);

        Assert.Empty(validationResult.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(":master-data--Well:W-99")]
    [InlineData("opendes::W-99")]
    [InlineData("opendes:master-data-Well:W-99")]
    [InlineData("opendes:master-data--:W-99")]
    [InlineData("opendes:master data--Well:W-99")]
    public void ValidateRecordId_WithInvalidId_AddsError(string? id)
    {
        var validationResult = new ValidationResult();

        AdmeServiceValidator.ValidateRecordId(id, "--id", validationResult);

        var error = Assert.Single(validationResult.Errors);
        Assert.StartsWith("--id", error);
    }

    [Theory]
    [InlineData("osdu:wks:master-data--Well:1.0.0")]
    [InlineData("opendes:bulkupdate:test:1.1.1784669899838")]
    [InlineData("*:wks:master-data--Well:1.0.0")]
    [InlineData("osdu:*:master-data--Well:1.*.*")]
    [InlineData("osdu:wks:*:*")]
    public void ValidateKind_WithValidSelector_DoesNotAddError(string kind)
    {
        var validationResult = new ValidationResult();

        AdmeServiceValidator.ValidateKind(kind, validationResult, allowWildcards: true);

        Assert.Empty(validationResult.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("osdu:wks:master-data--Well")]
    [InlineData("osdu:wks:master-data--Well:1.0")]
    [InlineData("os*du:wks:master-data--Well:1.0.0")]
    [InlineData("osdu:wks:master data--Well:1.0.0")]
    public void ValidateKind_WithInvalidSelector_AddsError(string kind)
    {
        var validationResult = new ValidationResult();

        AdmeServiceValidator.ValidateKind(kind, validationResult, allowWildcards: true);

        Assert.Single(validationResult.Errors);
    }

    [Theory]
    [InlineData("osdu:wks:master-data--Well:1.0.0", false)]
    [InlineData("osdu:wks:master-data--Well:*", true)]
    [InlineData("osdu:wks:*:1.0.0", true)]
    public void ValidateKind_WithoutWildcardSupport_RejectsSelectors(string kind, bool expectsError)
    {
        var validationResult = new ValidationResult();

        AdmeServiceValidator.ValidateKind(kind, validationResult);

        Assert.Equal(expectsError, validationResult.Errors.Count > 0);
    }

    [Theory]
    [InlineData(TestConstants.Endpoint)]
    [InlineData("https://sample.oep.ppe.azure-int.net")]
    public void ValidateEndpoint_AcceptsTrustedEndpoint(string endpoint)
    {
        var result = AdmeServiceValidator.ValidateEndpoint(new Uri(endpoint));

        Assert.Equal(endpoint, result.AbsoluteUri.TrimEnd('/'));
    }
}
