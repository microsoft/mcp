// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace StorageMcp.Generator.Tests;

public sealed class OptionsSyntaxAdapterTests
{
    private const string Source = """
        #nullable disable
        public partial class Options
        {
            public string Account { get; set; }
            public string Container { get; set; }
            public string Tenant { get; set; }
        }
        """;

    [Fact]
    public void RequirednessAndNullableContextArePreserved()
    {
        var result = OptionsSyntaxAdapter.RewriteSource(Source);
        Assert.Contains("#nullable enable", result);
        Assert.Contains("public required string Account", result);
        Assert.Contains("public required string Container", result);
        Assert.Contains("public string? Tenant", result);
        Assert.Equal(result, OptionsSyntaxAdapter.RewriteSource(result));
    }

    [Fact]
    public void UnexpectedOrInvalidSyntaxFailsClosed()
    {
        Assert.Throws<InvalidOperationException>(() => OptionsSyntaxAdapter.RewriteSource(Source.Replace("Tenant", "Subscription", StringComparison.Ordinal)));
        Assert.Throws<InvalidOperationException>(() => OptionsSyntaxAdapter.RewriteSource("class {"));
    }

    [Fact]
    public void DirectiveTextInsideDescriptionsIsNotRewritten()
    {
        var source = Source.Replace("public string Tenant", """
            [System.ComponentModel.Description("#nullable disable")]
            public string Tenant
            """, StringComparison.Ordinal);
        var result = OptionsSyntaxAdapter.RewriteSource(source);
        Assert.Contains("#nullable enable", result);
        Assert.Contains("""Description("#nullable disable")""", result);
    }
}
