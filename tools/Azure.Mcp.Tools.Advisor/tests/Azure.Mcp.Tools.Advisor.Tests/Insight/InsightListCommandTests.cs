// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Insight;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Insight;

public class InsightListCommandTests
    : SubscriptionCommandUnitTestsBase<InsightListCommand, IAdvisorInsightService>
{
    private void SetupEmptyResult() =>
        Service.ListInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<AdvisorInsight>([], false));

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("list", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
    }

    [Theory]
    [InlineData("--service-group commerce", true)]
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --insight-name ServiceRetirement --top 10", true)]
    [InlineData("--service-group commerce --insight-name IdleResources --tenant t1", true)]
    [InlineData("", true)]
    [InlineData("--insight-name IdleResources", true)]
    [InlineData("--subscription \" \"", false)]
    [InlineData("--service-group commerce --subscription sub1", false)]
    [InlineData("--service-group commerce --subscription \"\"", false)]
    [InlineData("--service-group \"\"", false)]
    [InlineData("--service-group \" \"", false)]
    [InlineData("--service-group /providers/microsoft.management/servicegroups/commerce", false)]
    [InlineData("--subscription sub1 --insight-name \" \"", false)]
    [InlineData("--subscription sub1 --insight-name \"x' | take 1\"", false)]
    [InlineData("--subscription sub1 --top 0", false)]
    [InlineData("--top 501", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceGroupScope_DoesNotResolveDefaultSubscription()
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync("--service-group", "commerce");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.Received(1).ListInsightsAsync(
            "commerce", null, null, 50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SubscriptionScope_ForwardsNormalizedOptions()
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--insight-name", " ServiceRetirement ",
            "--top", "7",
            "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListInsightsAsync(
            null, "sub1", "ServiceRetirement", 7, "tenant1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NoScope_QueriesAllServiceGroupsWithoutDefaultSubscription()
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync("--insight-name", "IdleResources");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.Received(1).ListInsightsAsync(
            null, null, "IdleResources", 50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MultipleTenants_SurfacesAmbiguityAsBadRequest()
    {
        Service.ListInsightsAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException(
                "Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify the tenant explicitly."));

        var response = await ExecuteCommandAsync("--service-group", "commerce");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specify the tenant explicitly", response.Message);
    }
}
