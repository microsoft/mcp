// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class WorkspaceAssignToCapacityMcpTests
    : CommandUnitTestsBase<WorkspaceAssignToCapacityCommand, IFabricCoreService>
{
    private const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff512";
    private const string CapacityId = "0f084df7-c13d-451b-af5f-ed0c466403b2";
    private const string ToolName = "core_assign-workspace-to-capacity";

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Duplicated)]
    [InlineData(StructuredOutputMode.Compact)]
    public async Task ListToolsHandler_AdvertisesRequiredIdsAndTypedPendingSchema(StructuredOutputMode? outputMode)
    {
        await using var loader = CreateLoader("all", outputMode);
        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools);

        Assert.Equal(ToolName, tool.Name);
        Assert.False(tool.Annotations!.ReadOnlyHint);
        Assert.True(tool.Annotations.DestructiveHint);
        Assert.False(tool.Annotations.IdempotentHint);
        Assert.False(tool.Annotations.OpenWorldHint);
        var required = tool.InputSchema.GetProperty("required").EnumerateArray()
            .Select(value => Assert.IsType<string>(value.GetString())).ToArray();
        Assert.Equal(["capacity-id", "workspace-id"], required.Order().ToArray());
        Assert.Equal(outputMode.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            var properties = schema.GetProperty("properties");
            Assert.Equal(4, properties.EnumerateObject().Count());
            Assert.Equal("string", properties.GetProperty("workspaceId").GetProperty("type").GetString());
            Assert.Equal("string", properties.GetProperty("capacityId").GetProperty("type").GetString());
            Assert.Equal("boolean", properties.GetProperty("accepted").GetProperty("type").GetString());
            Assert.Contains(properties.GetProperty("state").GetProperty("type").EnumerateArray(),
                type => type.GetString() == "string");
        }
    }

    [Theory]
    [InlineData("all", null)]
    [InlineData("all", StructuredOutputMode.Duplicated)]
    [InlineData("all", StructuredOutputMode.Compact)]
    [InlineData("namespace", null)]
    [InlineData("namespace", StructuredOutputMode.Duplicated)]
    [InlineData("namespace", StructuredOutputMode.Compact)]
    [InlineData("single", null)]
    [InlineData("single", StructuredOutputMode.Duplicated)]
    [InlineData("single", StructuredOutputMode.Compact)]
    public async Task CallToolHandler_PreservesAcceptedPendingReceiptAcrossOutputModes(string mode, StructuredOutputMode? outputMode)
    {
        await using var loader = CreateLoader(mode, outputMode);
        var result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        if (mode == "single" || outputMode != StructuredOutputMode.Compact)
        {
            using var response = JsonDocument.Parse(text);
            Assert.Equal(202, response.RootElement.GetProperty("status").GetInt32());
            AssertPendingReceipt(response.RootElement.GetProperty("results"));
        }
        // The existing single proxy returns legacy text for local commands in every output mode.
        Assert.Equal(mode != "single" && outputMode.HasValue, result.StructuredContent.HasValue);
        if (result.StructuredContent is { } structured)
        {
            AssertPendingReceipt(mode == "all" ? structured : structured.GetProperty("result"));
        }
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task CallToolHandler_ReadOnlyModePreventsMutation(string mode)
    {
        await using var loader = CreateLoader(mode, StructuredOutputMode.Compact, readOnly: true);

        var result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);

        Assert.Equal(mode == "single" ? null : (bool?)true, result.IsError);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task CallToolHandler_HttpModeAllowsThisRemoteTool(string mode)
    {
        await using var loader = CreateLoader(mode, StructuredOutputMode.Compact, transport: TransportTypes.Http);

        var result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task CallToolHandler_FailuresRemainErrorsWithoutPendingReceipt(string mode)
    {
        Service.AssignWorkspaceToCapacityAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("sensitive-backend-details", null, HttpStatusCode.Forbidden));
        await using var loader = CreateLoader(mode, StructuredOutputMode.Compact);

        var result = await loader.CallToolHandler(CreateRequest(mode), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var response = JsonDocument.Parse(text);
        Assert.Equal(403, response.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain("sensitive", text);
        Assert.DoesNotContain("\"accepted\"", text);
        Assert.Single(Service.ReceivedCalls());
    }

    private IToolLoader CreateLoader(
        string mode, StructuredOutputMode? outputMode, bool readOnly = false, string transport = TransportTypes.StdIo)
    {
        var factory = Substitute.For<ICommandFactory>();
        factory.AllCommands.Returns(new Dictionary<string, IBaseCommand> { [ToolName] = Command });
        factory.GroupCommands(Arg.Any<string[]>())
            .Returns(new Dictionary<string, IBaseCommand> { [ToolName] = Command });
        factory.FindCommandByName(ToolName).Returns(Command);
        factory.GetServiceArea(ToolName).Returns("core");
        var core = new CommandGroup("core", "Core test commands");
        core.AddCommand(Command.Name, Command);
        var root = new CommandGroup("root", "Offline test commands");
        root.AddSubGroup(core);
        factory.RootGroup.Returns(root);
        var options = OptionsFactory.Create(new ServerRuntimeConfiguration
        {
            Mode = mode,
            StructuredOutputMode = outputMode,
            ReadOnly = readOnly,
            Transport = transport,
            DangerouslyDisableElicitation = true
        });
        return mode switch
        {
            "all" => new CommandFactoryToolLoader(factory, options, NullLogger<CommandFactoryToolLoader>.Instance),
            "namespace" => new NamespaceToolLoader(factory, options, NullLogger<NamespaceToolLoader>.Instance),
            "single" => new SingleProxyToolLoader(factory, NullLogger<SingleProxyToolLoader>.Instance, options,
                OptionsFactory.Create(new McpServerConfiguration
                {
                    RootCommandGroupName = "fabmcp",
                    Name = "Fabric.Mcp.Server",
                    ShortName = "fabric",
                    DisplayName = "Fabric",
                    Version = "1.0.0",
                    Description = "Offline tests"
                })),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateRequest(string mode)
    {
        var parameters = new Dictionary<string, object?> { ["workspace-id"] = WorkspaceId, ["capacity-id"] = CapacityId };
        if (mode == "all")
        {
            return McpTestUtilities.CreateToolCallRequest(ToolName, parameters);
        }

        var arguments = new Dictionary<string, object?>
        {
            ["intent"] = "Submit the workspace capacity assignment",
            ["command"] = ToolName,
            ["parameters"] = parameters
        };
        if (mode == "single")
        {
            arguments["tool"] = "core";
        }
        return McpTestUtilities.CreateToolCallRequest(mode == "namespace" ? "core" : "fabric", arguments);
    }

    private static void AssertPendingReceipt(JsonElement receipt)
    {
        Assert.Equal(WorkspaceId, receipt.GetProperty("workspaceId").GetString());
        Assert.Equal(CapacityId, receipt.GetProperty("capacityId").GetString());
        Assert.True(receipt.GetProperty("accepted").GetBoolean());
        Assert.Equal("Pending", receipt.GetProperty("state").GetString());
        Assert.Equal(4, receipt.EnumerateObject().Count());
    }
}
