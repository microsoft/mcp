// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(Id = "ff881ee4-ee6e-4fae-8114-c6c6f36745bb", Name = "memories-reindex", Title = "Reindex Memories", Description = "Trigger a knowledge base reindex.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class MemoriesReindexCommand(ILogger<MemoriesReindexCommand> logger, ISreAgentService sreAgentService) : BaseSreAgentCommand<MemoryRemoteOptions>
{
    private readonly ILogger<MemoriesReindexCommand> _logger = logger; private readonly ISreAgentService _sreAgentService = sreAgentService;
    protected override void RegisterOptions(Command command) { base.RegisterOptions(command); command.Options.Add(SreAgentPortedOptionDefinitions.Agent); }
    protected override MemoryRemoteOptions BindOptions(ParseResult parseResult) { var o = base.BindOptions(parseResult); o.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.AgentNameName); return o; }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid) return context.Response; var o = BindOptions(parseResult);
        try { await _sreAgentService.CallAgentDataPlaneAsync(o.Subscription!, o.Agent!, o.ResourceGroup, "/api/v1/AgentMemory/rebuildIndex", HttpMethod.Post, tenant: o.Tenant, retryPolicy: o.RetryPolicy, cancellationToken: cancellationToken); SreAgentPortedCommandHelpers.SetTextResult(context.Response, "✅ Knowledge base reindex triggered. This may take a few minutes to complete."); }
        catch (Exception ex) { _logger.LogError(ex, "Error reindexing memories"); HandleException(context, ex); }
        return context.Response;
    }
}
