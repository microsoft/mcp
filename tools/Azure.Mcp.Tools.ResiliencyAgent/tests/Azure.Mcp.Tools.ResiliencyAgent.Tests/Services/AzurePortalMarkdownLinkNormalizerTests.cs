// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class AzurePortalMarkdownLinkNormalizerTests
{
    [Theory]
    [InlineData("#view/Microsoft_Azure_BCDRCenter/AbcCenterMenuBlade/~/overview")]
    [InlineData("#blade/Microsoft_Azure_DataProtection/JobDetailsBlade/jobId/job-1")]
    [InlineData("#resource/subscriptions/sub/resourceGroups/rg")]
    [InlineData("#@/resource/subscriptions/sub/resourceGroups/rg")]
    [InlineData("#@contoso.onmicrosoft.com/resource/subscriptions/sub/resourceGroups/rg")]
    public void Normalize_MakesKnownPortalFragmentsAbsolute(string destination)
    {
        string markdown = $"Open [the resource]({destination}).";

        string? normalized = AzurePortalMarkdownLinkNormalizer.Normalize(markdown);

        Assert.Equal(
            $"Open [the resource](https://portal.azure.com/{destination}).",
            normalized);
    }

    [Theory]
    [InlineData("https://portal.azure.com/#view/Microsoft_Azure_Test/Blade")]
    [InlineData("https://ms.portal.azure.com/#view/Microsoft_Azure_Test/Blade")]
    [InlineData("https://portal.azure.us/#view/Microsoft_Azure_Test/Blade")]
    [InlineData("https://portal.azure.cn/#view/Microsoft_Azure_Test/Blade")]
    [InlineData("https://learn.microsoft.com/azure/backup")]
    [InlineData("#configuration")]
    [InlineData("#@/resourceful/example")]
    [InlineData("#@javascript:/resource/subscriptions/sub")]
    [InlineData("#viewpoint/Microsoft_Azure_Test/Blade")]
    public void Normalize_LeavesNonRelativePortalDestinationsUnchanged(string destination)
    {
        string markdown = $"Open [the resource]({destination}).";

        Assert.Equal(markdown, AzurePortalMarkdownLinkNormalizer.Normalize(markdown));
    }

    [Fact]
    public void Normalize_LeavesImagesInlineCodeAndFencedCodeUnchanged()
    {
        const string Markdown =
            "Open [Backup](#view/Microsoft_Azure_Test/Overview).\r\n" +
            "![diagram](#view/Microsoft_Azure_Test/Image)\r\n" +
            "`[inline](#view/Microsoft_Azure_Test/Inline)`\r\n" +
            "```markdown\r\n" +
            "[fenced](#view/Microsoft_Azure_Test/Fenced)\r\n" +
            "```\r\n" +
            "> ~~~markdown\r\n" +
            "> [quoted](#view/Microsoft_Azure_Test/Quoted)\r\n" +
            "> ~~~\r\n";
        const string Expected =
            "Open [Backup](https://portal.azure.com/#view/Microsoft_Azure_Test/Overview).\r\n" +
            "![diagram](#view/Microsoft_Azure_Test/Image)\r\n" +
            "`[inline](#view/Microsoft_Azure_Test/Inline)`\r\n" +
            "```markdown\r\n" +
            "[fenced](#view/Microsoft_Azure_Test/Fenced)\r\n" +
            "```\r\n" +
            "> ~~~markdown\r\n" +
            "> [quoted](#view/Microsoft_Azure_Test/Quoted)\r\n" +
            "> ~~~\r\n";

        Assert.Equal(Expected, AzurePortalMarkdownLinkNormalizer.Normalize(Markdown));
    }

    [Fact]
    public void Normalize_PreservesTablesEscapedLabelsAnglesAndLineEndings()
    {
        const string Markdown =
            "| Resource | Link |\n" +
            "|---|---|\n" +
            "| events | [events\\[primary\\]](<#@/resource/subscriptions/sub/resourceGroups/rg/providers/Microsoft.EventHub/namespaces/events%5Bprimary%5D>) |\n";
        const string Expected =
            "| Resource | Link |\n" +
            "|---|---|\n" +
            "| events | [events\\[primary\\]](<https://portal.azure.com/#@/resource/subscriptions/sub/resourceGroups/rg/providers/Microsoft.EventHub/namespaces/events%5Bprimary%5D>) |\n";

        Assert.Equal(Expected, AzurePortalMarkdownLinkNormalizer.Normalize(Markdown));
    }

    [Fact]
    public void Normalize_LeavesContentWithoutPortalLinksByteForByteEquivalent()
    {
        const string Markdown =
            "No Portal links here.\r\n" +
            "\r\n" +
            "- [Documentation](https://learn.microsoft.com/azure/backup)\r\n";

        Assert.Equal(Markdown, AzurePortalMarkdownLinkNormalizer.Normalize(Markdown));
    }
}
