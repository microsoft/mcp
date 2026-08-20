// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Tests validating telemetry is emitted correctly for tool loading operations.
/// </summary>
public class ToolLoaderTelemetryTests : IDisposable
{
    private readonly Activity _activity;

    public ToolLoaderTelemetryTests()
    {
        _activity = new Activity("test-activity");
        _activity.Start();
    }

    public void Dispose()
    {
        _activity.Stop();
        _activity.Dispose();
    }

    [Fact]
    public async Task CommandFactoryToolLoader_EmitsErrorTelemetry_IfToolIsFiltered()
    {
        var toolName = "tool";
        var mcpServer = Substitute.For<McpServer>();
        var commandFactory = Substitute.For<ICommandFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Tool = ["nevercalled"] });
        var logger = Substitute.For<ILogger<CommandFactoryToolLoader>>();

        var mcpRuntime = CreateRuntime(new CommandFactoryToolLoader(commandFactory, options, logger));
        var request = CreateToolCallRequest(mcpServer, toolName);

        await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(ActivityStatusCode.Error, _activity.Status);
        Assert.Equal(false, GetAndAssertTagKeyValue(_activity, TagName.IsServerCommandInvoked));
        Assert.Equal(toolName, GetAndAssertTagKeyValue(_activity, TagName.ToolName));
        AssertTagDoesNotExist(_activity, TagName.ToolArea);
        AssertTagDoesNotExist(_activity, TagName.ToolId);
    }

    [Fact]
    public async Task CommandFactoryToolLoader_EmitsErrorTelemetry_IfCommandDoesNotExist()
    {
        var toolName = "tool";
        var mcpServer = Substitute.For<McpServer>();
        var commandFactory = Substitute.For<ICommandFactory>();
        commandFactory.AllCommands.Returns(new Dictionary<string, IBaseCommand>
        {
            ["nevercalled"] = Substitute.For<IBaseCommand>()
        });
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        var logger = Substitute.For<ILogger<CommandFactoryToolLoader>>();

        var mcpRuntime = CreateRuntime(new CommandFactoryToolLoader(commandFactory, options, logger));
        var request = CreateToolCallRequest(mcpServer, toolName);

        await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(ActivityStatusCode.Error, _activity.Status);
        Assert.Equal(false, GetAndAssertTagKeyValue(_activity, TagName.IsServerCommandInvoked));
        Assert.Equal(toolName, GetAndAssertTagKeyValue(_activity, TagName.ToolName));
        AssertTagDoesNotExist(_activity, TagName.ToolArea);
        AssertTagDoesNotExist(_activity, TagName.ToolId);
    }

    [Fact]
    public async Task CommandFactoryToolLoader_EmitsErrorTelemetry_IfClientDoesNotSupportElicitation()
    {
        var toolName = "tool";
        var toolId = Guid.NewGuid().ToString();
        var mcpServer = Substitute.For<McpServer>();
        var clientCapabilities = new ClientCapabilities();
        mcpServer.ClientCapabilities.Returns(clientCapabilities);
        var commandFactory = Substitute.For<ICommandFactory>();
        var toolCommand = Substitute.For<IBaseCommand>();
        toolCommand.Id.Returns(toolId);
        toolCommand.Metadata.Returns(new ToolMetadata { Secret = true });
        commandFactory.AllCommands.Returns(new Dictionary<string, IBaseCommand>
        {
            [toolName] = toolCommand
        });
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        var logger = Substitute.For<ILogger<CommandFactoryToolLoader>>();

        var mcpRuntime = CreateRuntime(new CommandFactoryToolLoader(commandFactory, options, logger));
        var request = CreateToolCallRequest(mcpServer, toolName);

        await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(ActivityStatusCode.Error, _activity.Status);
        Assert.Equal(false, GetAndAssertTagKeyValue(_activity, TagName.IsServerCommandInvoked));
        Assert.Equal(toolName, GetAndAssertTagKeyValue(_activity, TagName.ToolName));
        Assert.Equal(toolId, GetAndAssertTagKeyValue(_activity, TagName.ToolId));
        AssertTagDoesNotExist(_activity, TagName.ToolArea);
    }

    [Fact]
    public async Task CommandFactoryToolLoader_EmitsErrorTelemetry_IfToolHasAnExceptionWhenRunning()
    {
        var toolName = "tool";
        var toolArea = "area";
        var toolId = Guid.NewGuid().ToString();
        var mcpServer = Substitute.For<McpServer>();
        var commandFactory = Substitute.For<ICommandFactory>();
        commandFactory.GetServiceArea(Arg.Is(toolName)).Returns(toolArea);
        var toolCommand = Substitute.For<IBaseCommand>();
        toolCommand.Metadata.Returns(new ToolMetadata { Destructive = false });
        toolCommand.Id.Returns(toolId);
        toolCommand.GetCommand().Returns(new Command("tool-command"));
        toolCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Something went wrong"));
        commandFactory.AllCommands.Returns(new Dictionary<string, IBaseCommand>
        {
            [toolName] = toolCommand
        });
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        var logger = Substitute.For<ILogger<CommandFactoryToolLoader>>();

        var mcpRuntime = CreateRuntime(new CommandFactoryToolLoader(commandFactory, options, logger));
        var request = CreateToolCallRequest(mcpServer, toolName);

        await Assert.ThrowsAsync<Exception>(() => mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(ActivityStatusCode.Error, _activity.Status);
        Assert.Equal(true, GetAndAssertTagKeyValue(_activity, TagName.IsServerCommandInvoked));
        Assert.Equal(toolName, GetAndAssertTagKeyValue(_activity, TagName.ToolName));
        Assert.Equal(toolId, GetAndAssertTagKeyValue(_activity, TagName.ToolId));
        Assert.Equal(toolArea, GetAndAssertTagKeyValue(_activity, TagName.ToolArea));
    }

    [Fact]
    public async Task CommandFactoryToolLoader_EmitsSuccessTelemetry_WhenToolCallSucceeds()
    {
        var toolName = "tool";
        var toolArea = "area";
        var toolId = Guid.NewGuid().ToString();
        var mcpServer = Substitute.For<McpServer>();
        var commandFactory = Substitute.For<ICommandFactory>();
        commandFactory.GetServiceArea(Arg.Is(toolName)).Returns(toolArea);

        var toolCommand = Substitute.For<IBaseCommand>();
        toolCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CommandResponse()));
        toolCommand.Id.Returns(toolId);
        toolCommand.Metadata.Returns(new ToolMetadata { Destructive = false });
        toolCommand.GetCommand().Returns(new Command("tool-command"));
        commandFactory.AllCommands.Returns(new Dictionary<string, IBaseCommand>
        {
            [toolName] = toolCommand
        });
        var options = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        var logger = Substitute.For<ILogger<CommandFactoryToolLoader>>();

        var mcpRuntime = CreateRuntime(new CommandFactoryToolLoader(commandFactory, options, logger));
        var request = CreateToolCallRequest(mcpServer, toolName);

        await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(ActivityStatusCode.Ok, _activity.Status);
        Assert.Equal(true, GetAndAssertTagKeyValue(_activity, TagName.IsServerCommandInvoked));
        Assert.Equal(toolName, GetAndAssertTagKeyValue(_activity, TagName.ToolName));
        Assert.Equal(toolArea, GetAndAssertTagKeyValue(_activity, TagName.ToolArea));
        Assert.Equal(toolId, GetAndAssertTagKeyValue(_activity, TagName.ToolId));
    }

    private IMcpRuntime CreateRuntime(IToolLoader toolLoader)
    {
        var telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>()).Returns(_activity);

        var runtime = new McpRuntime(toolLoader, telemetry);

        return runtime;
    }

    private static RequestContext<CallToolRequestParams> CreateToolCallRequest(McpServer mcpServer, string toolName) =>
        new(mcpServer, new() { Method = RequestMethods.ToolsCall }, new()
        {
            Name = toolName,
            Arguments = new Dictionary<string, JsonElement>()
        });

    private static object GetAndAssertTagKeyValue(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));

        Assert.False(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was not found in activity tags.");
        Assert.NotNull(matching.Value);

        return matching.Value;
    }

    private static void AssertTagDoesNotExist(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));
        Assert.True(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was found in activity tags but should not exist.");
    }
}
