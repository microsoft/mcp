// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(Id = "723c42ee-186d-4dfb-bc81-8437257f190d", Name = "memories-search", Title = "Search Memories", Description = "Search knowledge base documents.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class MemoriesSearchCommand(ILogger<MemoriesSearchCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<MemoriesSearchOptions>
{
    private readonly ILogger<MemoriesSearchCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentPortedOptionDefinitions.Query);
    }

    protected override MemoriesSearchOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Query = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.Query.Name);
        return o;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var o = BindOptions(parseResult);
        try
        {
            var endpoint = await ResolveEndpointAsync(_sreAgentService, o, cancellationToken);
            var results = await _sreAgentService.SearchMemoriesAsync(endpoint, o.Query!, 10, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(o.Query!, results));
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
