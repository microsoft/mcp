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
    [InlineData("azuremigrate_platformlandingzone_request")]
    [InlineData("speech_stt_recognize")]
    [InlineData("speech_tts_synthesize")]
    [InlineData("resiliencyagent_guidance_get")]
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

    [Theory]
    [InlineData("adme_schema_list")]
    [InlineData("adme_schema_get")]
    [InlineData("adme_search")]
    [InlineData("adme_storage_record_fetch")]
    [InlineData("adme_storage_record_get")]
    [InlineData("adme_storage_record_list")]
    [InlineData("adme_storage_record_version_list")]
    public async Task AdmeTool_IsDiscoverableOverHttp(string toolName)
    {
        var tools = await fixture.Client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains(tools, tool => tool.Name == toolName);
    }
}
