// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands.ServiceGroupIntelligence;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.ServiceGroupIntelligence;

public class ServiceGroupIntelligenceGetCommandTests
    : CommandUnitTestsBase<ServiceGroupIntelligenceGetCommand, IAdvisorServiceGroupIntelligenceService>
{
    private void SetupEmptyResult() =>
        Service.GetStatusAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<AdvisorServiceGroupStatus>([], false));

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("--service-group commerce", true)]
    [InlineData("--criticality MissionCritical --status \"Attention Needed\"", true)]
    [InlineData("--criticality missioncritical --status attentionneeded --top 10 --tenant t1", true)]
    [InlineData("--service-group \"\"", false)]
    [InlineData("--service-group /providers/microsoft.management/servicegroups/commerce", false)]
    [InlineData("--criticality High", false)]
    [InlineData("--status Healthy", false)]
    [InlineData("--status \"x' | take 1\"", false)]
    [InlineData("--top 0", false)]
    [InlineData("--top 501", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_NoOptions_QueriesAllServiceGroupsWithDefaults()
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetStatusAsync(
            null, null, null, 50, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AllOptions_ForwardsThemToService()
    {
        SetupEmptyResult();

        var response = await ExecuteCommandAsync(
            "--service-group", " commerce ",
            "--criticality", "MissionCritical",
            "--status", "At Risk",
            "--top", "7",
            "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetStatusAsync(
            "commerce", "MissionCritical", "At Risk", 7, "tenant1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_MultipleTenants_SurfacesAmbiguityAsBadRequest()
    {
        Service.GetStatusAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException(
                "Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify the tenant explicitly."));

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specify the tenant explicitly", response.Message);
    }
}
