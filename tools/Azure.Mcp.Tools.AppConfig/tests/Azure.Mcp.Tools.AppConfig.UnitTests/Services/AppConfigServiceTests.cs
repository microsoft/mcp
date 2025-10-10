// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Tools.AppConfig.Services;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.UnitTests.Services;

[Trait("Area", "AppConfig")]
public class AppConfigServiceTests
{
    /// <summary>
    /// Regression guard: ensures ExecuteSingleResourceQueryAsync continues to expose the optional
    /// additionalFilter parameter and that AppConfigService calls it using a named argument.
    /// If the base signature changes or the service reverts to positional arguments, this test fails.
    /// </summary>
    [Fact]
    public void FindAppConfigStore_InvokesExecuteSingleResourceQueryAsyncWithNamedAdditionalFilter()
    {
        var baseType = typeof(AppConfigService).BaseType;
        Assert.NotNull(baseType);

        var executeMethod = baseType!.GetMethod(
            "ExecuteSingleResourceQueryAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(executeMethod);

        var parameters = executeMethod!.GetParameters();
        Assert.True(parameters.Length >= 7, "ExecuteSingleResourceQueryAsync should expose the additionalFilter parameter");
        Assert.Equal("additionalFilter", parameters[6].Name);

        var methodSource = ReadFindAppConfigStoreSource();
        Assert.Contains("ExecuteSingleResourceQueryAsync(", methodSource);
        Assert.Contains("additionalFilter:", methodSource);
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
