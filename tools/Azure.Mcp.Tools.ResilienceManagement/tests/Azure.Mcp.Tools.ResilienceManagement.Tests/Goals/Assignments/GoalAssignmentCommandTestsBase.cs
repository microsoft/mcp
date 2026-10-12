// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore fchild

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public abstract class GoalAssignmentCommandTestsBase<TCommand, TResult> : CommandUnitTestsBase<TCommand, IResilienceManagementService>
    where TCommand : class, IBaseCommand
{
    protected const string ResourceId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1";
    protected const string ServiceLevelResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/slis/sli1";
    protected const string OperationId = "22222222-2222-2222-2222-222222222222";
    protected const string Tenant = "33333333-3333-3333-3333-333333333333";
    protected const string Payload = """[{"properties":{"resourceArmId":"/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1","zonalResiliency":{"goalParticipation":"Excluded"}}}]""";
    protected abstract string CommandName { get; }
    protected abstract TResult ExpectedResult { get; }
    protected abstract Task<TResult> ConfigureServiceCall();
    protected virtual string[] AdditionalArguments => [];
    protected virtual bool Destructive => true;
    protected virtual bool Idempotent => true;
    protected string[] Arguments => ["--service-group", "sg1", "--goal-assignment", "assignment1", "--tenant", Tenant, .. AdditionalArguments];

    [Fact]
    public void Constructor_DeclaresTenantScopedControlPlaneMetadata()
    {
        Assert.Equal(CommandName, CommandDefinition.Name);
        Assert.False(string.IsNullOrWhiteSpace(Command.Description));
        Assert.False(string.IsNullOrWhiteSpace(Command.Title));
        Assert.True(Guid.TryParse(Command.Id, out _));
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.Equal(Destructive, Command.Metadata.Destructive);
        Assert.Equal(Idempotent, Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.Contains(CommandDefinition.Options, option => option.Name == "--tenant");
        Assert.DoesNotContain(CommandDefinition.Options, option => option.Name == "--subscription");
    }

    [Theory]
    [InlineData("--service-group")]
    [InlineData("--goal-assignment")]
    public async Task ExecuteAsync_RejectsMissingRequiredFieldWithoutCallingService(string option)
    {
        var arguments = Arguments.ToList();
        int index = arguments.IndexOf(option);
        arguments.RemoveRange(index, 2);

        var response = await ExecuteCommandAsync(arguments.ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--service-group", "parent/child")]
    [InlineData("--service-group", " ")]
    [InlineData("--goal-assignment", " ")]
    [InlineData("--goal-assignment", ".")]
    [InlineData("--goal-assignment", "..")]
    [InlineData("--goal-assignment", "parent/child")]
    [InlineData("--goal-assignment", "parent\\child")]
    [InlineData("--goal-assignment", "name?query")]
    [InlineData("--goal-assignment", "name#fragment")]
    [InlineData("--goal-assignment", "name%2fchild")]
    [InlineData("--goal-assignment", "name\nchild")]
    [InlineData("--goal-assignment", "ab")]
    [InlineData("--goal-assignment", "1234567890123456789012345")]
    [InlineData("--goal-assignment", "goal_assignment")]
    [InlineData("--goal-assignment", "goal.assignment")]
    [InlineData("--goal-assignment", "goal(assignment)")]
    [InlineData("--goal-assignment", "goal assignment")]
    [InlineData("--goal-assignment", "goal\u00e9")]
    public async Task ExecuteAsync_RejectsInvalidNameWithoutCallingService(string option, string value)
    {
        string[] arguments = Arguments;
        arguments[Array.IndexOf(arguments, option) + 1] = value;

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("A-1")]
    [InlineData("123456789012345678901234")]
    public async Task ExecuteAsync_AcceptsAssignmentNameBoundaries(string name)
    {
        ConfigureServiceCall().Returns(ExpectedResult);
        string[] arguments = Arguments;
        arguments[Array.IndexOf(arguments, "--goal-assignment") + 1] = name;

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(name, Assert.Single(Service.ReceivedCalls()).GetArguments()[1]);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsOverlongServiceGroupWithoutCallingService()
    {
        string[] arguments = Arguments;
        arguments[1] = new string('a', 91);

        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_BindsExactTenantArgumentsAndCancellation()
    {
        ConfigureServiceCall().Returns(ExpectedResult);
        using var cancellation = new CancellationTokenSource();

        var response = await Command.ExecuteAsync(Context, CommandDefinition.Parse(Arguments), cancellation.Token);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        object?[] arguments = Assert.Single(Service.ReceivedCalls()).GetArguments();
        Assert.Equal("sg1", arguments[0]);
        Assert.Equal("assignment1", arguments[1]);
        Assert.Equal(Tenant, arguments[^2]);
        Assert.Equal(cancellation.Token, arguments[^1]);
        AssertPayloadArguments(arguments);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Results));
        AssertSerializedResult(document.RootElement);
    }

    [Theory]
    [InlineData(403, "Authorization failed")]
    [InlineData(404, "not found")]
    [InlineData(409, "conflicts")]
    [InlineData(400, "request failed")]
    [InlineData(422, "operation failed")]
    [InlineData(500, "operation failed")]
    public async Task ExecuteAsync_SanitizesErrorsButLogsOriginalException(int status, string expectedMessage)
    {
        const string secret = "RAW_PROVIDER_SECRET";
        Exception exception = status switch
        {
            422 => new InvalidOperationException(secret + Payload),
            500 => new Exception(secret + Payload),
            _ => new RequestFailedException(status, secret + Payload)
        };
        ConfigureServiceCall().ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(Arguments);

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(secret, response.Message);
        Assert.Null(response.Results);
        var log = Assert.Single(Logger.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(ILogger.Log));
        Assert.Equal(LogLevel.Error, log.GetArguments()[0]);
        Assert.Same(exception, log.GetArguments()[3]);
        string state = log.GetArguments()[2]!.ToString()!;
        Assert.DoesNotContain(secret, state);
        Assert.DoesNotContain(Payload, state);
        Assert.DoesNotContain(ResourceId, state);
        Assert.DoesNotContain(ServiceLevelResourceId, state);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationAndReportsUnknownCompletion()
    {
        using var cancellation = new CancellationTokenSource();
        var exception = new OperationCanceledException("private cancellation details", cancellation.Token);
        ConfigureServiceCall().ThrowsAsync(exception);

        var response = await Command.ExecuteAsync(Context, CommandDefinition.Parse(Arguments), cancellation.Token);

        Assert.Equal(cancellation.Token, Assert.Single(Service.ReceivedCalls()).GetArguments()[^1]);
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("Completion is unknown", response.Message);
        Assert.DoesNotContain("private cancellation details", response.Message);
        Assert.Null(response.Results);
    }

    protected virtual void AssertPayloadArguments(object?[] arguments) { }

    protected virtual void AssertSerializedResult(JsonElement result)
    {
        JsonElement operation = result.GetProperty("operation");
        Assert.Equal(OperationId, operation.GetProperty("operationId").GetString());
        Assert.Equal("sg1", operation.GetProperty("serviceGroup").GetString());
        Assert.Equal("assignment1", operation.GetProperty("goalAssignment").GetString());
        Assert.Equal("Accepted", operation.GetProperty("status").GetString());
        Assert.False(operation.GetProperty("hasCompleted").GetBoolean());
    }
}
