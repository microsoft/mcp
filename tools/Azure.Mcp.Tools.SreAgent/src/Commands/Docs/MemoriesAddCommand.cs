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

[CommandMetadata(Id = "06255dae-7848-45f9-8cfc-b48bed1fe763", Name = "memories-add", Title = "Add Memory", Description = "Upload markdown content to the SRE Agent knowledge base.", Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, Secret = false, LocalRequired = false)]
public sealed class MemoriesAddCommand(ILogger<MemoriesAddCommand> logger, ISreAgentService sreAgentService) : SreAgentDataPlaneCommand<MemoriesAddOptions>
{
    private readonly ILogger<MemoriesAddCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Content);
    }

    protected override MemoriesAddOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name);
        o.Content = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Content.Name);
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
            var safe = SreAgentPortedCommandHelpers.SanitizeFileName(o.Name!);
            var file = safe.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? safe : $"{safe}.md";
            await _sreAgentService.UploadMemoryAsync(endpoint, file, o.Content!, o.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Memory '{file}' added to knowledge base. It will be available for RAG retrieval after indexing.");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding memory"); HandleException(context, ex); }
        return context.Response;
    }
}
