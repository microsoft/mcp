// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public class ItemDeleteToolRegistrationTests()
{
    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    public async Task RegisteredTool_ExposesSchemaAnnotationsAndTypedConfirmation(StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(ItemDeleteTestData.ItemUrl + "?hardDelete=true", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            return Task.FromResult(ItemDeleteTestData.CreateResponse());
        });
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential, new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            Transport = transport,
            StructuredOutputMode = mode,
            DangerouslyDisableElicitation = true
        });
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(listed.Tools, candidate => candidate.Name == ItemDeleteTestData.ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["item-id", "workspace-id"],
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        var properties = tool.InputSchema.GetProperty("properties");
        Assert.Equal(["hard-delete", "item-id", "workspace-id"], properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(["boolean", "null"], properties.GetProperty("hard-delete").GetProperty("type").EnumerateArray().Select(value => value.GetString()));
        Assert.False(properties.GetProperty("hard-delete").TryGetProperty("default", out _));
        Assert.False(tool.InputSchema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var arguments = ItemDeleteTestData.CreateArguments();
        arguments["workspace-id"] = $"{{{ItemDeleteTestData.WorkspaceId.ToUpperInvariant()}}}";
        arguments["item-id"] = Guid.Parse(ItemDeleteTestData.ItemId).ToString("N");
        arguments["hard-delete"] = true;
        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        JsonElement payload;
        if (mode is null)
        {
            Assert.Null(result.StructuredContent);
            using var document = JsonDocument.Parse(text);
            payload = document.RootElement.GetProperty("results").Clone();
        }
        else
        {
            Assert.NotNull(tool.OutputSchema);
            Assert.Equal(["hardDeleteRequested", "itemId", "workspaceId"],
                tool.OutputSchema.Value.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Duplicated)
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
            else
            {
                Assert.DoesNotContain(ItemDeleteTestData.ItemId, text);
            }
        }

        Assert.Equal(["hardDeleteRequested", "itemId", "workspaceId"], payload.EnumerateObject().Select(property => property.Name).Order());
        var confirmation = JsonSerializer.Deserialize(payload, CoreJsonContext.Default.ItemDeleteCommandResult);
        Assert.NotNull(confirmation);
        Assert.Equal(Guid.Parse(ItemDeleteTestData.WorkspaceId), confirmation.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemDeleteTestData.ItemId), confirmation.ItemId);
        Assert.True(confirmation.HardDeleteRequested);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("null", null)]
    [InlineData("false", false)]
    [InlineData("true", true)]
    [InlineData("\"false\"", false)]
    [InlineData("\"true\"", true)]
    [InlineData("[\"false\"]", false)]
    [InlineData("[\"true\"]", true)]
    public async Task RegisteredMcp_PreservesExplicitValuesAndExistingCoercion(string? jsonValue, bool? expectedHardDelete)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            var query = expectedHardDelete switch { true => "?hardDelete=true", false => "?hardDelete=false", _ => "" };
            Assert.Equal(ItemDeleteTestData.ItemUrl + query, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(ItemDeleteTestData.CreateResponse());
        });
        await using var provider = ItemDeleteTestData.CreateServices(handler, ItemDeleteTestData.CreateCredential());
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = ItemDeleteTestData.CreateArguments();
        if (jsonValue is not null)
        {
            using var document = JsonDocument.Parse(jsonValue);
            arguments["hard-delete"] = document.RootElement.Clone();
        }

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        using var response = JsonDocument.Parse(Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(expectedHardDelete is true, response.RootElement.GetProperty("results").GetProperty("hardDeleteRequested").GetBoolean());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\" \"")]
    [InlineData("\"yes\"")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("[\"true\", \"false\"]")]
    [InlineData("\"--hard-delete\"")]
    public async Task RegisteredMcp_RejectsInvalidHardDeleteBeforeAuthentication(string jsonValue)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        using var document = JsonDocument.Parse(jsonValue);
        var arguments = ItemDeleteTestData.CreateArguments();
        arguments["hard-delete"] = document.RootElement.Clone();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("hardDelete", false, true)]
    [InlineData("HARD-DELETE", true, false)]
    [InlineData("hardDelete", true, true)]
    public async Task RegisteredMcp_RejectsAliasesThatRepeatTheSameOption(string secondName, bool firstValue, bool secondValue)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = ItemDeleteTestData.CreateArguments();
        arguments["hard-delete"] = firstValue;
        arguments[secondName] = secondValue;

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("workspace-id", null)]
    [InlineData("workspace-id", "not-a-uuid")]
    [InlineData("workspace-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("item-id", null)]
    [InlineData("item-id", "../items")]
    [InlineData("item-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("item-id", ItemDeleteTestData.ItemId + "?hardDelete=true")]
    [InlineData("workspace", "a-workspace-name")]
    public async Task RegisteredMcp_RejectsInvalidIdsAndUnknownOptionsBeforeAuthentication(string name, string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = ItemDeleteTestData.CreateArguments();
        arguments[name] = value;
        arguments["hard-delete"] = true;

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("--hard-delete false", "?hardDelete=false")]
    [InlineData("--hard-delete true", "?hardDelete=true")]
    [InlineData("--hard-delete=true", "?hardDelete=true")]
    public async Task RegisteredCli_UsesTheGuardedCommandThroughTheRoot(string arguments, string query)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(ItemDeleteTestData.ItemUrl + query, request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Delete, request.Method);
            return Task.FromResult(ItemDeleteTestData.CreateResponse());
        });
        await using var provider = ItemDeleteTestData.CreateServices(handler, ItemDeleteTestData.CreateCredential());
        var factory = provider.GetRequiredService<ICommandFactory>();

        var parsed = factory.RootCommand.Parse($"core delete-item {ItemDeleteTestData.RequiredArguments} {arguments}");
        Assert.Empty(parsed.Errors);
        var exitCode = await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal((int)HttpStatusCode.OK, exitCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("--hard-delete")]
    [InlineData("--hard-delete \"\"")]
    [InlineData("--hard-delete=yes")]
    [InlineData("--hard-delete false true")]
    [InlineData("--hard-delete false --hard-delete true")]
    [InlineData("--hard-delete true --hard-delete false")]
    [InlineData("--hard-delete true --hard-delete true")]
    [InlineData("--hard-delete --hard-delete true")]
    [InlineData("--hard-delete true --hard-delete")]
    public async Task RegisteredCli_RejectsValuelessInvalidAndRepeatedFlagsBeforeAuthentication(string arguments)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential);
        var factory = provider.GetRequiredService<ICommandFactory>();

        var parsed = factory.RootCommand.Parse($"core delete-item {ItemDeleteTestData.RequiredArguments} {arguments}");
        Assert.NotEmpty(parsed.Errors);
        var exitCode = await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEqual((int)HttpStatusCode.OK, exitCode);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.NotFound, null, HttpStatusCode.NotFound)]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden)]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Unauthorized, null, HttpStatusCode.Unauthorized)]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests)]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", HttpStatusCode.TooManyRequests)]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.InternalServerError, null, HttpStatusCode.InternalServerError)]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Accepted, null, HttpStatusCode.BadGateway)]
    public async Task RegisteredMcp_ReturnsSanitizedFailuresWithoutConfirmation(
        StructuredOutputMode? mode, HttpStatusCode status, string? retryAfter, HttpStatusCode expectedStatus)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = ItemDeleteTestData.CreateResponse(status, "private-backend-detail");
            if (retryAfter is not null)
            {
                response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            }
            return Task.FromResult(response);
        });
        await using var provider = ItemDeleteTestData.CreateServices(handler, ItemDeleteTestData.CreateCredential(), new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            StructuredOutputMode = mode,
            DangerouslyDisableElicitation = true
        });
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, ItemDeleteTestData.CreateArguments()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal((int)expectedStatus, document.RootElement.GetProperty("status").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-", text);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task RegisteredMcp_ReturnsCancellationWithoutAuthenticatingOrConfirmingDeletion()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, ItemDeleteTestData.CreateArguments()),
            cancellation.Token);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        using var document = JsonDocument.Parse(Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal((int)HttpStatusCode.RequestTimeout, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("deletion state was not confirmed", document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task RegisteredTool_IsHiddenAndUnavailableInReadOnlyMode()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential, new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = true
        });
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(listed.Tools, tool => tool.Name == ItemDeleteTestData.ToolName);
        Assert.Contains(listed.Tools, tool => tool.Name == "core_search-catalog");
        Assert.Equal(
            ["core_get-capacity", "core_get-workspace", "core_list-capacities", "core_list-items", "core_list-workspaces", "core_search-catalog"],
            listed.Tools.Select(tool => tool.Name).Order());
        Assert.All(listed.Tools, tool => Assert.True(tool.Annotations?.ReadOnlyHint));

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, ItemDeleteTestData.CreateArguments()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("read-only", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("accept", "accept", true)]
    [InlineData("accept", "reject", false)]
    [InlineData("accept", null, false)]
    [InlineData("decline", "accept", false)]
    [InlineData("cancel", "accept", false)]
    public async Task RegisteredTool_PreservesDestructiveElicitation(string action, string? decision, bool shouldDelete)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDeleteTestData.CreateResponse()));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential, new ServerRuntimeConfiguration { Namespace = ["core"] });
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = ItemDeleteTestData.CreateArguments();
        arguments["hard-delete"] = true;
        var request = McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments);
        request.Server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new() { Form = new() } });
        request.Server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()).Returns(new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = new JsonObject
            {
                ["action"] = action,
                ["content"] = new JsonObject { ["decision"] = decision }
            }
        });

        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(!shouldDelete, result.IsError);
        Assert.Equal(shouldDelete ? 1 : 0, handler.CallCount);
        if (!shouldDelete)
        {
            Assert.Empty(credential.ReceivedCalls());
        }
        await request.Server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(message => message.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RegisteredTool_RejectsClientsWithoutElicitation(bool? hardDelete)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemDeleteTestData.CreateCredential();
        await using var provider = ItemDeleteTestData.CreateServices(handler, credential, new ServerRuntimeConfiguration { Namespace = ["core"] });
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = ItemDeleteTestData.CreateArguments();
        arguments["hard-delete"] = hardDelete;
        var request = McpTestUtilities.CreateToolCallRequest(ItemDeleteTestData.ToolName, arguments);
        request.Server.ClientCapabilities.Returns((ClientCapabilities?)null);

        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public void Registration_DoesNotAccumulateValidatorsAcrossServiceProviders()
    {
        using var firstHandler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var secondHandler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var first = ItemDeleteTestData.CreateServices(firstHandler, ItemDeleteTestData.CreateCredential());
        using var second = ItemDeleteTestData.CreateServices(secondHandler, ItemDeleteTestData.CreateCredential());

        var firstCommand = first.GetRequiredService<ItemDeleteCommand>().GetCommand();
        var secondCommand = second.GetRequiredService<ItemDeleteCommand>().GetCommand();

        Assert.Single(firstCommand.Validators);
        Assert.Single(secondCommand.Validators);
        Assert.Empty(Assert.Single(firstCommand.Options, option => option.Name == "--hard-delete").Validators);
        Assert.Empty(Assert.Single(secondCommand.Options, option => option.Name == "--hard-delete").Validators);
        Assert.NotEmpty(firstCommand.Parse($"{ItemDeleteTestData.RequiredArguments} --hard-delete").Errors);
        Assert.NotEmpty(secondCommand.Parse($"{ItemDeleteTestData.RequiredArguments} --hard-delete").Errors);
    }
}
