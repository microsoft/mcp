// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;


namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

public sealed class SkuGetCommand(ILogger<SkuGetCommand> logger)
    : BaseManagedLustreCommand<SkuGetOptions>(logger)
{
    private const string CommandTitle = "Get AMLFS SKU information";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves the available Azure Managed Lustre SKU, including increments, bandwidth, scale targets and zonal support. If a location is specified, the results will be filtered to that location.
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
        command.Options.Add(ManagedLustreOptionDefinitions.LocationOption.AsOptional());
    }

    protected override SkuGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.LocationOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var options = BindOptions(parseResult);
            var service = context.GetService<IManagedLustreService>();
            var skus = await service.SkuGetInfoAsync(options.Subscription!, options.Tenant, options.Location, options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(skus ?? []), ManagedLustreJsonContext.Default.SkuGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AMLFS SKU info.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record SkuGetResult(List<Models.ManagedLustreSkuInfo> Skus);
}
