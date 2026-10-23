// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Server;

namespace Azure.Mcp.Tools.Insights.Services;

public interface ISamplingService
{
    /// <summary>
    /// Sends a single text prompt to the host LLM via MCP sampling and returns the
    /// concatenated text content of the response, or <c>null</c> if the response had no
    /// text content.
    /// </summary>
    Task<string?> SampleTextAsync(
        McpServer mcpServer,
        string systemPrompt,
        string userPrompt,
        int maxTokens,
        CancellationToken cancellationToken);
}
