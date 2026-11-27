// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore Fproviders Fservice

using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class MarkdownRenderingHelpersTests
{
    [Fact]
    public void BuildPortalResourceUrl_UsesResourceRouteForArmResource()
    {
        const string resourceId =
            "/subscriptions/sub 1/resourceGroups/rg(1)/providers/Microsoft.Web/sites/app";

        string url = MarkdownRenderingHelpers.BuildPortalResourceUrl(resourceId);

        Assert.Equal(
            "https://portal.azure.com/#@/resource/subscriptions/sub%201/resourceGroups/rg%281%29/" +
            "providers/Microsoft.Web/sites/app",
            url);
    }

    [Fact]
    public void BuildPortalResourceUrl_UsesServiceGroupMenuForNamedServiceGroup()
    {
        string url = MarkdownRenderingHelpers.BuildPortalResourceUrl(
            "/providers/Microsoft.Management/serviceGroups/Team-Prod_(West).1");

        Assert.Equal(
            "https://portal.azure.com/#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/overview/" +
            "serviceGroupId/Team-Prod_%28West%29.1",
            url);
    }

    [Fact]
    public void BuildPortalResourceUrl_UsesBrowseViewForGuidServiceGroup()
    {
        const string serviceGroupId = "00000000-0000-0000-0000-000000000001";

        string url = MarkdownRenderingHelpers.BuildPortalResourceUrl(
            $"/providers/Microsoft.Management/serviceGroups/{serviceGroupId}");

        Assert.Equal(
            "https://portal.azure.com/#view/Microsoft_Azure_Resources/ServiceGroupsBrowse.ReactView/" +
            "id/%2Fproviders%2FMicrosoft.Management%2FserviceGroups%2F00000000-0000-0000-0000-000000000001",
            url);
    }

    [Fact]
    public void EscapeLinkLabel_UsesSharedTableAndParenthesisEscaping()
    {
        Assert.Equal(
            "storage \\(one\\) \\[primary\\]\\|west",
            MarkdownRenderingHelpers.EscapeLinkLabel("storage (one) [primary]|west"));
    }
}
