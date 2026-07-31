// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(
    Id = "387de9fa-0d29-4b44-b43c-c7d328a751d4",
    Name = "memories_list",
    Title = "List Memories",
    Description = "Retrieve a complete list of all indexed knowledge base documents stored in an SRE Agent's memory. Returns all document names and metadata without any search filter or query. Use this to browse everything in the knowledge base before searching.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MemoriesListCommand(ILogger<MemoriesListCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseSreAgentOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<MemoriesListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseSreAgentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var docs = await _sreAgentService.ListMemoriesAsync(endpoint, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(docs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing memories");
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static string Format(List<DocumentInfo> docs)
    {
        if (docs.Count == 0)
            return "No documents found in knowledge base.";
        var lines = new List<string> { "# Knowledge Base Documents", string.Empty, $"{docs.Count} document(s)", string.Empty };
        foreach (var d in docs)
        {
            var name = d.Name ?? d.FileName ?? "unnamed";
            var size = d.Size is > 0 ? $" ({d.Size.Value / 1024.0:0.0} KB)" : string.Empty;
            lines.Add($"- **{name}**{size}");
        }
        return string.Join('\n', lines);
    }
}
