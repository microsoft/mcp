// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Models;

public class VaultExpandParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrWhitespace_ReturnsNone(string? input)
    {
        Assert.Equal(VaultExpand.None, VaultExpandParser.Parse(input));
    }

    [Theory]
    [InlineData("security", VaultExpand.Security)]
    [InlineData("network", VaultExpand.Network)]
    [InlineData("monitoring", VaultExpand.Monitoring)]
    [InlineData("mua", VaultExpand.Mua)]
    [InlineData("all", VaultExpand.All)]
    public void Parse_SingleToken_ReturnsExpectedFlag(string input, VaultExpand expected)
    {
        Assert.Equal(expected, VaultExpandParser.Parse(input));
    }

    [Theory]
    [InlineData("SECURITY")]
    [InlineData("Security")]
    [InlineData(" security ")]
    public void Parse_IsCaseAndWhitespaceInsensitive(string input)
    {
        Assert.Equal(VaultExpand.Security, VaultExpandParser.Parse(input));
    }

    [Fact]
    public void Parse_MultipleTokens_CombinesFlags()
    {
        var result = VaultExpandParser.Parse("security,network,mua");
        Assert.Equal(VaultExpand.Security | VaultExpand.Network | VaultExpand.Mua, result);
    }

    [Fact]
    public void Parse_All_IsEquivalentToCombinedFlags()
    {
        Assert.Equal(
            VaultExpand.Security | VaultExpand.Network | VaultExpand.Monitoring | VaultExpand.Mua,
            VaultExpandParser.Parse("all"));
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("security,foobar")]
    [InlineData("mua,invalid,network")]
    public void Parse_UnknownToken_Throws(string input)
    {
        var ex = Assert.Throws<ArgumentException>(() => VaultExpandParser.Parse(input));
        Assert.Contains("--expand", ex.Message);
    }

    [Fact]
    public void Parse_DuplicateTokens_AreIdempotent()
    {
        Assert.Equal(VaultExpand.Security, VaultExpandParser.Parse("security,security"));
    }
}
