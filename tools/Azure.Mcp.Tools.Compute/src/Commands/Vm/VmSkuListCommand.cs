// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

/// <summary>
/// Command to list VM SKUs with capabilities and availability zone placement.
/// </summary>
public sealed class VmSkuListCommand(ILogger<VmSkuListCommand> logger)
    : SubscriptionCommand<VmSkuListOptions>()
{
    private const string CommandTitle = "List VM SKUs";
    private readonly ILogger<VmSkuListCommand> _logger = logger;

    public override string Id => "d4c2e8f1-5a3b-4d7c-9e8f-1a2b3c4d5e6f";

    public override string Name => "list-skus";

    public override string Description =>
        """
        List available Virtual Machine (VM) SKUs for a subscription and location. Returns SKU details including name, tier, size, family, capabilities (vCPUs, memory, GPU, etc.), availability zones, and any restrictions. Use the --location parameter to filter SKUs available in a specific Azure region. Use the optional --sku parameter to filter by exact SKU name (e.g., Standard_D2s_v3).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Location);
        command.Options.Add(ComputeOptionDefinitions.Sku);
    }

    protected override VmSkuListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Sku.Name);
        return options;
    }

        public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        var computeService = context.GetService<IComputeService>();

        try
        {
            var skus = await computeService.ListVmSkusAsync(
                options.Subscription!,
                options.Location!,
                options.Sku,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmSkuListResult(skus),
                ComputeJsonContext.Default.VmSkuListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VM SKUs. Location: {Location}, Subscription: {Subscription}, Options: {@Options}",
                options.Location, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed listing VM SKUs. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmSkuListResult(List<VmSkuInfo> Skus);
}
