// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(Id = "387de9fa-0d29-4b44-b43c-c7d328a751d4", Name = "memories_list", Title = "List Memories", Description = "List knowledge base documents.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class MemoriesListCommand(ILogger<MemoriesListCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<MemoryRemoteOptions>
{
    private readonly ILogger<MemoriesListCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            var docs = await _sreAgentService.ListMemoriesAsync(endpoint, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(docs));
        }
        catch (Exception ex) { _logger.LogError(ex, "Error listing memories"); HandleException(context, ex); }
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
