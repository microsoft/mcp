// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public sealed class WorkspaceCreateCommandTests()
    : CommandUnitTestsBase<WorkspaceCreateCommand, IFabricCoreService>
{
    [Fact]
    public void Constructor_InitializesCommandAndMutationMetadata()
    {
        Assert.Equal("create-workspace", Command.Name);
        Assert.Equal("Create Fabric Workspace", Command.Title);
        Assert.NotEmpty(Command.Description);
        Assert.True(Guid.TryParse(Command.Id, out var id));
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(ToolOperationPlane.Control, Command.Metadata.OperationPlane);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.Secret);
        Assert.Same(CoreJsonContext.Default.WorkspaceCreateResult, Command.ResultTypeInfo);
    }

    [Fact]
    public void Constructor_RejectsMissingDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkspaceCreateCommand(null!, Service));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceCreateCommand(Logger, null!));
    }

    [Fact]
    public void BindOptions_UsesExactlyTheFourFabricCreationOptions()
    {
        var options = Command.BindOptions(CommandDefinition.Parse(
            ["--display-name", "New workspace", "--description", "A description",
             "--capacity-id", WorkspaceCreateTestData.CapacityId, "--domain-id", WorkspaceCreateTestData.DomainId]));

        Assert.Equal("New workspace", options.DisplayName);
        Assert.Equal("A description", options.Description);
        Assert.Equal(WorkspaceCreateTestData.CapacityId, options.CapacityId);
        Assert.Equal(WorkspaceCreateTestData.DomainId, options.DomainId);
        Assert.Equal(4, CommandDefinition.Options.Count);
        Assert.Equal("--display-name", Assert.Single(CommandDefinition.Options, static option => option.Required).Name);
    }

    public static TheoryData<string[], string> InvalidArguments
    {
        get
        {
            var data = new TheoryData<string[], string>
            {
                { [], "--display-name" },
                { ["--display-name", ""], "--display-name" },
                { ["--display-name", " \t "], "--display-name" },
                { ["--display-name", new string('a', 257)], "--display-name" },
                { ["--display-name", "Admin monitoring"], "--display-name" },
                { ["--display-name", "ADMIN MONITORING"], "--display-name" },
                { ["--display-name", " Admin monitoring "], "--display-name" },
                { ["--display-name", "New workspace", "--description", new string('a', 4001)], "--description" }
            };
            foreach (var option in new[] { "--capacity-id", "--domain-id" })
            {
                foreach (var value in new[] { "", " ", "not-a-uuid", "00000000-0000-0000-0000-000000000000", "https://example.com" })
                {
                    data.Add(["--display-name", "New workspace", option, value], option);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public async Task ExecuteAsync_RejectsInvalidInputBeforeServiceCalls(string[] arguments, string option)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(option, response.Message);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(1, -1)]
    [InlineData(256, 0)]
    [InlineData(256, 4000)]
    public async Task ExecuteAsync_AcceptsExactLengthBoundaries(int nameLength, int descriptionLength)
    {
        var name = new string('n', nameLength);
        var description = descriptionLength < 0 ? null : new string('d', descriptionLength);
        var arguments = new List<string> { "--display-name", name };
        if (description is not null)
        {
            arguments.AddRange(["--description", description]);
        }

        Service.CreateWorkspaceAsync(Arg.Any<CreateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .Returns(WorkspaceCreateTestData.CreateResult());

        var response = await ExecuteCommandAsync(arguments.ToArray());

        Assert.Equal(HttpStatusCode.Created, response.Status);
        await Service.Received(1).CreateWorkspaceAsync(
            Arg.Is<CreateWorkspaceRequest>(request =>
                request.DisplayName == name && request.Description == description &&
                request.CapacityId == null && request.DomainId == null),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsOptionalAssignmentsAndReturnsTypedCreationResult()
    {
        Service.CreateWorkspaceAsync(Arg.Any<CreateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .Returns(WorkspaceCreateTestData.CreateResult());

        var response = await ExecuteCommandAsync(
            "--display-name", "New workspace", "--description", "",
            "--capacity-id", WorkspaceCreateTestData.CapacityId, "--domain-id", WorkspaceCreateTestData.DomainId);

        var result = ValidateAndDeserializeResponse(response, CoreJsonContext.Default.WorkspaceCreateResult, HttpStatusCode.Created);
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.WorkspaceId), result.Workspace.Id);
        Assert.Equal("New workspace", result.Workspace.DisplayName);
        Assert.Equal(WorkspaceCreateTestData.Location, result.Location);
        await Service.Received(1).CreateWorkspaceAsync(
            Arg.Is<CreateWorkspaceRequest>(request =>
                request.DisplayName == "New workspace" && request.Description == "" &&
                request.CapacityId == WorkspaceCreateTestData.CapacityId && request.DomainId == WorkspaceCreateTestData.DomainId),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "name is unused")]
    [InlineData(HttpStatusCode.Unauthorized, "Workspace.ReadWrite.All")]
    [InlineData(HttpStatusCode.Forbidden, "workspace-creation permission")]
    [InlineData(HttpStatusCode.NotFound, "capacity/domain IDs")]
    [InlineData(HttpStatusCode.Conflict, "name is already in use")]
    [InlineData(HttpStatusCode.TooManyRequests, "not automatically retried")]
    [InlineData(HttpStatusCode.InternalServerError, "may already have been created")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "may already have been created")]
    public async Task ExecuteAsync_PreservesHttpFailureStatusWithoutLeakingDetails(HttpStatusCode status, string guidance)
    {
        Service.CreateWorkspaceAsync(Arg.Any<CreateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException(WorkspaceCreateTestData.SecretMarker, null, status));

        var response = await ExecuteCommandAsync("--display-name", "Private display name");

        Assert.Equal(status, response.Status);
        Assert.Contains(guidance, response.Message);
        AssertSanitized(response);
        await Service.Received(1).CreateWorkspaceAsync(Arg.Any<CreateWorkspaceRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("task-cancellation", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task ExecuteAsync_SanitizesAllFailurePaths(string failure, HttpStatusCode status)
    {
        var detail = WorkspaceCreateTestData.SecretMarker;
        Exception exception = failure switch
        {
            "authentication" => new AuthenticationFailedException(detail),
            "credentials" => new CredentialUnavailableException(detail),
            "network" => new HttpRequestException(detail),
            "json" => new JsonException(detail),
            "cancellation" => new OperationCanceledException(detail),
            "task-cancellation" => new TaskCanceledException(detail),
            "timeout" => new TimeoutException(detail),
            _ => new Exception(detail)
        };
        Service.CreateWorkspaceAsync(Arg.Any<CreateWorkspaceRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync("--display-name", "Private display name");

        Assert.Equal(status, response.Status);
        AssertSanitized(response);
        if (status != HttpStatusCode.Unauthorized)
        {
            Assert.Contains("may already have been created", response.Message);
        }
    }

    private void AssertSanitized(CommandResponse response)
    {
        Assert.Null(response.Results);
        var json = JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse);
        Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, json);
        Assert.DoesNotContain("Private display name", json);
        Assert.DoesNotContain("StackTrace", json);
        var logs = Logger.ReceivedCalls().Where(static call => call.GetMethodInfo().Name == "Log");
        Assert.NotEmpty(logs);
        foreach (var log in logs)
        {
            Assert.Null(log.GetArguments()[3]);
            var text = log.GetArguments()[2]?.ToString() ?? "";
            Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, text);
            Assert.DoesNotContain("Private display name", text);
        }
    }
}
