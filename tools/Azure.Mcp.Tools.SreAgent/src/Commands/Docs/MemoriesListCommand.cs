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

[CommandMetadata(Id = "387de9fa-0d29-4b44-b43c-c7d328a751d4", Name = "memories-list", Title = "List Memories", Description = "List knowledge base documents.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class MemoriesListCommand(ILogger<MemoriesListCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<MemoryRemoteOptions>
{
    private readonly ILogger<MemoriesListCommand> _logger = logger; private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); }
    protected override MemoryRemoteOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try { var json = await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, "/api/v1/AgentMemory/files", HttpMethod.Get, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); var docs = SreAgentPortedCommandHelpers.DeserializeArray(json, SreAgentJsonContext.Default.ListDocumentInfo); SreAgentPortedCommandHelpers.SetTextResult(context.Response, Format(docs)); }
        catch (Exception ex) { _logger.LogError(ex, "Error listing memories"); HandleException(context, ex); }
        return context.Response;
    }
    private static string Format(List<DocumentInfo> docs) { if (docs.Count == 0) return "No documents found in knowledge base."; var lines = new List<string> { "# Knowledge Base Documents", string.Empty, $"{docs.Count} document(s)", string.Empty }; foreach (var d in docs) { var name = d.Name ?? d.FileName ?? "unnamed"; var size = d.Size is > 0 ? $" ({d.Size.Value / 1024.0:0.0} KB)" : string.Empty; lines.Add($"- **{name}**{size}"); } return string.Join('\n', lines); }
}
