// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words ipynb

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemCreateDefinitionMcpTests()
{
    private const string ToolName = "core_create-item";
    private const string FolderId = "bbbbbbbb-1111-2222-3333-cccccccccccc";
    private const string LabelId = "b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f";
    private const string InlineDefinition = """
        {"format":"ipynb","parts":[{"path":"artifact.content.ipynb","payload":"e30=","payloadType":"InlineBase64"}]}
        """;
    private static readonly string[] LocalOptions = ["definition-directory", "definition-files", "definition-format"];

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
    public async Task Discovery_HidesOnlyFileInputsOverHttpWithoutChangingOtherHosts()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        foreach (var transport in new[] { TransportTypes.Http, TransportTypes.StdIo, TransportTypes.Http })
        {
            await using var provider = Services(handler, credential, transport);
            var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

            var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

            var tool = Assert.Single(catalog.Tools, static tool => tool.Name == ToolName);
            var properties = tool.InputSchema.GetProperty("properties");
            Assert.Equal("string", properties.GetProperty("definition").GetProperty("type").GetString());
            foreach (var option in new[] { "folder-id", "creation-payload", "options", "sensitivity-label-settings" })
            {
                Assert.Equal("string", properties.GetProperty(option).GetProperty("type").GetString());
            }
            foreach (var option in LocalOptions)
            {
                Assert.Equal(transport == TransportTypes.StdIo, properties.TryGetProperty(option, out _));
            }
            Assert.Equal(["display-name", "item-type"],
                tool.InputSchema.GetProperty("required").EnumerateArray().Select(static value => value.GetString()).Order());
            Assert.False(tool.Annotations?.ReadOnlyHint);
            Assert.False(tool.Annotations?.IdempotentHint);
            Assert.False(tool.Annotations?.DestructiveHint);
            Assert.False(provider.GetRequiredService<ItemCreateCommand>().Metadata.LocalRequired);
            Assert.Equal("bfdfd3c0-4551-4454-a930-5bf5b1ad5690", provider.GetRequiredService<ItemCreateCommand>().Id);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [MemberData(nameof(RoutingCases))]
    public async Task InlineDefinition_UsesOnePostAndPreservesOutputAcrossRoutesAndTransports(
        string route, string transport, StructuredOutputMode? output)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{FabricOperationTestData.WorkspaceId}/items", request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(["definition", "displayName", "type"], body.RootElement.EnumerateObject().Select(static property => property.Name).Order());
            using var expected = JsonDocument.Parse(InlineDefinition);
            Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty("definition")));
            return FabricOperationTestData.ItemResponse(type: "Notebook");
        });
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, transport, output, files: files);
        var arguments = Arguments();
        arguments["definition"] = InlineDefinition;

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(FabricOperationTestData.ItemId, Payload(result, route).GetProperty("item").GetProperty("id").GetString());
        Assert.False(Payload(result, route).TryGetProperty("operation", out _));
        Assert.DoesNotContain("e30=", Text(result));
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(files.ReceivedCalls());
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task HttpCreationWithoutDefinition_RemainsAvailable(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(["displayName", "type"], body.RootElement.EnumerateObject().Select(static property => property.Name).Order());
            return FabricOperationTestData.ItemResponse();
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.Http);

        var result = await Loader(provider, route).CallToolHandler(Request(route, Arguments()), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task LocalFiles_PackageOnlyExplicitSavedFilesBeforeOnePost(string route)
    {
        using var directory = new FabricDefinitionTestDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "notebooks"));
        Directory.CreateDirectory(Path.Combine(directory.Path, "assets"));
        byte[] content = [0xef, 0xbb, 0xbf, .. Encoding.UTF8.GetBytes("{\"cells\":[]}\r\n")];
        var source = Path.Combine(directory.Path, "notebooks", "sales.ipynb");
        await File.WriteAllBytesAsync(source, content, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(directory.Path, ".platform"), "{}", TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(directory.Path, "assets", "empty.bin"), [], TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(directory.Path, ".env"), "private-unselected-data", TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var definition = body.RootElement.GetProperty("definition");
            Assert.Equal("ipynb", definition.GetProperty("format").GetString());
            var parts = definition.GetProperty("parts").EnumerateArray().ToArray();
            Assert.Equal(["artifact.content.ipynb", ".platform", "assets/empty.bin"],
                parts.Select(static part => part.GetProperty("path").GetString()));
            Assert.Equal(content, Convert.FromBase64String(parts[0].GetProperty("payload").GetString()!));
            Assert.Equal("e30=", parts[1].GetProperty("payload").GetString());
            Assert.Equal("", parts[2].GetProperty("payload").GetString());
            Assert.All(parts, static part => Assert.Equal("InlineBase64", part.GetProperty("payloadType").GetString()));
            Assert.DoesNotContain("private-unselected-data", body.RootElement.GetRawText());
            Assert.DoesNotContain(directory.Path, body.RootElement.GetRawText());
            Assert.Equal(FolderId, body.RootElement.GetProperty("folderId").GetString());
            Assert.False(body.RootElement.GetProperty("options").GetProperty("futureSetting").GetBoolean());
            Assert.Empty(body.RootElement.GetProperty("sensitivityLabelSettings").EnumerateObject());
            Assert.False(body.RootElement.TryGetProperty("creationPayload", out _));
            return FabricOperationTestData.ItemResponse(type: "Notebook");
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.StdIo);
        var arguments = Arguments();
        arguments["definition-directory"] = directory.Path;
        arguments["definition-files"] = """
            [{"partPath":"artifact.content.ipynb","sourcePath":"notebooks/sales.ipynb"},{"partPath":".platform"},{"partPath":"assets/empty.bin"}]
            """;
        arguments["definition-format"] = "ipynb";
        arguments["folder-id"] = FolderId;
        arguments["options"] = """{"futureSetting":false}""";
        arguments["sensitivity-label-settings"] = "{}";

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(content, await File.ReadAllBytesAsync(source, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(Path.Combine(directory.Path, "artifact.content.ipynb")));
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public async Task HttpDirectFileArguments_AreRejectedBeforeFileAccessAuthenticationOrPost(string route)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, TransportTypes.Http, files: files);
        var arguments = Arguments();
        arguments["definition-directory"] = "private-host-path";
        arguments["definition-files"] = """[{"partPath":"notebook-content.py"}]""";

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-host-path", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("""{"parts":[]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"private-invalid-base64","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"artifact.content.ipynb","payload":"e30=","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"notebook-content.py","payload":"e30=","payloadType":"InlineBase64","private-unknown":true}]}""")]
    public async Task InvalidDefinitions_ReturnSanitizedInputErrorsBeforeAuthenticationOrPost(string definition)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http);
        var arguments = Arguments();
        arguments["definition"] = definition;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-", Text(result));
        using var response = JsonDocument.Parse(Text(result));
        Assert.Equal(400, response.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task ConflictingSourcesAndRepeatedCliOptions_DoNotSubmitCreation()
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo);
        var arguments = Arguments();
        arguments["definition"] = InlineDefinition;
        arguments["definition-directory"] = "private-host-path";
        arguments["definition-files"] = """[{"partPath":"artifact.content.ipynb"}]""";

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("not both", Text(result));
        var command = provider.GetRequiredService<ItemCreateCommand>();
        IBaseCommand executable = command;
        var response = await executable.ExecuteAsync(new(), command.GetCommand().Parse(
            ["--workspace-id", FabricOperationTestData.WorkspaceId, "--display-name", "Created", "--item-type", "Notebook",
                "--definition", InlineDefinition, "--definition", InlineDefinition]), TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task ConcurrentInlineInputs_DoNotShareDefinitions()
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var name = body.RootElement.GetProperty("displayName").GetString();
            var part = Assert.Single(body.RootElement.GetProperty("definition").GetProperty("parts").EnumerateArray());
            Assert.Equal(name, Encoding.UTF8.GetString(Convert.FromBase64String(part.GetProperty("payload").GetString()!)));
            Assert.Equal(name, body.RootElement.GetProperty("options").GetProperty("marker").GetString());
            return FabricOperationTestData.ItemResponse(displayName: name!);
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.Http);
        var loader = Loader(provider, "all");

        var results = await Task.WhenAll(new[] { "first", "second" }.Select(name =>
        {
            var arguments = Arguments();
            arguments["display-name"] = name;
            arguments["options"] = $$"""{"marker":"{{name}}"}""";
            arguments["definition"] = JsonSerializer.Serialize(
                new FabricItemDefinition([new("artifact.content.ipynb", Convert.ToBase64String(Encoding.UTF8.GetBytes(name)), "InlineBase64")], "ipynb"),
                CoreJsonContext.Default.FabricItemDefinition);
            return loader.CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken).AsTask();
        }));

        Assert.All(results, static result => Assert.False(result.IsError));
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo, false)]
    [InlineData("namespace", TransportTypes.StdIo, false)]
    [InlineData("single", TransportTypes.StdIo, false)]
    [InlineData("all", TransportTypes.Http, false)]
    [InlineData("namespace", TransportTypes.Http, false)]
    [InlineData("single", TransportTypes.Http, false)]
    [InlineData("all", TransportTypes.StdIo, true)]
    [InlineData("namespace", TransportTypes.StdIo, true)]
    [InlineData("single", TransportTypes.StdIo, true)]
    [InlineData("all", TransportTypes.Http, true)]
    [InlineData("namespace", TransportTypes.Http, true)]
    [InlineData("single", TransportTypes.Http, true)]
    public async Task RequestProperties_UseOnePostForEitherCreationPathAcrossRoutesAndTransports(
        string route, string transport, bool withDefinition)
    {
        const string workloadJson = """{"futureSetting":{"enabled":false,"values":[1,2.50,null,""]}}""";
        using var expectedWorkload = JsonDocument.Parse(workloadJson);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{FabricOperationTestData.WorkspaceId}/items", request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(withDefinition
                ? ["definition", "displayName", "folderId", "options", "sensitivityLabelSettings", "type"]
                : ["creationPayload", "displayName", "folderId", "sensitivityLabelSettings", "type"],
                body.RootElement.EnumerateObject().Select(static property => property.Name).Order());
            Assert.Equal(FolderId, body.RootElement.GetProperty("folderId").GetString());
            Assert.True(JsonElement.DeepEquals(expectedWorkload.RootElement, body.RootElement.GetProperty(withDefinition ? "options" : "creationPayload")));
            var label = body.RootElement.GetProperty("sensitivityLabelSettings");
            Assert.Equal(LabelId, label.GetProperty("labelId").GetString());
            Assert.Equal("ApplyOrFail", label.GetProperty("sensitivityLabelApplyStrategy").GetString());
            return FabricOperationTestData.ItemResponse();
        });
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, transport, files: files);
        var arguments = Arguments();
        arguments["folder-id"] = "{" + FolderId.ToUpperInvariant() + "}";
        arguments["sensitivity-label-settings"] = $$"""{"labelId":"{{LabelId}}","sensitivityLabelApplyStrategy":"ApplyOrFail"}""";
        arguments[withDefinition ? "options" : "creation-payload"] = workloadJson;
        if (withDefinition)
        {
            arguments["definition"] = InlineDefinition;
        }

        var result = await Loader(provider, route).CallToolHandler(Request(route, arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(FabricOperationTestData.ItemId, Payload(result, route).GetProperty("item").GetProperty("id").GetString());
        Assert.DoesNotContain("futureSetting", Text(result));
        Assert.DoesNotContain("sensitivityLabelSettings", Payload(result, route).GetRawText());
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(files.ReceivedCalls());
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("folder-id", "private-folder-name")]
    [InlineData("folder-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("options", "[]")]
    [InlineData("options", """{"private-setting":1,"private-setting":2}""")]
    [InlineData("sensitivity-label-settings", """{"labelId":"private-invalid-id"}""")]
    [InlineData("sensitivity-label-settings", """{"sensitivityLabelApplyStrategy":"private-invalid-policy"}""")]
    [InlineData("sensitivity-label-settings", """{"sensitivityLabelApplyStrategy":null}""")]
    [InlineData("sensitivity-label-settings", """{"private-unknown-property":true}""")]
    public async Task InvalidProperties_FailBeforeReadingSavedFilesOrAuthenticating(string option, string value)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, files: files);
        var arguments = Arguments();
        arguments["definition-directory"] = "private-host-path";
        arguments["definition-files"] = """[{"partPath":"notebook-content.py"}]""";
        arguments[option] = value;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-", Text(result));
        using var response = JsonDocument.Parse(Text(result));
        Assert.Equal(400, response.RootElement.GetProperty("status").GetInt32());
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("private-invalid-json")]
    [InlineData("""{"private-setting":1,"private-setting":2}""")]
    public async Task InvalidCreationPayload_DoesNotFallBackToCreationWithoutPayload(string json)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http);
        var arguments = Arguments();
        arguments["creation-payload"] = json;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("--creation-payload", Text(result));
        Assert.DoesNotContain("private-", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("definition", InlineDefinition)]
    [InlineData("definition-directory", "private-host-path")]
    [InlineData("definition-files", """[{"partPath":"notebook-content.py"}]""")]
    [InlineData("definition-format", "FabricGitSource")]
    public async Task CreationPayload_ConflictsWithEveryDefinitionOptionBeforeFileAccess(string option, string value)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        var files = Substitute.For<IFabricItemDefinitionFileService>();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, files: files);
        var arguments = Arguments();
        arguments["creation-payload"] = "{}";
        arguments[option] = value;

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("--creation-payload cannot be combined", Text(result));
        Assert.Empty(files.ReceivedCalls());
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WorkloadOptions_RequireADefinitionEvenWhenCreationPayloadIsPresent(bool withPayload)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http);
        var arguments = Arguments();
        arguments["options"] = "{}";
        if (withPayload)
        {
            arguments["creation-payload"] = "{}";
        }

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("--options requires", Text(result));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task WorkloadOptions_DoNotEnableLroWaiting()
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.True(body.RootElement.GetProperty("options").GetProperty("sync").GetBoolean());
            return FabricOperationTestData.Accepted();
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.Http);
        var arguments = Arguments();
        arguments["definition"] = InlineDefinition;
        arguments["options"] = """{"sync":true}""";

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal("Accepted", Payload(result, "all").GetProperty("operation").GetProperty("status").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.BadRequest)]
    [InlineData("ApplyOrFail", HttpStatusCode.BadRequest)]
    [InlineData("Ignore", HttpStatusCode.BadRequest)]
    [InlineData(null, HttpStatusCode.Forbidden)]
    [InlineData("ApplyOrFail", HttpStatusCode.Forbidden)]
    [InlineData("Ignore", HttpStatusCode.Forbidden)]
    public async Task LabelFailures_DoNotDowngradeRequestedPolicyOrResubmit(string? strategy, HttpStatusCode status)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var settings = body.RootElement.GetProperty("sensitivityLabelSettings");
            Assert.Equal(LabelId, settings.GetProperty("labelId").GetString());
            Assert.Equal(strategy is not null, settings.TryGetProperty("sensitivityLabelApplyStrategy", out var value));
            if (strategy is not null)
            {
                Assert.Equal(strategy, value.GetString());
            }
            return FabricOperationTestData.JsonResponse("""{"message":"private-backend-label-failure"}""", status);
        });
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http);
        var arguments = Arguments();
        arguments["sensitivity-label-settings"] = JsonSerializer.Serialize(
            new FabricSensitivityLabelSettings(Guid.Parse(LabelId), strategy), CoreJsonContext.Default.FabricSensitivityLabelSettings);

        var result = await Loader(provider, "all").CallToolHandler(Request("all", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        using var response = JsonDocument.Parse(Text(result));
        Assert.Equal((int)status, response.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain("private-", Text(result));
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--folder-id", FolderId)]
    [InlineData("--creation-payload", "{}")]
    [InlineData("--options", "{}")]
    [InlineData("--sensitivity-label-settings", "{}")]
    public async Task RepeatedNewCliOptions_AreRejectedRatherThanSelectingOne(string option, string value)
    {
        using var handler = NoHttp();
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo);
        var command = provider.GetRequiredService<ItemCreateCommand>();
        IBaseCommand executable = command;

        var response = await executable.ExecuteAsync(new(), command.GetCommand().Parse(
            ["--workspace-id", FabricOperationTestData.WorkspaceId, "--display-name", "Created", "--item-type", "Notebook",
                option, value, option, value]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport,
        StructuredOutputMode? output = StructuredOutputMode.Compact, IFabricItemDefinitionFileService? files = null)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        if (files is not null)
        {
            services.AddSingleton(files);
        }
        services.AddSingleton<IAreaSetup>(setup);
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(OptionsFactory.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Microsoft Fabric MCP Server",
            Version = "1.0.0",
            Description = "Offline definition input tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(OptionsFactory.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            Transport = transport,
            StructuredOutputMode = output
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<NamespaceToolLoader>();
        services.AddSingleton<SingleProxyToolLoader>();
        return services.BuildServiceProvider();
    }

    private static IToolLoader Loader(IServiceProvider provider, string route) => route switch
    {
        "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
        "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
        "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
        _ => throw new ArgumentOutOfRangeException(nameof(route))
    };

    private static RequestContext<CallToolRequestParams> Request(string route, Dictionary<string, object?> arguments)
    {
        if (route == "all")
        {
            return McpTestUtilities.CreateToolCallRequest(ToolName, arguments);
        }
        var parameters = new Dictionary<string, object?>
        {
            ["intent"] = "Create an item from the supplied definition",
            ["command"] = ToolName,
            ["parameters"] = arguments
        };
        if (route == "single")
        {
            parameters["tool"] = "core";
        }
        return McpTestUtilities.CreateToolCallRequest(route == "namespace" ? "core" : "fabric", parameters);
    }

    private static Dictionary<string, object?> Arguments() => new()
    {
        ["workspace-id"] = FabricOperationTestData.WorkspaceId,
        ["display-name"] = "Created",
        ["item-type"] = "Notebook"
    };

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } structured)
        {
            return route == "namespace" ? structured.GetProperty("result") : structured;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }

    private static FabricCoreHttpMessageHandler NoHttp() =>
        new((_, _) => throw new InvalidOperationException("No HTTP request is expected."));
}
