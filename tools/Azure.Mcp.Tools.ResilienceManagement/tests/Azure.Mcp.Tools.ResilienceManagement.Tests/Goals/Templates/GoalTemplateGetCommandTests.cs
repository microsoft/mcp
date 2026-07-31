// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Templates;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Templates;

public class GoalTemplateGetCommandTests : CommandUnitTestsBase<GoalTemplateGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";

    [Fact]
    public async Task ExecuteAsync_ListsGoalTemplates_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "template1"), new("id2", "template2") };
        Service.ListGoalTemplatesAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalTemplateGetCommandResult);
        Assert.NotNull(result.GoalTemplates);
        Assert.Equal(2, result.GoalTemplates!.Count);
        Assert.Null(result.GoalTemplate);
    }

    [Fact]
    public async Task ExecuteAsync_GetsGoalTemplate_WhenNameProvided()
    {
        var expected = new GoalTemplateInfo("id1", "template1");
        Service.GetGoalTemplateAsync(ServiceGroup, "template1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--name", "template1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalTemplateGetCommandResult);
        Assert.NotNull(result.GoalTemplate);
        Assert.Equal("template1", result.GoalTemplate!.Name);
        Assert.Null(result.GoalTemplates);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListGoalTemplatesAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
