// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

public sealed class EnumParameterMigrationTests()
{
    [Theory]
    [InlineData("managedlustre_fs_blob_autoimport_create", "conflict-resolution-mode", false, "Fail", "Skip", "OverwriteIfDirty", "OverwriteAlways")]
    [InlineData("managedlustre_fs_blob_autoimport_create", "admin-status", false, "Enable", "Disable")]
    [InlineData("resiliency_recoveryplan_validateforfailover", "user-consent", false, "Unspecified", "Allowed")]
    [InlineData("deploy_iac_rules_get", "deployment-tool", true, "AzCli", "AZD")]
    [InlineData("managedlustre_fs_update", "maintenance-day", false, "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday")]
    [InlineData("managedlustre_fs_update", "root-squash-mode", false, "All", "RootOnly", "None")]
    [InlineData("resiliency_drill_start", "mode", true, "Failover", "TestFailover")]
    public async Task EnumParameters_AgreeAcrossDiscoveryAndRejectMalformedCalls(
        string toolName, string parameter, bool required, params string[] expectedValues)
    {
        await using var services = Assert.IsType<ServiceProvider>(CommandFactoryHelpers.CreateDefaultServiceProvider());
        var factory = CommandFactoryHelpers.CreateCommandFactory(services);
        var command = Assert.IsAssignableFrom<IBaseCommand>(factory.FindCommandByName(toolName));
        var loader = new CommandFactoryToolLoader(
            new CommandContextAccessor(),
            factory,
            Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Tool = [toolName] }),
            NullLogger<CommandFactoryToolLoader>.Instance);
        var listed = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(listed.Tools);
        var schema = tool.InputSchema;
        var property = schema.GetProperty("properties").GetProperty(parameter);

        Assert.Equal("string", property.GetProperty("type").GetString());
        Assert.Equal(expectedValues, property.GetProperty("enum").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(required, schema.GetProperty("required").EnumerateArray().Any(value => value.GetString() == parameter));

        var childTool = BaseToolLoader.CreateToolFromCommand(toolName, command);
        Assert.Equal(schema.GetRawText(), childTool.InputSchema.GetRawText());

        using var learn = JsonDocument.Parse(factory.GetLearnResponse([.. toolName.Split('_'), "--learn"]));
        var learnOption = learn.RootElement.GetProperty("results")[0].GetProperty("option")
            .EnumerateArray().Single(option => option.GetProperty("name").GetString() == $"--{parameter}");
        Assert.Equal(expectedValues, learnOption.GetProperty("enum").EnumerateArray().Select(value => value.GetString()));

        var arguments = schema.GetProperty("required").EnumerateArray().ToDictionary(
            value => value.GetString()!,
            value =>
            {
                var requiredProperty = schema.GetProperty("properties").GetProperty(value.GetString()!);
                return JsonString(requiredProperty.TryGetProperty("enum", out var values)
                    ? values[0].GetString()!
                    : "example");
            });

        foreach (var value in expectedValues)
        {
            arguments[parameter] = JsonString(value);
            Assert.True(command.GetCommand().TryParseFromDictionary(arguments, out var parsed, out var error), error);
            Assert.NotNull(parsed);
            Assert.Empty(parsed.Errors);
            arguments[parameter] = JsonString(value.ToLowerInvariant());
            Assert.True(command.GetCommand().TryParseFromDictionary(arguments, out parsed, out error), error);
            Assert.NotNull(parsed);
            Assert.Empty(parsed.Errors);
        }

        foreach (var value in new[] { "not-an-option", "0", "999", "a,b", "", " " })
        {
            arguments[parameter] = JsonString(value);
            Assert.True(command.GetCommand().TryParseFromDictionary(arguments, out var parsed, out var error), error);
            Assert.NotNull(parsed);
            Assert.Contains(parsed.Errors, parseError => parseError.Message.Contains($"--{parameter}", StringComparison.Ordinal));
        }
    }

    private static JsonElement JsonString(string value)
    {
        using var document = JsonDocument.Parse(JsonValue.Create(value)!.ToJsonString());
        return document.RootElement.Clone();
    }
}
