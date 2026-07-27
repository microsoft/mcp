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
    Id = "06255dae-7848-45f9-8cfc-b48bed1fe763",
    Name = "memories_add",
    Title = "Add Memory",
    Description = "Add a document to the SRE Agent knowledge base by name. Uploads markdown content that will be indexed for RAG-based knowledge retrieval.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class MemoriesAddCommand(ILogger<MemoriesAddCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MemoriesAddOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<MemoriesAddCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MemoriesAddOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var safe = SreAgentPortedCommandHelpers.SanitizeFileName(options.Name);
            var file = safe.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? safe : $"{safe}.md";
            await _sreAgentService.UploadMemoryAsync(endpoint, file, options.Content, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ Memory '{file}' added to knowledge base. It will be available for RAG retrieval after indexing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding memory");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
