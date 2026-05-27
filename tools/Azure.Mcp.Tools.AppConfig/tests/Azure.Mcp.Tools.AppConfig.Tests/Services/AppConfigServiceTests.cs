// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Tools.AppConfig.Services;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests.Services;

public class AppConfigServiceTests
{
    /// <summary>
    /// Regression guard (AC-01): ensures FindAppConfigStore does not have a redundant
    /// subscriptionIdentifier parameter that duplicates subscription.
    /// </summary>
    [Fact]
    public void FindAppConfigStore_DoesNotHaveRedundantSubscriptionIdentifierParameter()
    {
        var method = typeof(AppConfigService).GetMethod(
            "FindAppConfigStore",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var paramNames = method!.GetParameters().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("subscriptionIdentifier", paramNames);
    }

    private static string ReadFindAppConfigStoreSource()
    {
        var sourceFile = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Services", "AppConfigService.cs");

        if (!File.Exists(sourceFile))
        {
            return string.Empty;
        }

        var source = File.ReadAllText(sourceFile);
        var marker = "private async Task<AppConfigurationAccount> FindAppConfigStore";
        var startIndex = source.IndexOf(marker, StringComparison.Ordinal);
        if (startIndex < 0)
        {
            return source;
        }

        var snippet = source[startIndex..];
        var nextMethodIndex = snippet.IndexOf("private", 1, StringComparison.Ordinal);
        if (nextMethodIndex > 0)
        {
            snippet = snippet[..nextMethodIndex];
        }

        return snippet;
    }
}
