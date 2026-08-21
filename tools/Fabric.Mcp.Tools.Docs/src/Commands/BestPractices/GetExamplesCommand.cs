// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

[CommandMetadata(
    Id = "3efdeea3-ee84-43e7-b7a9-c4accb03795a",
    Name = "api-examples",
    Title = "API Examples",
    Description = "Retrieves example API request and response files for a Microsoft Fabric item type. Use this when the user needs sample API calls or implementation examples. Returns a dictionary of example files with their contents.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetExamplesCommand(IFabricPublicApiService service, ILogger<GetExamplesCommand> logger)
    : AuthenticatedCommand<ItemTypeOptions, GetExamplesCommand.ExampleFileResult>
{
    private readonly ILogger<GetExamplesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemTypeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var availableExamples = await _service.GetExamplesAsync(options.ItemType, cancellationToken);

            context.Response.Results = ResponseResult.Create(new(availableExamples), FabricJsonContext.Default.ExampleFileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting examples for item type {ItemType}", options.ItemType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ExampleFileResult(IDictionary<string, string> Examples);
}
