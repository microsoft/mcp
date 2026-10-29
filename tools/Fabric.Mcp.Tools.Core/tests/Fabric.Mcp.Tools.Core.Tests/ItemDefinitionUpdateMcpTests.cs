// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words ipynb

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemDefinitionUpdateTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemDefinitionUpdateMcpTests()
{
    public static TheoryData<string, string, StructuredOutputMode?> RoutingCases
    {
        get
        {
            var data = new TheoryData<string, string, StructuredOutputMode?>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
                    {
                        data.Add(route, transport, output);
                    }
                }
            }
            return data;
        }
    }

    [Fact]
    public async Task Discovery_PreservesInlineSupportAndIsolatesFileOptionsByHost()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        foreach (var transport in new[] { TransportTypes.Http, TransportTypes.StdIo, TransportTypes.Http })
        {
            await using var provider = CreateServices(handler, credential, transport);
            var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
            var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
            var tool = Assert.Single(catalog.Tools, static tool => tool.Name == ToolName);
            var properties = tool.InputSchema.GetProperty("properties");
            Assert.Equal(["item-id", "workspace-id"],
                tool.InputSchema.GetProperty("required").EnumerateArray().Select(static value => value.GetString()).Order());
            foreach (var name in new[] { "workspace-id", "item-id", "definition", "definition-item-type", "options" })
            {
                Assert.Equal("string", properties.GetProperty(name).GetProperty("type").GetString());
            }
            foreach (var name in new[] { "definition-directory", "definition-files", "definition-format" })
            {
                Assert.Equal(transport == TransportTypes.StdIo, properties.TryGetProperty(name, out _));
            }
            foreach (var name in new[] { "folder-id", "creation-payload", "sensitivity-label-settings", "display-name", "item-type" })
            {
                Assert.False(properties.TryGetProperty(name, out _));
            }
            Assert.Equal(["boolean", "null"], properties.GetProperty("update-metadata").GetProperty("type").EnumerateArray()
                .Select(static value => value.GetString()));
            Assert.False(properties.GetProperty("update-metadata").TryGetProperty("default", out _));
            Assert.Equal("boolean", properties.GetProperty("sync").GetProperty("type").GetString());
            Assert.Equal("boolean", properties.GetProperty("early-poll").GetProperty("type").GetString());
            Assert.Equal("number", properties.GetProperty("max-wait-seconds").GetProperty("type").GetString());
            Assert.True(tool.Annotations?.DestructiveHint);
            Assert.True(tool.Annotations?.IdempotentHint);
            Assert.False(tool.Annotations?.ReadOnlyHint);
            Assert.False(tool.Annotations?.OpenWorldHint);
            var command = provider.GetRequiredService<ItemDefinitionUpdateCommand>();
            Assert.False(command.Metadata.LocalRequired);
            Assert.False(command.Metadata.Secret);
            Assert.Equal("e15155f3-c5e4-423b-9d88-ce4e8d701a4f", command.Id);
            Assert.Same(CoreJsonContext.Default.ItemDefinitionUpdateCommandResult, command.ResultTypeInfo);
            Assert.Equal(["operation"], tool.OutputSchema!.Value.GetProperty("properties").EnumerateObject().Select(static property => property.Name));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [MemberData(nameof(RoutingCases))]
    public async Task InlineDefinition_HasOnePostAndTypedNoResultCompletionAcrossRoutes(
        string route, string transport, StructuredOutputMode? output)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(Url, request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(["definition"], body.RootElement.EnumerateObject().Select(static property => property.Name));
            using var expected = JsonDocument.Parse(Definition);
            Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty("definition")));
            return new(HttpStatusCode.OK);
        });
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = CreateServices(handler, credential, transport, output, files);

        var result = await Loader(provider, route).CallToolHandler(Request(route, Arguments()), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var payload = Payload(result, route);
        var typed = JsonSerializer.Deserialize(payload, CoreJsonContext.Default.ItemDefinitionUpdateCommandResult);
        Assert.NotNull(typed);
        Assert.Equal(FabricOperationStatus.Succeeded, typed.Operation.Status);
        Assert.Null(typed.Operation.OperationId);
        Assert.Equal(["operation"], payload.EnumerateObject().Select(static property => property.Name));
        Assert.DoesNotContain("e30=", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", null)]
    [InlineData("namespace", false)]
    [InlineData("single", true)]
    public async Task SavedFiles_SendTheCompleteExplicitSelectionWithoutChangingFilesOrPlatform(string route, bool? metadata)
    {
        using var directory = new FabricDefinitionTestDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "notebooks"));
        byte[] notebook = [0xef, 0xbb, 0xbf, .. Encoding.UTF8.GetBytes("{\"cells\":[]}\r\n")];
        byte[] platform = Encoding.UTF8.GetBytes("""{"metadata":{"type":"Notebook","displayName":"Selected name"}}""");
        var source = Path.Combine(directory.Path, "notebooks", "sales.ipynb");
        await File.WriteAllBytesAsync(source, notebook, TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(directory.Path, ".platform"), platform, TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(directory.Path, "empty.bin"), [], TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(directory.Path, ".env"), "private-unselected", TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            var query = metadata switch { true => "?updateMetadata=true", false => "?updateMetadata=false", _ => "" };
            Assert.Equal(Url + query, request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(["definition", "options"], body.RootElement.EnumerateObject().Select(static property => property.Name).Order());
            var definition = body.RootElement.GetProperty("definition");
            Assert.Equal("ipynb", definition.GetProperty("format").GetString());
            var parts = definition.GetProperty("parts").EnumerateArray().ToArray();
            Assert.Equal(["artifact.content.ipynb", ".platform", "assets/empty.bin"], parts.Select(static part => part.GetProperty("path").GetString()));
            Assert.Equal(notebook, Convert.FromBase64String(parts[0].GetProperty("payload").GetString()!));
            Assert.Equal(platform, Convert.FromBase64String(parts[1].GetProperty("payload").GetString()!));
            Assert.Equal("", parts[2].GetProperty("payload").GetString());
            Assert.False(body.RootElement.GetProperty("options").GetProperty("futureSetting").GetBoolean());
            Assert.DoesNotContain("private-", body.RootElement.GetRawText());
            Assert.DoesNotContain(directory.Path, body.RootElement.GetRawText());
            return new(HttpStatusCode.OK);
        });
        await using var provider = CreateServices(handler, FabricOperationTestData.Credential());
        var arguments = Arguments(inline: false);
        arguments["definition-directory"] = directory.Path;
        arguments["definition-files"] = """
            [{"partPath":"artifact.content.ipynb","sourcePath":"notebooks/sales.ipynb"},{"partPath":".platform"},{"partPath":"assets/empty.bin","sourcePath":"empty.bin"}]
            """;
        arguments["definition-format"] = "ipynb";
        arguments["definition-item-type"] = "Notebook";
        arguments["options"] = """{"futureSetting":false}""";
        if (metadata is not null)
        {
            arguments["update-metadata"] = metadata;
        }

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(notebook, await File.ReadAllBytesAsync(source, TestContext.Current.CancellationToken));
        Assert.Equal(platform, await File.ReadAllBytesAsync(Path.Combine(directory.Path, ".platform"), TestContext.Current.CancellationToken));
        Assert.False(File.Exists(Path.Combine(directory.Path, "artifact.content.ipynb")));
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task HttpFiles_AreRejectedBeforeFileAccessAuthenticationOrPost(string route)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = CreateServices(handler, credential, TransportTypes.Http, files: files);
        var arguments = Arguments(inline: false);
        arguments["definition-directory"] = "private-host-directory";
        arguments["definition-files"] = """[{"partPath":"content"}]""";

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task DirectCommand_StillEnforcesTheIndependentServiceFileGuard()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = CreateServices(handler, credential, files: files);
        var command = provider.GetRequiredService<ItemDefinitionUpdateCommand>();
        provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServerRuntimeConfiguration>>().Value.Transport = TransportTypes.Http;

        var response = await command.ExecuteAsync(new(), new ItemDefinitionUpdateOptions
        {
            WorkspaceId = FabricOperationTestData.WorkspaceId,
            ItemId = FabricOperationTestData.ItemId,
            DefinitionDirectory = "private-host-directory",
            DefinitionFiles = "private-invalid-json"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("inline --definition", response.Message);
        Assert.DoesNotContain("private-", response.Message);
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(Definition, "private-directory", """[{"partPath":"content"}]""", null)]
    [InlineData(Definition, null, null, "ipynb")]
    [InlineData(null, "private-directory", null, null)]
    [InlineData(null, null, """[{"partPath":"content"}]""", null)]
    [InlineData(null, null, null, "ipynb")]
    [InlineData(null, "private-directory", "[]", null)]
    [InlineData(null, "private-directory", """[{"partPath":"content","private-unknown":true}]""", null)]
    [InlineData(null, "private-directory", """[{"partPath":"content","partPath":"other"}]""", null)]
    public async Task InvalidSources_FailBeforeIo(string? definition, string? directory, string? filesJson, string? format)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = CreateServices(handler, credential, files: files);
        var arguments = Arguments(inline: false);
        arguments["definition"] = definition;
        arguments["definition-directory"] = directory;
        arguments["definition-files"] = filesJson;
        arguments["definition-format"] = format;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private-invalid-json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"definition":{"parts":[]}}""")]
    [InlineData("""{"parts":[]}""")]
    [InlineData("""{"parts":[null]}""")]
    [InlineData("""{"parts":[{"path":"content","payload":"e30="}]}""")]
    [InlineData("""{"parts":[{"path":"content","payload":"e30=","payloadType":"Uri"}]}""")]
    [InlineData("""{"parts":[{"path":"content","payload":"private-payload","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"content","payload":"e30=","payloadType":"InlineBase64","private-field":true}]}""")]
    [InlineData("""{"parts":[{"path":"content","path":"other","payload":"e30=","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"../private-file","payload":"e30=","payloadType":"InlineBase64"}]}""")]
    public async Task InvalidInlineInput_IsNotSilentlyAccepted(string definition)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential);
        var arguments = Arguments();
        arguments["definition"] = definition;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-", Text(result));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("../private-outside")]
    [InlineData("missing-private-file")]
    [InlineData("content:private-stream")]
    [InlineData("oversized")]
    public async Task UnsafeMissingOrOversizedFiles_FailWithoutARequest(string source)
    {
        using var directory = new FabricDefinitionTestDirectory();
        await File.WriteAllBytesAsync(Path.Combine(directory.Path, "oversized"), new byte[9], TestContext.Current.CancellationToken);
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential, limits: new(MaxParts: 2, MaxPartBytes: 8, MaxTotalBytes: 8));
        var arguments = Arguments(inline: false);
        arguments["definition-directory"] = directory.Path;
        arguments["definition-files"] = $$"""[{"partPath":"content","sourcePath":"{{source}}"}]""";

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain(directory.Path, Text(result));
        Assert.DoesNotContain("private-", Text(result));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("true")]
    [InlineData("{")]
    [InlineData("""{"setting":{"private-duplicate":1,"private-duplicate":2}}""")]
    public async Task InvalidOptions_AreValidatedBeforeReadingFiles(string json)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = CreateServices(handler, credential, files: files);
        var arguments = Arguments(inline: false);
        arguments["definition-directory"] = "private-path";
        arguments["definition-files"] = """[{"partPath":"content"}]""";
        arguments["options"] = json;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("--options", Text(result));
        Assert.DoesNotContain("private-", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("ascii", false)]
    [InlineData("ascii", true)]
    [InlineData("utf8", false)]
    [InlineData("utf8", true)]
    [InlineData("depth", false)]
    [InlineData("depth", true)]
    public async Task Options_EnforceExactByteAndDepthLimitsThroughTheRealCommand(string kind, bool exceed)
    {
        string json;
        if (kind == "depth")
        {
            var depth = FabricItemJsonInput.MaxObjectDepth + (exceed ? 1 : 0);
            json = "{\"data\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        }
        else
        {
            var character = kind == "utf8" ? '\u00e9' : 'x';
            var bytesPerCharacter = Encoding.UTF8.GetByteCount(character.ToString());
            var size = FabricItemJsonInput.MaxObjectBytes - 11;
            json = "{\"data\":\"" + new string(character, size / bytesPerCharacter) + new string('x', size % bytesPerCharacter) + "\"}";
            Assert.Equal(FabricItemJsonInput.MaxObjectBytes, Encoding.UTF8.GetByteCount(json));
            if (exceed)
            {
                json += " ";
            }
        }
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            using var expected = JsonDocument.Parse(json);
            Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty("options")));
            return new(HttpStatusCode.OK);
        });
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential, TransportTypes.Http);
        var arguments = Arguments();
        arguments["options"] = json;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.Equal(exceed, result.IsError);
        Assert.Equal(exceed ? 0 : 1, handler.CallCount);
        if (exceed)
        {
            Assert.Empty(credential.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("FutureType", true)]
    [InlineData("Notebook", false)]
    [InlineData(" ", false)]
    public async Task DefinitionType_IsOnlyAnOptionalLocalHint(string? itemType, bool succeeds)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(["definition"], body.RootElement.EnumerateObject().Select(static property => property.Name));
            return new(HttpStatusCode.OK);
        });
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential);
        var arguments = Arguments();
        arguments["definition-item-type"] = itemType;
        arguments["definition"] = JsonSerializer.Serialize(new FabricItemDefinition(
            [new("content", "e30=", "InlineBase64"),
                new(".platform", Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"metadata":{"type":"Notebook"}}""")), "InlineBase64")]),
            CoreJsonContext.Default.FabricItemDefinition);

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.Equal(!succeeds, result.IsError);
        Assert.Equal(succeeds ? 1 : 0, handler.CallCount);
        if (!succeeds)
        {
            Assert.Empty(credential.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("null", null)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"true\"", true)]
    [InlineData("[\"false\"]", false)]
    public async Task UpdateMetadata_PreservesOmissionAndExplicitValues(string? value, bool? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            var query = expected switch { true => "?updateMetadata=true", false => "?updateMetadata=false", _ => "" };
            Assert.Equal(Url + query, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        await using var provider = CreateServices(handler, FabricOperationTestData.Credential());
        var arguments = Arguments();
        if (value is not null)
        {
            using var json = JsonDocument.Parse(value);
            arguments["update-metadata"] = json.RootElement.Clone();
        }

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("--update-metadata")]
    [InlineData("--update-metadata", "private-invalid")]
    [InlineData("--update-metadata", "true", "--update-metadata", "false")]
    [InlineData("--definition", Definition)]
    [InlineData("--options", "{}", "--options", "{}")]
    [InlineData("--creation-payload", "{}")]
    [InlineData("--folder-id", FabricOperationTestData.ItemId)]
    [InlineData("--sensitivity-label-settings", "{}")]
    public async Task Cli_RejectsAmbiguousOrUnsupportedOptions(params string[] extra)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential);
        var command = provider.GetRequiredService<ItemDefinitionUpdateCommand>();
        IBaseCommand executable = command;

        var response = await executable.ExecuteAsync(new(), command.GetCommand().Parse(
            ["--workspace-id", FabricOperationTestData.WorkspaceId, "--item-id", FabricOperationTestData.ItemId, "--definition", Definition, .. extra]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.DoesNotContain("private-", response.Message);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all", false)]
    [InlineData("namespace", false)]
    [InlineData("single", false)]
    [InlineData("all", true)]
    [InlineData("namespace", true)]
    [InlineData("single", true)]
    public async Task AcceptedUpdate_OnlyWaitsWhenRequestedAndNeverRetrievesAResult(string route, bool sync)
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal(Url, request.RequestUri?.AbsoluteUri);
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}", request.RequestUri?.AbsolutePath);
            return Task.FromResult(FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""));
        });
        await using var provider = CreateServices(handler, FabricOperationTestData.Credential(), TransportTypes.Http, clock: clock);
        var arguments = Arguments();
        arguments["sync"] = sync;
        var task = Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken).AsTask();
        if (sync)
        {
            await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
            clock.Advance(3);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var operation = Payload(result, route).GetProperty("operation");
        Assert.Equal(sync ? "Succeeded" : "Accepted", operation.GetProperty("status").GetString());
        Assert.Equal(FabricOperationTestData.OperationId, operation.GetProperty("operationId").GetString());
        Assert.Equal(sync ? 2 : 1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Compact)]
    [InlineData(StructuredOutputMode.Duplicated)]
    public async Task CallerCancellation_PreservesTheAcceptedReferenceInErrorOutput(StructuredOutputMode? output)
    {
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        await using var provider = CreateServices(handler, FabricOperationTestData.Credential(),
            TransportTypes.Http, output, clock: clock);
        var arguments = Arguments();
        arguments["sync"] = true;

        var task = Loader(provider, "all").CallToolHandler(Request("all", arguments), caller.Token).AsTask();
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        await caller.CancelAsync();
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var operation = Payload(result, "all").GetProperty("operation");
        Assert.Equal(FabricOperationTestData.OperationId, operation.GetProperty("operationId").GetString());
        Assert.Equal("CallerCanceled", operation.GetProperty("issue").GetProperty("code").GetString());
        using var response = JsonDocument.Parse(Text(result));
        Assert.Equal(408, response.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Fact]
    public async Task ReadOnlyMode_HidesAndRejectsTheTool()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential, readOnly: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, static tool => tool.Name == ToolName);

        var result = await loader.CallToolHandler(Request("all", Arguments()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("accept", "accept", true)]
    [InlineData("accept", "reject", false)]
    [InlineData("decline", "accept", false)]
    [InlineData("cancel", "accept", false)]
    public async Task Replacement_RequiresDestructiveConsentWhenSupported(string action, string decision, bool accepted)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var credential = FabricOperationTestData.Credential();
        await using var provider = CreateServices(handler, credential, disableElicitation: false);
        var request = Request("all", Arguments());
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

        var result = await Loader(provider, "all").CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.Equal(!accepted, result.IsError);
        Assert.Equal(accepted ? 1 : 0, handler.CallCount);
        if (!accepted)
        {
            Assert.Empty(credential.ReceivedCalls());
        }
        await request.Server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(message => message.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }
}
