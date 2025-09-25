// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.Areas.Server;

/// <summary>
/// Live integration tests for Azure MCP Server that validate tool loading behavior across different modes.
/// These tests start actual MCP server instances and verify the correct tools are loaded.
/// </summary>
public class ServerCommandTests(ITestOutputHelper output)
{
    protected ITestOutputHelper Output { get; } = output;

    private async Task<IMcpClient> CreateClientAsync(params string[] arguments)
    {
        var settingsFixture = new LiveTestSettingsFixture();
        await settingsFixture.InitializeAsync();
        var settings = settingsFixture.Settings;

        string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments,
            StandardErrorLines = line =>
            {
                // Safely capture stderr lines without accessing test context from background thread
                try
                { Output.WriteLine($"[MCP Server] {line}"); }
                catch { /* Ignore if test context unavailable */ }
            }
        };

        if (!string.IsNullOrEmpty(settings.TestPackage))
        {
            Environment.CurrentDirectory = settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", settings.TestPackage, .. arguments];
        }

        var clientTransport = new StdioClientTransport(transportOptions);
        return await McpClientFactory.CreateAsync(clientTransport);
    }

    #region Default Mode Tests

    [Fact]
    public async Task DefaultMode_LoadsNamespaceTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        // Should have tools from multiple areas
        var toolNames = listResult.Select(t => t.Name).ToList();

        // Default mode is now namespace mode, so should have namespace-level tools (not 60+ individual tools)
        Assert.True(toolNames.Count > 20, $"Expected more than 20 namespace tools, got {toolNames.Count}");

        // Should include the documentation tool
        Assert.Contains("documentation", toolNames, StringComparer.OrdinalIgnoreCase);

        // Log for debugging
        Output.WriteLine($"Default mode (namespace) loaded {toolNames.Count} tools");
        foreach (var name in toolNames)
        {
            Output.WriteLine($"  - {name}");
        }
    }

    #endregion

    #region All Mode Tests

    [Fact]
    public async Task AllMode_LoadsAllIndividualTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "all");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        // Should have tools from multiple areas
        var toolNames = listResult.Select(t => t.Name).ToList();

        // Should include Azure service tools and extension tools (all individual tools)
        Assert.True(toolNames.Count > 60, $"Expected more than 60 individual tools, got {toolNames.Count}");

        // Should include the microsoft_docs_search tool
        Assert.Contains("microsoft_docs_search", toolNames, StringComparer.OrdinalIgnoreCase);

        // Log for debugging
        Output.WriteLine($"All mode loaded {toolNames.Count} tools");
        foreach (var name in toolNames)
        {
            Output.WriteLine($"  - {name}");
        }
    }

    #endregion

    #region Single Tool Proxy Mode Tests

    [Fact]
    public async Task SingleProxyMode_LoadsSingleAzureTool()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "single");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(listResult);

        var tool = listResult.First();
        Assert.Equal("azure", tool.Name);
        Assert.Contains("azure", tool.Description, StringComparison.OrdinalIgnoreCase);

        Output.WriteLine($"Single proxy mode loaded 1 tool: {tool.Name}");
        Output.WriteLine($"Description: {tool.Description}");
    }

    [Fact]
    public async Task SingleProxyMode_WithNamespaceFilter_StillLoadsSingleAzureTool()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "single", "--namespace", "storage");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // Single proxy mode should still expose single azure tool regardless of namespace filter
        Assert.Single(listResult);
        Assert.Equal("azure", listResult.First().Name);

        Output.WriteLine("Single proxy mode with namespace filter still loaded 1 tool");
    }

    [Fact]
    public async Task SingleProxyMode_WithReadOnlyFlag_LoadsSingleAzureTool()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "single", "--read-only");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(listResult);
        Assert.Equal("azure", listResult.First().Name);

        // Verify read-only behavior for single proxy mode
        var tool = listResult.First();
        var readOnlyHint = tool.ProtocolTool?.Annotations?.ReadOnlyHint;

        // The azure tool should have read-only hint when in read-only mode
        Output.WriteLine($"Single proxy read-only tool ReadOnlyHint: {readOnlyHint}");
        Output.WriteLine($"Tool annotations: {tool.ProtocolTool?.Annotations != null}");

        // For single proxy mode, the ReadOnlyHint may be null because it's a proxy tool
        // but we should still have annotations present
        Assert.NotNull(tool.ProtocolTool?.Annotations);

        Output.WriteLine("Single proxy read-only mode loaded 1 tool");
    }

    #endregion

    #region Namespace Proxy Mode Tests

    [Fact]
    public async Task NamespaceProxyMode_LoadsNamespaceTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "namespace");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        var toolNames = listResult.Select(t => t.Name).ToList();

        // In namespace mode without specific namespaces, should default to extension tools
        Assert.True(toolNames.Count > 20, "Should have more than 20 tools in namespace mode");

        // Should include the documentation tool
        Assert.Contains("documentation", toolNames, StringComparer.OrdinalIgnoreCase);

        Output.WriteLine($"Namespace proxy mode loaded {toolNames.Count} tools");
        foreach (var name in toolNames)
        {
            Output.WriteLine($"  - {name}");
        }
    }

    [Fact]
    public async Task NamespaceProxyMode_WithSpecificNamespaces_LoadsNamespaceSpecificTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "namespace", "--namespace", "storage", "--namespace", "keyvault");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        var toolNames = listResult.Select(t => t.Name).ToList();

        // Should include namespace-specific tools
        var hasRelevantTools = toolNames.Any(name =>
            name.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("keyvault", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasRelevantTools, "Should have tools related to specified namespaces");

        // Should not include documentation tool when explicit namespaces are specified
        Assert.DoesNotContain("documentation", toolNames, StringComparer.OrdinalIgnoreCase);

        // Should contain exactly 2 tools for the specified namespaces
        Assert.Equal(2, toolNames.Count);

        // Verify tools are exactly from storage and keyvault namespaces
        Assert.All(toolNames, toolName =>
        {
            var isStorageOrKeyVault = toolName.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
                                    toolName.Contains("keyvault", StringComparison.OrdinalIgnoreCase);
            Assert.True(isStorageOrKeyVault, $"Tool '{toolName}' should be related to storage or keyvault namespaces");
        });

        Output.WriteLine($"Namespace proxy mode with [storage, keyvault] loaded {toolNames.Count} tools");
    }

    [Fact]
    public async Task NamespaceProxyMode_WithDocumentationNamespace_LoadsOnlyDocumentationTool()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "namespace", "--namespace", "documentation");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        var toolNames = listResult.Select(t => t.Name).ToList();

        // Should contain only the documentation tool
        Assert.Single(listResult);
        Assert.Contains("documentation", toolNames, StringComparer.OrdinalIgnoreCase);

        // Verify it's exactly the documentation tool
        Assert.Equal("documentation", toolNames.First(), StringComparer.OrdinalIgnoreCase);

        Output.WriteLine($"Namespace proxy mode with [documentation] loaded {toolNames.Count} tools");
        Output.WriteLine($"Tool: {toolNames.First()}");
    }

    [Fact]
    public async Task NamespaceProxyMode_StorageToolLearnMode_ReturnsStorageCommands()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "namespace");

        // Act - Call storage tool in learn mode
        var learnParameters = new Dictionary<string, object?>
        {
            ["learn"] = true
        };

        var result = await client.CallToolAsync("storage", learnParameters, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Get the text content
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent.Text);

        var responseText = textContent.Text;

        // Verify the response contains information about storage commands
        Assert.Contains("available command", responseText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("storage", responseText, StringComparison.OrdinalIgnoreCase);

        // Verify it contains specific storage commands we expect
        Assert.Contains("storage_account_get", responseText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("storage_blob_container_get", responseText, StringComparison.OrdinalIgnoreCase);

        Output.WriteLine("Storage tool learn mode response:");
        Output.WriteLine(responseText);
        Output.WriteLine($"✓ Learn mode returned {responseText.Length} characters of storage command information");
    }

    #endregion

    #region Default Mode with Filters Tests

    [Fact]
    public async Task DefaultMode_WithNamespaceFilter_LoadsFilteredTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--namespace", "storage", "--namespace", "keyvault");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);
        Assert.Equal(2, listResult.Count());

        var toolNames = listResult.Select(t => t.Name).ToList();

        // Should only include tools from specified namespaces
        var hasStorageOrKeyVault = toolNames.Any(name =>
            name.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("keyvault", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasStorageOrKeyVault, "Should have tools related to storage or keyvault namespaces");

        Output.WriteLine($"Default mode with namespaces [storage, keyvault] loaded {toolNames.Count} tools");
        foreach (var name in toolNames)
        {
            Output.WriteLine($"  - {name}");
        }
    }

    [Fact]
    public async Task AllMode_WithNamespaceFilter_LoadsFilteredIndividualTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "all", "--namespace", "storage", "--namespace", "keyvault");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        var toolNames = listResult.Select(t => t.Name).ToList();

        // Should only include individual tools from specified namespaces
        var hasStorageOrKeyVault = toolNames.Any(name =>
            name.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("keyvault", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasStorageOrKeyVault, "Should have tools related to storage or keyvault namespaces");

        // In all mode with namespace filter, should have more individual tools than namespace mode
        Assert.True(toolNames.Count > 2, $"Expected more than 2 individual tools, got {toolNames.Count}");

        Output.WriteLine($"All mode with namespaces [storage, keyvault] loaded {toolNames.Count} tools");
    }

    [Fact]
    public async Task AllMode_WithReadOnlyFlag_LoadsOnlyReadOnlyTools()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "all", "--read-only");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(listResult);

        // All tools should be read-only or the count should be reduced
        var toolCount = listResult.Count();
        Assert.True(toolCount > 0, "Should have at least some read-only tools");

        // Verify tools have read-only annotations
        var toolsWithReadOnlyHint = 0;
        var toolsWithAnnotations = 0;

        foreach (var tool in listResult)
        {
            var hasAnnotations = tool.ProtocolTool?.Annotations != null;
            var readOnlyHint = tool.ProtocolTool?.Annotations?.ReadOnlyHint;

            if (hasAnnotations)
            {
                toolsWithAnnotations++;
            }

            if (readOnlyHint.HasValue && readOnlyHint.Value)
            {
                toolsWithReadOnlyHint++;
            }

            Output.WriteLine($"Tool: {tool.Name} - HasAnnotations: {hasAnnotations}, ReadOnlyHint: {readOnlyHint}");
        }

        Output.WriteLine($"Tools with annotations: {toolsWithAnnotations}/{toolCount}");
        Output.WriteLine($"Tools with ReadOnlyHint=true: {toolsWithReadOnlyHint}/{toolCount}");

        // In read-only mode, ALL tools must have annotations and ReadOnlyHint=true
        Assert.Equal(toolCount, toolsWithAnnotations);
        Assert.Equal(toolCount, toolsWithReadOnlyHint);

        // Additional verification message
        Output.WriteLine("✓ All tools have annotations and ReadOnlyHint=true as expected in read-only mode");

        Output.WriteLine($"Default read-only mode loaded {toolCount} tools");
    }

    #endregion

    #region Negative Tests - Invalid Modes and Namespaces

    [Fact]
    public async Task InvalidMode_FailsToStartServer()
    {
        // Act & Assert
        await Assert.ThrowsAsync<IOException>(async () =>
        {
            await using var client = await CreateClientAsync("server", "start", "--mode", "invalid-mode");
        });
    }

    [Fact]
    public async Task InvalidNamespace_LoadsGracefully()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--namespace", "invalid-namespace", "--namespace", "another-invalid");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // Should not crash, but may have fewer or no tools
        Assert.NotNull(listResult);

        // Invalid namespaces should result in 0 tools
        Assert.Empty(listResult);

        Output.WriteLine($"Invalid namespaces loaded {listResult.Count()} tools");
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public async Task VerifyUniqueToolNames_InAllMode()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start", "--mode", "all");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var toolNames = listResult.Select(t => t.Name).ToList();
        var uniqueNames = toolNames.Distinct().ToList();

        Assert.Equal(toolNames.Count, uniqueNames.Count);

        Output.WriteLine($"Verified {toolNames.Count} unique tool names in all mode");
    }

    [Fact]
    public async Task VerifyUniqueToolNames_InDefaultMode()
    {
        // Arrange
        await using var client = await CreateClientAsync("server", "start");

        // Act
        var listResult = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var toolNames = listResult.Select(t => t.Name).ToList();
        var uniqueNames = toolNames.Distinct().ToList();

        Assert.Equal(toolNames.Count, uniqueNames.Count);

        Output.WriteLine($"Verified {toolNames.Count} unique tool names in default (namespace) mode");
    }

    #endregion
}
