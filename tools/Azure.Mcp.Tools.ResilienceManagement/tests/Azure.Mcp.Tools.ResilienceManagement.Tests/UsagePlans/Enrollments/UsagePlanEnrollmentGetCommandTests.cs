// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.UsagePlans.Enrollments;

public class UsagePlanEnrollmentGetCommandTests : SubscriptionCommandUnitTestsBase<UsagePlanEnrollmentGetCommand, IResilienceManagementService>
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string ResourceGroup = "rg1";
    private const string UsagePlan = "plan1";

    [Fact]
    public async Task ExecuteAsync_ListsEnrollments_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "enrollment1"), new("id2", "enrollment2") };
        Service.ListUsagePlanEnrollmentsAsync(ResourceGroup, UsagePlan, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--resource-group", ResourceGroup, "--usage-plan", UsagePlan);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanEnrollmentGetCommandResult);
        Assert.NotNull(result.Enrollments);
        Assert.Equal(2, result.Enrollments!.Count);
        Assert.Null(result.Enrollment);
    }

    [Fact]
    public async Task ExecuteAsync_GetsEnrollment_WhenNameProvided()
    {
        var expected = new UsagePlanEnrollmentInfo("id1", "enrollment1");
        Service.GetUsagePlanEnrollmentAsync(ResourceGroup, UsagePlan, "enrollment1", SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--resource-group", ResourceGroup, "--usage-plan", UsagePlan, "--name", "enrollment1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.UsagePlanEnrollmentGetCommandResult);
        Assert.NotNull(result.Enrollment);
        Assert.Equal("enrollment1", result.Enrollment!.Name);
        Assert.Null(result.Enrollments);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListUsagePlanEnrollmentsAsync(ResourceGroup, UsagePlan, SubscriptionId, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", SubscriptionId, "--resource-group", ResourceGroup, "--usage-plan", UsagePlan);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
