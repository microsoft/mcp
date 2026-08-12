// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace VallyEvaluator.Tests;

public class VallyUtilitiesTests
{
    [Fact]
    public void ResolvePromptNamespace_MapsResilienceManagementPackageToResilienceNamespace()
    {
        IReadOnlySet<string> promptNamespaces = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "resilience"
        };

        var result = Program.ResolvePromptNamespace("resiliencemanagement", promptNamespaces);

        Assert.Equal("resilience", result);
    }

    [Fact]
    public void ReplaceAngleBracketPlaceholders_ReplacesKnownPlaceholder_FromReplacementsDictionary()
    {
        var input = "the <service-name> exists";

        var result = VallyUtilities.ReplaceAngleBracketPlaceholders(input, VallyUtilities.Replacements);

        Assert.Equal("the Azure Monitor exists", result);
    }
}
