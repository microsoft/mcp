// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CopilotCliTester.Models;
using Xunit;

namespace CopilotCliTester.Tests;

public sealed class AgentRunnerUtilsTests : IDisposable
{
    private static AgentMetadata CreateMetadata(params AgentSessionEvent[] events)
    {
        var metadata = new AgentMetadata();
        metadata.Events.AddRange(events);
        return metadata;
    }

    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"reporoot_test_{Guid.NewGuid():N}");

    public AgentRunnerUtilsTests()
    {
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static AgentSessionEvent ToolEvent(string toolName, Dictionary<string, object?>? extraData = null)
    {
        var data = new Dictionary<string, object?> { ["toolName"] = toolName };
        if (extraData is not null)
        {
            foreach (var kv in extraData)
                data[kv.Key] = kv.Value;
        }
        return new AgentSessionEvent { Type = "tool.execution_start", Data = data };
    }

    #region GetToolCalls

    [Fact]
    public void GetToolCalls_FiltersToolExecutionStartEvents()
    {
        var metadata = CreateMetadata(
            ToolEvent("storage_account_list"),
            new AgentSessionEvent { Type = "message", Data = new() { ["toolName"] = "irrelevant" } },
            ToolEvent("keyvault_secret_get"));

        var result = AgentRunnerUtils.GetToolCalls(metadata);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetToolCalls_ExcludesIgnoredTools()
    {
        var metadata = CreateMetadata(
            ToolEvent("report_intent"),
            ToolEvent("storage_account_list"));

        var result = AgentRunnerUtils.GetToolCalls(metadata);

        Assert.Single(result);
        Assert.Equal("storage_account_list", result[0].Data["toolName"]?.ToString());
    }

    [Fact]
    public void GetToolCalls_ExcludesEventsWithNullToolName()
    {
        var metadata = CreateMetadata(
            new AgentSessionEvent { Type = "tool.execution_start", Data = new() { ["toolName"] = null } },
            ToolEvent("real_tool"));

        var result = AgentRunnerUtils.GetToolCalls(metadata);

        Assert.Single(result);
    }

    [Fact]
    public void GetToolCalls_ExcludesEventsWithEmptyToolName()
    {
        var metadata = CreateMetadata(
            new AgentSessionEvent { Type = "tool.execution_start", Data = new() { ["toolName"] = "  " } },
            ToolEvent("real_tool"));

        var result = AgentRunnerUtils.GetToolCalls(metadata);

        Assert.Single(result);
    }

    [Fact]
    public void GetToolCalls_EmptyMetadata_ReturnsEmpty()
    {
        var metadata = new AgentMetadata();

        var result = AgentRunnerUtils.GetToolCalls(metadata);

        Assert.Empty(result);
    }

    #endregion

    #region WasToolInvoked

    [Theory]
    [InlineData("storage_account_list", "storage_account_list", true)]
    [InlineData("Storage_Account_List", "storage_account_list", true)]
    [InlineData("azure-storage_account_list", "storage_account_list", true)]
    [InlineData("azure-storage-account_list", "storage-account_list", true)]
    [InlineData("azure-storage-account_list", "account_list", false)]
    [InlineData("keyvault_secret_get", "storage_account_list", false)]
    public void WasToolInvoked_VariousMatches(string invokedToolName, string queryToolName, bool expected)
    {
        var metadata = CreateMetadata(ToolEvent(invokedToolName));

        var result = AgentRunnerUtils.WasToolInvoked(metadata, queryToolName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void WasToolInvoked_EmptyMetadata_ReturnsFalse()
    {
        var metadata = new AgentMetadata();

        Assert.False(AgentRunnerUtils.WasToolInvoked(metadata, "anything"));
    }

    #endregion

    #region ResolveToolName

    [Fact]
    public void GetInvokedToolNames_ResolvesFromArgumentsCommand_Dictionary()
    {
        var argsDict = new Dictionary<string, object?> { ["command"] = "real_command" };
        var evt = ToolEvent("wrapper_tool", new() { ["arguments"] = argsDict });
        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("real_command", result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_ResolvesFromArgumentsCommand_JsonString()
    {
        var jsonArgs = """{"command": "resolved_tool"}""";
        var evt = ToolEvent("wrapper", new() { ["arguments"] = jsonArgs });
        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("resolved_tool", result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_ResolvesFromArgumentsCommand_JsonElement()
    {
        var doc = JsonDocument.Parse("""{"command": "element_tool"}""");
        var evt = ToolEvent("wrapper", new() { ["arguments"] = doc.RootElement.Clone() });
        doc.Dispose();
        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("element_tool", result[0]);
    }

    [Theory]
    [InlineData("mcp_resolved", "some_tool", "mcp_resolved")]
    [InlineData(null, "final_fallback", "final_fallback")]
    public void GetInvokedToolNames_FallbackBehavior_ReturnsExpected(string? mcpToolName, string? toolName, string expected)
    {
        var data = new Dictionary<string, object?>();

        if (mcpToolName is not null)
            data["mcpToolName"] = mcpToolName;

        if (toolName is not null)
            data["toolName"] = toolName;

        var evt = new AgentSessionEvent
        {
            Type = "tool.execution_start",
            Data = data
        };

        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal(expected, result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_FallsBackToToolName_WhenNoArgumentsOrMcpToolName()
    {
        var evt = new AgentSessionEvent
        {
            Type = "tool.execution_start",
            Data = new() { ["toolName"] = "has_name" }
        };

        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("has_name", result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_ArgumentsCommandTakesPriorityOverMcpToolName()
    {
        var argsDict = new Dictionary<string, object?> { ["command"] = "from_args" };
        var evt = ToolEvent("fallback", new()
        {
            ["arguments"] = argsDict,
            ["mcpToolName"] = "from_mcp"
        });
        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("from_args", result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_McpToolNameTakesPriorityOverToolName()
    {
        // No arguments key, so falls to mcpToolName before toolName
        var evt = new AgentSessionEvent
        {
            Type = "tool.execution_start",
            Data = new()
            {
                ["toolName"] = "basic_name",
                ["mcpToolName"] = "mcp_name"
            }
        };
        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("mcp_name", result[0]);
    }

    [Fact]
    public void GetInvokedToolNames_UsesCorrectPrecedence_ArgumentsOverMcpOverTool()
    {
        var argsDict = new Dictionary<string, object?>
        {
            ["command"] = "from_arguments"
        };

        var evt = new AgentSessionEvent
        {
            Type = "tool.execution_start",
            Data = new()
            {
                ["toolName"] = "from_tool",
                ["mcpToolName"] = "from_mcp",
                ["arguments"] = argsDict
            }
        };

        var metadata = CreateMetadata(evt);

        var result = AgentRunnerUtils.GetInvokedToolNames(metadata);

        Assert.Single(result);
        Assert.Equal("from_arguments", result[0]);
    }

    #endregion

    #region SafeJson

    [Theory]
    [InlineData("hello", "\"hello\"")]
    [InlineData(null, "null")]
    [InlineData(42, "42")]
    public void SafeJson_ReturnsExpectedSerialization(object? input, string expected)
    {
        var result = AgentRunnerUtils.SafeJson(input);

        Assert.Equal(expected, result);
    }

    #endregion

    #region WasToolInvoked with ResolveToolName paths

    [Fact]
    public void WasToolInvoked_MatchesViaArgumentsCommand()
    {
        var argsDict = new Dictionary<string, object?> { ["command"] = "storage_account_list" };
        var evt = ToolEvent("namespace_proxy", new() { ["arguments"] = argsDict });
        var metadata = CreateMetadata(evt);

        Assert.True(AgentRunnerUtils.WasToolInvoked(metadata, "storage_account_list"));
    }

    [Fact]
    public void WasToolInvoked_MatchesViaMcpToolName()
    {
        var evt = ToolEvent("wrapper", new() { ["mcpToolName"] = "keyvault_secret_get" });
        var metadata = CreateMetadata(evt);

        Assert.True(AgentRunnerUtils.WasToolInvoked(metadata, "keyvault_secret_get"));
    }

    [Fact]
    public void WasToolInvoked_NamespacePrefixViaArgumentsCommand()
    {
        var argsDict = new Dictionary<string, object?> { ["command"] = "azure-storage_account_list" };
        var evt = ToolEvent("proxy", new() { ["arguments"] = argsDict });
        var metadata = CreateMetadata(evt);

        Assert.True(AgentRunnerUtils.WasToolInvoked(metadata, "storage_account_list"));
    }

    #endregion

    #region FindRepoRoot

    [Theory]
    [InlineData("Microsoft.Mcp.slnx")]
    [InlineData("mcp.sln")]
    public void FindRepoRoot_FindsSentinelFiles(string sentinelName)
    {
        File.WriteAllText(Path.Combine(_tempRoot, sentinelName), "");

        var nested = Path.Combine(_tempRoot, "eng", "tools", "deep");
        Directory.CreateDirectory(nested);

        var result = AgentRunnerUtils.FindRepoRoot(nested);

        Assert.Equal(_tempRoot, result);
    }

    [Fact]
    public void FindRepoRoot_FindsFromSameDirectory()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "Microsoft.Mcp.slnx"), "");

        var result = AgentRunnerUtils.FindRepoRoot(_tempRoot);

        Assert.Equal(_tempRoot, result);
    }

    [Fact]
    public void FindRepoRoot_ThrowsWhenNoSentinelFile()
    {
        // Empty temp dir with no sentinel files - walk will eventually hit root without finding one
        var nested = Path.Combine(_tempRoot, "a", "b", "c");
        Directory.CreateDirectory(nested);

        Assert.Throws<InvalidOperationException>(() => AgentRunnerUtils.FindRepoRoot(nested));
    }

    [Fact]
    public void FindRepoRoot_PrefersFirstMatchWalkingUp()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "Microsoft.Mcp.slnx"), "");

        var mid = Path.Combine(_tempRoot, "mid");
        Directory.CreateDirectory(mid);
        File.WriteAllText(Path.Combine(mid, "Microsoft.Mcp.slnx"), "");

        var deep = Path.Combine(mid, "deep");
        Directory.CreateDirectory(deep);

        var result = AgentRunnerUtils.FindRepoRoot(deep);

        Assert.Equal(mid, result);
    }

    #endregion
}
