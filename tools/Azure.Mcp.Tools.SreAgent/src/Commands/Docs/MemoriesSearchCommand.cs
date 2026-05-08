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
public sealed class MemoriesSearchCommand(ILogger<MemoriesSearchCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<MemoriesSearchOptions>
{
    private readonly ILogger<MemoriesSearchCommand> _logger = logger; private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); command.Options.Add(SreAgentPortedOptionDefinitions.Query); }
    protected override MemoriesSearchOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); o.Query = parseResult.GetValueOrDefault<string>(SreAgentPortedOptionDefinitions.QueryName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try { var json = await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, $"/api/v1/AgentMemory/documents?query={Uri.EscapeDataString(o.Query!)}&k=10", HttpMethod.Get, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); var results = SreAgentPortedCommandHelpers.DeserializeArray(json, SreAgentJsonContext.Default.ListMemorySearchResult); SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(o.Query!, results)); }
        catch (Exception ex) { _logger.LogError(ex, "Error searching memories"); HandleException(context, ex); }
        return context.Response;
    }
    private static string Format(string query, List<MemorySearchResult> results) { if (results.Count == 0) return $"No documents matched query: \"{query}\""; var lines = new List<string> { $"# Search Results for \"{query}\"", string.Empty, $"{results.Count} result(s)", string.Empty }; foreach (var r in results) { var score = r.Score.HasValue ? $" (relevance: {SreAgentPortedCommandHelpers.Percent(r.Score.Value)}%)" : string.Empty; lines.Add($"### {r.FileName ?? "unknown"}{score}"); if (!string.IsNullOrWhiteSpace(r.Content)) lines.Add(r.Content.Length > 500 ? r.Content[..500] + "..." : r.Content); lines.Add(string.Empty); } return string.Join('\n', lines); }
}
