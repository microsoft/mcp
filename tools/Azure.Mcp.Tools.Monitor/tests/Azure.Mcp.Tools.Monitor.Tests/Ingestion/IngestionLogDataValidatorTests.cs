// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Validation;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Ingestion;

public class IngestionLogDataValidatorTests
{
    private static readonly Dictionary<string, string> s_schema = new()
    {
        ["TimeGenerated"] = "datetime",
        ["Message"] = "string",
        ["Count"] = "int",
        ["Enabled"] = "boolean",
        ["Id"] = "guid",
        ["Payload"] = "dynamic"
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json")]
    [InlineData("{\"a\":1}")]
    [InlineData("[]")]
    [InlineData("[1,2]")]
    public void Validate_InvalidShape_ReturnsErrors(string? data)
    {
        var result = IngestionLogDataValidator.Validate(data, null);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.False(result.SchemaChecked);
    }

    [Fact]
    public void Validate_ValidShapeWithoutSchema_IsValid()
    {
        var result = IngestionLogDataValidator.Validate("[{\"a\":1},{\"b\":\"x\"}]", null);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.RecordCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_PayloadTooLarge_ReturnsError()
    {
        var data = "[{\"a\":\"" + new string('x', IngestionLogDataValidator.MaxPayloadBytes) + "\"}]";

        var result = IngestionLogDataValidator.Validate(data, null);

        Assert.False(result.IsValid);
        Assert.Contains("maximum request size", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_MatchingSchema_IsValid()
    {
        const string data = "[{\"TimeGenerated\":\"2024-01-01T00:00:00Z\",\"Message\":\"hi\",\"Count\":3,\"Enabled\":true,\"Id\":\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\",\"Payload\":{\"x\":1}}]";

        var result = IngestionLogDataValidator.Validate(data, s_schema);

        Assert.True(result.IsValid);
        Assert.True(result.SchemaChecked);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Validate_TypeMismatches_ReportRecordAndColumn()
    {
        const string data = "[{\"TimeGenerated\":\"nope\",\"Message\":5,\"Count\":1.5,\"Enabled\":\"yes\",\"Id\":\"x\"},{\"Message\":null}]";

        var result = IngestionLogDataValidator.Validate(data, s_schema);

        Assert.False(result.IsValid);
        Assert.Equal(5, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Equal(0, e.RecordIndex));
        Assert.Contains(result.Errors, e => e.Column == "Count");
    }

    [Fact]
    public void Validate_UndeclaredAndMissingColumns_AreWarnings()
    {
        const string data = "[{\"Message\":\"hi\",\"Extra\":1}]";

        var result = IngestionLogDataValidator.Validate(data, s_schema);

        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, w => w.Column == "Extra");
        Assert.Contains(result.Warnings, w => w.Column == "TimeGenerated");
    }

    [Fact]
    public void Validate_ManyErrors_AreCapped()
    {
        var records = string.Join(",", Enumerable.Repeat("{\"Count\":\"x\"}", 200));

        var result = IngestionLogDataValidator.Validate($"[{records}]", s_schema);

        Assert.False(result.IsValid);
        Assert.Equal(50, result.Errors.Count);
    }
}
