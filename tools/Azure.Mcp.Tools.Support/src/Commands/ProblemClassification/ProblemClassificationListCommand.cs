// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Support.Models;
using Azure.Mcp.Tools.Support.Options.ProblemClassification;
using Azure.Mcp.Tools.Support.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Support.Commands.ProblemClassification;

[CommandMetadata(
    Id = "e3d7d58f-6fd7-4296-95bd-df3bfff742be",
    Name = "list",
    Title = "List Azure Support Problem Classifications",
    Description = """
        List the problem classifications for an Azure support service. Returns each problem classification's name,
        full ID, and display name. The full ID is the problem classification value required to create an Azure
        support ticket. Use the service list tool first to find the service name.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ProblemClassificationListCommand(ILogger<ProblemClassificationListCommand> logger, ISupportService supportService)
    : AuthenticatedCommand<ProblemClassificationListOptions, ProblemClassificationListCommand.ProblemClassificationListCommandResult>
{
    private readonly ILogger<ProblemClassificationListCommand> _logger = logger;
    private readonly ISupportService _supportService = supportService;

    public override JsonTypeInfo<ProblemClassificationListCommandResult>? ResultTypeInfo => SupportJsonContext.Default.ProblemClassificationListCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProblemClassificationListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var classifications = await _supportService.ListProblemClassificationsAsync(options.Service, options.Tenant, cancellationToken);
            SetResult(context, new(classifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Azure support problem classifications. Service: {Service}.", options.Service);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ProblemClassificationListCommandResult(List<SupportProblemClassification> ProblemClassifications);
}
