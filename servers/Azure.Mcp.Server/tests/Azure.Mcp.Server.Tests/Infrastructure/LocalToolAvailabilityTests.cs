// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public sealed class LocalToolAvailabilityTests(LocalToolAvailabilityFixture fixture)
    : IClassFixture<LocalToolAvailabilityFixture>
{
    [Theory]
    [InlineData("storage_blob_upload")]
    [InlineData("keyvault_certificate_import")]
    public async Task LocalOnlyTool_IsNotListedOrCallableOverHttp(string toolName)
    {
        var tools = await fixture.Client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.DoesNotContain(tools, tool => tool.Name == toolName);

        var result = await fixture.Client.CallToolAsync(
            toolName, new Dictionary<string, object?>(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("not found",
            string.Join(Environment.NewLine, result.Content.OfType<TextContentBlock>().Select(block => block.Text)),
            StringComparison.OrdinalIgnoreCase);
    }
}
