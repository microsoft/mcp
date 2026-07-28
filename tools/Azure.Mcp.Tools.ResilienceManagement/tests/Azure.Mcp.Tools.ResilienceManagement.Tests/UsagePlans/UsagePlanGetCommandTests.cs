// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans;

public class UsagePlanGetCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanGetCommand, IResilienceManagementService>
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string ResourceGroup = "rg1";

    [Fact]
    public async Task ExecuteAsync_ListsUsagePlansBySubscription_WhenNoResourceGroupOrName()
    {
        var expected = new List<ResourceSummary> { new("id1", "plan1"), new("id2", "plan2") };
        Service.ListUsagePlansBySubscriptionAsync(SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanGetCommandResult);
        Assert.NotNull(result.UsagePlans);
        Assert.Equal(2, result.UsagePlans!.Count);
        Assert.Null(result.UsagePlan);
    }

    [Fact]
    public async Task ExecuteAsync_ListsUsagePlansByResourceGroup_WhenResourceGroupOnly()
    {
        var expected = new List<ResourceSummary> { new("id1", "plan1") };
        Service.ListUsagePlansAsync(ResourceGroup, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--resource-group", ResourceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanGetCommandResult);
        Assert.NotNull(result.UsagePlans);
        Assert.Single(result.UsagePlans!);
    }

    [Fact]
    public async Task ExecuteAsync_GetsUsagePlan_WhenNameAndResourceGroupProvided()
    {
        var expected = new UsagePlanInfo("id1", "plan1", "Microsoft.AzureResilience/usagePlans", "westus");
        Service.GetUsagePlanAsync(ResourceGroup, "plan1", SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--resource-group", ResourceGroup, "--name", "plan1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanGetCommandResult);
        Assert.NotNull(result.UsagePlan);
        Assert.Equal("plan1", result.UsagePlan!.Name);
        Assert.Null(result.UsagePlans);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenNameProvidedWithoutResourceGroup()
    {
        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--name", "plan1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListUsagePlansBySubscriptionAsync(SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
