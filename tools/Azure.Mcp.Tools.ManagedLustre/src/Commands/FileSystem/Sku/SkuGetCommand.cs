// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.Sku;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.Sku;

[CommandMetadata(
    Id = "43f679ba-1b6e-4851-9315-f8ad16b789e5",
    Name = "get",
    Title = "Get AMLFS SKU information",
    Description = "Retrieves the available Azure Managed Lustre SKU, including increments, bandwidth, scale targets and zonal support. If a location is specified, the results will be filtered to that location.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SkuGetCommand(IManagedLustreService service, ILogger<SkuGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SkuGetOptions, SkuGetCommand.SkuGetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<SkuGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SkuGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var skus = await _service.SkuGetInfoAsync(options.Subscription!, options.Tenant, options.Location, options.RetryPolicy, cancellationToken);

            context.Response.Results = ResponseResult.Create(new(skus ?? []), ManagedLustreJsonContext.Default.SkuGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AMLFS SKU info.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record SkuGetResult(List<Models.ManagedLustreSkuInfo> Skus);
}
