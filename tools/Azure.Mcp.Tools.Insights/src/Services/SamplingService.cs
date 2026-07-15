// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Azure.Mcp.Tools.Insights.Services;

/// <inheritdoc cref="ISamplingService"/>
public sealed class SamplingService : ISamplingService
{
    public async Task<string?> SampleTextAsync(
        McpServer mcpServer,
        string systemPrompt,
        string userPrompt,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mcpServer);
        ArgumentException.ThrowIfNullOrEmpty(userPrompt);

        var request = new CreateMessageRequestParams
        {
            MaxTokens = maxTokens,
            SystemPrompt = string.IsNullOrEmpty(systemPrompt) ? null : systemPrompt,
            Messages =
            [
                new SamplingMessage
                {
                    Role = Role.User,
                    Content = [new TextContentBlock { Text = userPrompt }],
                }
            ],
        };

        var response = await mcpServer.SampleAsync(request, cancellationToken);
        var content = response?.Content;
        if (content == null || content.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (var block in content)
        {
            if (block is TextContentBlock text && !string.IsNullOrEmpty(text.Text))
            {
                builder.Append(text.Text);
            }
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }
}
