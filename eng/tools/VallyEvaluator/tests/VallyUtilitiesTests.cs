using Xunit;

namespace VallyEvaluator.Tests;

public class VallyUtilitiesTests
{
    [Fact]
    public void ReplaceAngleBracketPlaceholders_ReplacesKnownPlaceholder_FromReplacementsDictionary()
    {
        var input = "the <service-name> exists";

        var result = VallyUtilities.ReplaceAngleBracketPlaceholders(input, VallyUtilities.Replacements);

        Assert.Equal("the Azure Monitor exists", result);
    }
}
