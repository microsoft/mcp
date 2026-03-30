// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class PluginTelemetryCommandTests
{
    private readonly IPluginFileReferenceAllowlistProvider _fileReferenceProvider;
    private readonly IPluginSkillNameAllowlistProvider _skillNameProvider;
    private readonly ICommandFactory _commandFactory;
    private readonly PluginTelemetryCommand _command;

    public PluginTelemetryCommandTests()
    {
        var serverAssembly = typeof(Azure.Mcp.Server.Program).Assembly;

        _fileReferenceProvider = new ResourcePluginFileReferenceAllowlistProvider(
            NullLogger<ResourcePluginFileReferenceAllowlistProvider>.Instance,
            serverAssembly,
            "allowed-plugin-file-references.json");

        _skillNameProvider = new ResourcePluginSkillNameAllowlistProvider(
            NullLogger<ResourcePluginSkillNameAllowlistProvider>.Instance,
            serverAssembly,
            "allowed-skill-names.json");

        // Build a real CommandFactory with ServerSetup to get actual registered commands
        var services = new ServiceCollection();
        services.AddSingleton<IAreaSetup>(new ServerSetup());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "azmcp",
            Name = "Azure.Mcp.Server.Test",
            DisplayName = "Azure MCP Server (Test)",
            Version = "1.0.0-test"
        }));
        services.AddSingleton<ITelemetryService>(Substitute.For<ITelemetryService>());
        services.AddSingleton<ILogger<CommandFactory>>(NullLogger<CommandFactory>.Instance);
        services.AddSingleton<ILogger<ServiceInfoCommand>>(NullLogger<ServiceInfoCommand>.Instance);
        services.AddSingleton(_fileReferenceProvider);
        services.AddSingleton(_skillNameProvider);

        // Register area services (ServerSetup registers its commands here)
        new ServerSetup().ConfigureServices(services);

        services.AddSingleton<ICommandFactory, CommandFactory>();

        var serviceProvider = services.BuildServiceProvider();
        _commandFactory = serviceProvider.GetRequiredService<ICommandFactory>();

        _command = new PluginTelemetryCommand(
            _fileReferenceProvider,
            _skillNameProvider,
            serviceProvider);
    }

    [Theory]
    [InlineData("azure-storage", true)]
    [InlineData("azure-ai", true)]
    [InlineData("microsoft-foundry", true)]
    [InlineData("custom-skill", false)]
    [InlineData("azure-mcp-skill", false)]
    public void SkillNameProvider_ValidatesSkillNamesCorrectly(string skillName, bool shouldBeAllowed)
    {
        var result = _skillNameProvider.IsSkillNameAllowed(skillName);
        Assert.Equal(shouldBeAllowed, result);
    }

    [Theory]
    [InlineData("azure-ai\\references\\auth-best-practices.md", true)]
    [InlineData("custom/file/path.ts", false)]
    public void FileReferenceProvider_ValidatesFileReferencesCorrectly(string fileReference, bool shouldBeAllowed)
    {
        var result = _fileReferenceProvider.IsPathAllowed(fileReference);
        Assert.Equal(shouldBeAllowed, result);
    }

    [Theory]
    [InlineData("server_start", true)]
    [InlineData("server_info", true)]
    [InlineData("server_plugin-telemetry", true)]
    [InlineData("invalid-tool-name", false)]
    [InlineData("storage_account-create", false)] // not registered in ServerSetup-only factory
    public void CommandFactory_ValidatesToolNamesCorrectly(string toolName, bool shouldBeAllowed)
    {
        var result = _commandFactory.FindCommandByName(toolName);

        if (shouldBeAllowed)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void SkillNameProvider_RejectsNullOrEmptySkillNames(string? skillName, bool expected)
    {
        var result = _skillNameProvider.IsSkillNameAllowed(skillName!);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void FileReferenceProvider_RejectsNullOrEmptyPaths(string? filePath, bool expected)
    {
        var result = _fileReferenceProvider.IsPathAllowed(filePath!);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("AZURE-STORAGE", false)]
    [InlineData("Azure-Storage", false)]
    [InlineData("azure-storage", true)]
    public void SkillNameProvider_IsCaseSensitive(string skillName, bool expected)
    {
        var result = _skillNameProvider.IsSkillNameAllowed(skillName);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Command_ConstructorSucceeds_WithProviders()
    {
        Assert.NotNull(_command);
        Assert.Equal("plugin-telemetry", _command.Name);
        Assert.Equal("Plugin Telemetry", _command.Title);
    }

    [Fact]
    public void CommandFactory_RegistersExpectedServerCommands()
    {
        var allCommands = _commandFactory.AllCommands;
        Assert.Contains("server_start", allCommands.Keys);
        Assert.Contains("server_info", allCommands.Keys);
        Assert.Contains("server_plugin-telemetry", allCommands.Keys);
        Assert.True(allCommands.Count >= 3);
    }

    [Theory]
    // Claude Code prefix
    [InlineData("mcp__plugin_azure_azure__pricing", "pricing")]
    [InlineData("mcp__plugin_azure_azure__group_resource_list", "group_resource_list")]
    // VS Code prefix
    [InlineData("mcp_azure_mcp_monitor", "monitor")]
    [InlineData("mcp_azure_mcp_subscription_list", "subscription_list")]
    // Copilot CLI prefix
    [InlineData("azure-documentation", "documentation")]
    [InlineData("azure-group_resource_list", "group_resource_list")]
    [InlineData("azure-monitor_workspace_list", "monitor_workspace_list")]
    // No prefix - returned as-is
    [InlineData("pricing", "pricing")]
    [InlineData("server_start", "server_start")]
    // Unknown prefix - returned as-is (validation catches these later)
    [InlineData("unknown_tool", "unknown_tool")]
    [InlineData("mcp_azure_documentation", "mcp_azure_documentation")]
    public void StripClientPrefix_RemovesKnownPrefixes(string rawToolName, string expected)
    {
        var result = PluginTelemetryCommand.StripClientPrefix(rawToolName);
        Assert.Equal(expected, result);
    }

    [Theory]
    // Area-level matches (server area exists in our test factory)
    [InlineData("mcp__plugin_azure_azure__server", "server")]
    [InlineData("mcp_azure_mcp_server", "server")]
    [InlineData("azure-server", "server")]
    // Exact command matches
    [InlineData("server_start", "server_start")]
    [InlineData("mcp__plugin_azure_azure__server_info", "server_info")]
    // Full command path with prefix — proves exact match after stripping (same path monitor_workspace_list takes)
    [InlineData("azure-server_start", "server_start")]
    [InlineData("mcp_azure_mcp_server_start", "server_start")]
    [InlineData("mcp__plugin_azure_azure__server_plugin-telemetry", "server_plugin-telemetry")]
    // Allowlisted Azure extension tools (pass through as-is)
    [InlineData("azure_auth-set_auth_context", "azure_auth-set_auth_context")]
    [InlineData("azure_get_auth_context", "azure_get_auth_context")]
    [InlineData("azure_query_azure_resource_graph", "azure_query_azure_resource_graph")]
    [InlineData("azure_recommend_custom_modes", "azure_recommend_custom_modes")]
    [InlineData("azure_get_dotnet_template_tags", "azure_get_dotnet_template_tags")]
    [InlineData("azure_get_dotnet_templates_for_tag", "azure_get_dotnet_templates_for_tag")]
    // Invalid - no matching command or area
    [InlineData("mcp__plugin_azure_azure__nonexistent", null)]
    [InlineData("azure-fakeTool", null)]
    [InlineData("completely_unknown", null)]
    // Non-standard VS Code prefixes are rejected (not stripped)
    [InlineData("mcp_azure_documentation", null)]
    public void ValidateAndNormalizeToolName_ValidatesAndNormalizes(string rawToolName, string? expected)
    {
        var result = PluginTelemetryCommand.ValidateAndNormalizeToolName(rawToolName, _commandFactory);
        Assert.Equal(expected, result);
    }
}
