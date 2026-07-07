// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(
    Id = "723c42ee-186d-4dfb-bc81-8437257f190d",
    Name = "memories_search",
    Title = "Search Memories",
    Description = "Search the SRE Agent knowledge base. Uses semantic search to find relevant documents stored in the agent's knowledge base.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MemoriesSearchCommand(ILogger<MemoriesSearchCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MemoriesSearchOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<MemoriesSearchCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MemoriesSearchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var results = await _sreAgentService.SearchMemoriesAsync(endpoint, options.Query, 10, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(options.Query, results));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error searching memories"); HandleException(context, ex); }
        return context.Response;
    }

    private static string Format(string query, List<MemorySearchResult> results)
    {
        if (results.Count == 0)
            return $"No documents matched query: \"{query}\"";
        var lines = new List<string> { $"# Search Results for \"{query}\"", string.Empty, $"{results.Count} result(s)", string.Empty };
        foreach (var r in results)
        {
            lines.Add($"### {r.Title ?? r.Id ?? "unknown"}");
            if (!string.IsNullOrWhiteSpace(r.Contents))
                lines.Add(r.Contents.Length > 500 ? r.Contents[..500] + "..." : r.Contents);
            lines.Add(string.Empty);
        }
        return string.Join('\n', lines);
    }
}
