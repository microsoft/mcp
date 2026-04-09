// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Mcp.Core.Services.Pagination;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Pagination;

public class TableAppResourceTests
{
    [Fact]
    public void TableAppHtml_IsEmbeddedInAssembly()
    {
        var assembly = typeof(TableAppResource).Assembly;
        using var stream = assembly.GetManifestResourceStream("TableApp.html");

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0, "TableApp.html embedded resource should not be empty.");
    }

    [Fact]
    public void TableAppHtml_ContainsValidHtmlContent()
    {
        var assembly = typeof(TableAppResource).Assembly;
        using var stream = assembly.GetManifestResourceStream("TableApp.html")!;
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        Assert.StartsWith("<!DOCTYPE html>", content, StringComparison.OrdinalIgnoreCase);
    }
}
