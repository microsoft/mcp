// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CostManagement.Services;
using Xunit;

namespace Azure.Mcp.Tools.CostManagement.UnitTests.Services;

// MapResult is covered via live tests; QueryResult has internal-only ctor in
// Azure.ResourceManager.CostManagement 1.0.3 and ArmCostManagementModelFactory
// does not expose a builder for it. The static helpers below are exercised directly.
public class CostManagementServiceTests
{
    [Theory]
    [InlineData(new[] { "Cost", "Currency" }, "Cost", 0)]
    [InlineData(new[] { "PreTaxCost", "Currency" }, "Cost", -1)]
    [InlineData(new[] { "PreTaxCost", "Currency" }, "PreTaxCost", 0)]
    [InlineData(new[] { "CostUSD", "Currency" }, "CostUSD", 0)]
    public void ResolveColumnIndex_FindsFirstAvailableCandidate(string[] columns, string firstCandidate, int expected)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < columns.Length; i++)
        {
            index[columns[i]] = i;
        }

        var result = CostManagementService.ResolveColumnIndex(index, firstCandidate);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveColumnIndex_PicksFirstAvailableFromMultipleCandidates()
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["PreTaxCost"] = 1,
            ["Currency"] = 2
        };

        var result = CostManagementService.ResolveColumnIndex(index, "Cost", "PreTaxCost", "CostUSD");

        Assert.Equal(1, result);
    }

    [Fact]
    public void ReadDecimal_ReturnsZero_ForNullData()
    {
        var result = CostManagementService.ReadDecimal(null, "Cost");
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ReadDecimal_ParsesNumber()
    {
        var data = BinaryData.FromString("123.45");
        var result = CostManagementService.ReadDecimal(data, "Cost");
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ReadDecimal_ParsesQuotedString()
    {
        var data = BinaryData.FromString("\"99.5\"");
        var result = CostManagementService.ReadDecimal(data, "Cost");
        Assert.Equal(99.5m, result);
    }

    [Fact]
    public void ReadDecimal_ReturnsZero_ForJsonNull()
    {
        var data = BinaryData.FromString("null");
        var result = CostManagementService.ReadDecimal(data, "Cost");
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ReadDecimal_Throws_ForNonNumericContent()
    {
        var data = BinaryData.FromString("true");
        var ex = Assert.Throws<InvalidOperationException>(() => CostManagementService.ReadDecimal(data, "Cost"));
        Assert.Contains("Cost", ex.Message);
        Assert.Contains("decimal", ex.Message);
    }

    [Fact]
    public void ReadString_ReturnsNull_ForNullData()
    {
        var result = CostManagementService.ReadString(null, "Currency");
        Assert.Null(result);
    }

    [Fact]
    public void ReadString_ParsesQuotedString()
    {
        var data = BinaryData.FromString("\"EUR\"");
        var result = CostManagementService.ReadString(data, "Currency");
        Assert.Equal("EUR", result);
    }

    [Fact]
    public void ReadString_ConvertsNumberToString()
    {
        var data = BinaryData.FromString("20180331");
        var result = CostManagementService.ReadString(data, "UsageDate");
        Assert.Equal("20180331", result);
    }

    [Fact]
    public void ReadString_ReturnsNull_ForJsonNull()
    {
        var data = BinaryData.FromString("null");
        var result = CostManagementService.ReadString(data, "Currency");
        Assert.Null(result);
    }

    [Fact]
    public void FormatUsageDate_ReturnsNull_ForNullData()
    {
        var result = CostManagementService.FormatUsageDate(null);
        Assert.Null(result);
    }

    [Fact]
    public void FormatUsageDate_FormatsDailyAsIso8601()
    {
        var data = BinaryData.FromString("20260415");
        var result = CostManagementService.FormatUsageDate(data);
        Assert.Equal("2026-04-15", result);
    }

    [Fact]
    public void FormatUsageDate_FormatsMonthlyAsFirstOfMonth()
    {
        var data = BinaryData.FromString("202604");
        var result = CostManagementService.FormatUsageDate(data);
        Assert.Equal("2026-04-01", result);
    }

    [Fact]
    public void FormatUsageDate_PassesThroughUnexpectedLength()
    {
        var data = BinaryData.FromString("\"2026-04-15\"");
        var result = CostManagementService.FormatUsageDate(data);
        Assert.Equal("2026-04-15", result);
    }

    [Fact]
    public void KnownDimensions_ContainsTheEightDocumentedValues()
    {
        var expected = new[]
        {
            "ServiceName", "ResourceGroupName", "ResourceLocation", "ResourceId",
            "MeterCategory", "MeterSubCategory", "ChargeType", "BillingPeriod"
        };

        Assert.Equal(expected.Length, CostManagementService.KnownDimensions.Count);
        foreach (var dim in expected)
        {
            Assert.Contains(dim, CostManagementService.KnownDimensions);
        }
    }

    [Fact]
    public void KnownDimensions_IsCaseInsensitive()
    {
        Assert.Contains("ServiceName".ToLowerInvariant(), CostManagementService.KnownDimensions);
        Assert.Contains("ResourceGroupName".ToUpperInvariant(), CostManagementService.KnownDimensions);
    }
}
